//**********************************************************************
//***** Copyright (c) 1992-2013 Allen Systems Group, Inc.  All rights reserved.
//**********************************************************************
//*****$Workfile:   ProductBuild.cs  $
//***** $Archive:   I:\Archives\CS\2\ProductBuild.c~  $
//*****$Revision:   2.3  $
//*****  $Author:   KOFORNAG  $
//*****    $Date:   10 Aug 2007 17:02:38  $
//***** $Modtime:   10 Aug 2007 17:02:46  $
//*****     $Log:   I:\Archives\CS\2\ProductBuild.c~  $
//*****
//*****   Rev 2.3   10 Aug 2007 17:02:38   KOFORNAG
//*****   Merge of Project EIMigrateToASGMail1
//*****
//*****   Rev 2.2   22 Nov 2006 14:56:46   KOFORNAG
//*****   Merge of Project EIBranchProjectsPb
//*****
//*****   Rev 2.1   13 Sep 2006 08:29:02   KOFORNAG
//*****   Merge of Project EIBranchLibraryBuilder
//*****
//*****   Rev 2.0   12 Sep 2006 17:43:28   KOFORNAG
//*****   Create Branch Revision.
//*****
//*****   Rev 1.13   29 Jun 2005 18:20:36   DMASKOFF
//*****   Merge of Project EIVMSPBHandleValidationFailure2
//*****
//*****   Rev 1.12   24 Jun 2005 17:01:14   DMASKOFF
//*****   Merge of Project FIVMSLBValidationFromPB
//*****
//*****   Rev 1.11   16 Jun 2005 17:56:02   DMASKOFF
//*****   DMASKOFF - Merge of Project EIVMSLBLinkedProjectsSupport
//*****
//*****   Rev 1.10   04 May 2005 17:28:06   TGALASSO
//*****Merge of Project FILBUnderscoreExt
//*****
//*****   Rev 1.9   09 Dec 2004 19:22:08   DMASKOFF
//*****Merge of Project EIVMSPBStoreBuildHostName
//*****
//*****   Rev 1.8   23 Sep 2004 18:07:50   KOFORNAG
//*****Merge of Project FIPreprodPartialBuilds
//*****
//*****   Rev 1.7   03 Sep 2004 17:30:14   KOFORNAG
//*****Merge of Project EILbCommandLine
//*****
//*****   Rev 1.6   25 May 2004 12:28:50   KOFORNAG
//*****Merge of Project EILbOptimize
//*****
//*****   Rev 1.5   Aug 22 2003 13:41:32   RRUSSELL
//*****Merge of Project FIBuildReportingSystem
//*****
//*****   Rev 1.4   Aug 12 2003 16:58:04   RRUSSELL
//*****Merge of Project EIBuildReportingSystem
//*****
//*****   Rev 1.3   Apr 19 2003 15:34:18   RRUSSELL
//*****Merge of Project EIPRDBLD-Misc28
//*****
//*****   Rev 1.2   Apr 18 2003 16:36:36   RRUSSELL
//*****Merge of Project EIPRDBLD-IntegrateWithLB2
//*****
//*****   Rev 1.1   11 Mar 2003 13:47:34   RRUSSELL
//*****Merge of Project EILIBBLD-IntegrateWithPB
//*****
//*****   Rev 1.0   04 Mar 2003 10:51:12   RRUSSELL
//*****Initialize Archive
//*****  $Endlog$
//**********************************************************************

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Web.UI;
using System.Net.Mail;
using System.Threading;
using LibraryBuilder;



namespace LibraryBuilder {

    public class ProductBuild {

        ProductBuildDataSet pbDataSet;
        DateTime startTime;
        DateTime completeTime;
        StringCollection buildTimes;
        string buildLogfile;
        object buildLogfileLock;
        LibraryManifest libraryManifest;
        bool isValidationSuccessful = true;
	    
        public const int BUILDSTATUS_SUCCEEDED  = 0;
        public const int BUILDSTATUS_WARNINGS   = 1;
        public const int BUILDSTATUS_FAILED     = 2;
        public const int BUILDSTATUS_INCOMPLETE = 3;


        public ProductBuild(LibraryManifest currManifest, string buildPath) {
            buildLogfile = "";
            buildLogfileLock = new Object();
            buildTimes = new StringCollection();
            libraryManifest = currManifest;

            string buildDefinitionFile = ProductBuildDataSet.GetBuildConfigurationFilename( buildPath );
            pbDataSet = ProductBuildDataSet.ReadBuildConfigurationFile( buildDefinitionFile );
        }

        public int Run(NewMessage userMsg, bool isQuietMode) {
            int buildErrorLevel = BUILDSTATUS_INCOMPLETE;
            isValidationSuccessful = true;

            try {
                startTime = DateTime.Now;
                //must not build on net path   
                if (LibraryManifest.IsNetworkPath(pbDataSet.GetBuildProperty("BuildPath"))){
                    string errorMsg = "Builds are not allowed on VMS product directories; ";
                    errorMsg += "please select a different build path and try again.";
                    throw new Exception(errorMsg);
                }

                // register build start in BUILD_STATUS
                RegisterBuildStart();

                // allow log to be overwritten
                SetBuildLogfile( pbDataSet.GetBuildProperty("BuildLogFileName") );
                if ( File.Exists( GetBuildLogfile() ) ){
                    File.SetAttributes( GetBuildLogfile(), FileAttributes.Normal );
                    File.Delete(GetBuildLogfile());
                    FileStream logFile = File.Create(GetBuildLogfile());
                    logFile.Close();
                }

                if ( !pbDataSet.SourcePathEqualsBuildPath() && !userMsg.IsCancelled()) {
                    try {
                        ValidateLibrary(userMsg, isQuietMode);
                    }
                    catch (Exception e) {
                        isValidationSuccessful = false;
                        throw e;
                    }
                }
                
                if (!userMsg.IsCancelled())
                    buildErrorLevel = PerformBuild(userMsg,isQuietMode);
            }
            catch (Exception e) {
                buildErrorLevel = BUILDSTATUS_FAILED;
                throw e;
            }
            finally {
                completeTime = DateTime.Now;

                // register build finish in BUILD_STATUS
                RegisterBuildFinish( buildErrorLevel );

                SaveReport(buildErrorLevel,userMsg,isQuietMode);

                SendEmail( buildErrorLevel );
            }

            return buildErrorLevel;
        }

        public void RunBatch(string batcFileName, string cmdArgs, string workingDir, 
            bool isQuietMode, int waitTime, NewMessage userMessage) {

            if (userMessage.Cancelled) return;

            if (cmdArgs == null) cmdArgs = "";
            cmdArgs += " 2>&1 | " + LibraryBuilderConfig.GetLogProcessPath()+ " " + userMessage.UniqueKey + " >> " + GetBuildLogfile();
                
            using (Process buildProcess = new Process()) {
                buildProcess.StartInfo.FileName =  batcFileName;
                buildProcess.StartInfo.Arguments = cmdArgs;
                buildProcess.StartInfo.WorkingDirectory = workingDir;
                buildProcess.StartInfo.CreateNoWindow = false;
                buildProcess.StartInfo.UseShellExecute = false;
                buildProcess.Start();
				
                userMessage.MonitorCancell(buildProcess.Id);

                buildProcess.WaitForExit(waitTime);
                buildProcess.Close();
            }
        }
        

        /// <returns>Returns exit code of the application.</returns>
        private int RunBatchWithInternalOutputHandling(string batchFileName, string cmdArgs, string workingDir, 
            bool isQuietMode, int waitTime, NewMessage userMessage) {

            if (userMessage.Cancelled) return 0;

            // RunBatch() pipes output of the app to LogMsg.exe (which will post it 
            // to the message queue). A consequence of this approach is that the exit code 
            // that the Process object returns after the process completes is always 0.
            // (Perhaps the last app that terminates is LogMsg, and its exit code is 0.)
            //
            // This method can be used when getting the exit code is important.
            // For example, when we run validation, we need to terminate the build if
            // validation fails.
            //
            // To resolve the problem, we will manually read app output, and post it
            // to the message queue.
            // When the app exits, we will append all collected output to build log file.
            // (RunBatch() also redirects output to the build log file)

            if (cmdArgs == null) cmdArgs = "";
            cmdArgs += " 2>&1";

            using (Process buildProcess = new Process()) {
                buildProcess.StartInfo.FileName =  batchFileName;
                buildProcess.StartInfo.Arguments = cmdArgs;
                buildProcess.StartInfo.WorkingDirectory = workingDir;
                buildProcess.StartInfo.CreateNoWindow = false;
                buildProcess.StartInfo.UseShellExecute = false;

                buildProcess.StartInfo.RedirectStandardInput  = false;
                buildProcess.StartInfo.RedirectStandardError  = false;
                buildProcess.StartInfo.RedirectStandardOutput = true;
                buildProcess.Start();
				
                userMessage.MonitorCancell(buildProcess.Id);

                string processOutput = "";
                while (true) {
                    String outputLine = buildProcess.StandardOutput.ReadLine();
                    if (outputLine == null) break;

                    processOutput += ( outputLine + "\r\n" );
                    userMessage.SendCopyMessage(outputLine, 0, 0);	
                }

                bool exited = buildProcess.WaitForExit(waitTime);

                int exitCode;
                if (!exited) { 
                    buildProcess.Kill();
                    exitCode = -1;
                }
                else {
                    exitCode = buildProcess.ExitCode;
                }

                buildProcess.Close();

                string logFilePath = GetBuildLogfile();
                Console.WriteLine("Echoing output to " + logFilePath);
                using (StreamWriter writer = new StreamWriter(logFilePath, true)) {
                    writer.WriteLine(processOutput);
                    writer.Close();
                }

                return exitCode;
            }
        }


        private void ValidateLibrary(NewMessage userMsg, bool isQuietMode) {

            string buildTimeEntry = DateTime.Now + " - Starting Validate.";
            buildTimes.Add( buildTimeEntry );
            Console.WriteLine( buildTimeEntry );

            string buildPath = pbDataSet.GetBuildProperty("BuildPath");
            string sourcePath = pbDataSet.GetBuildProperty("SourcePath");

            // save build configuration file
            string buildDefinitionFile = ProductBuildDataSet.GetBuildConfigurationFilename( buildPath );
            string tempFile = ProductBuildDataSet.GetBuildConfigurationFilename( Path.GetTempPath() );
            if ( File.Exists( tempFile ) ) {
                File.SetAttributes( tempFile, FileAttributes.Normal );
                File.Delete( tempFile );
            }
            string tempBuildDefinitionDirectory = Path.Combine( Path.GetPathRoot( tempFile ), Path.GetDirectoryName( tempFile ) );
            if ( !Directory.Exists( tempBuildDefinitionDirectory ) ) Directory.CreateDirectory( tempBuildDefinitionDirectory );
            File.Move( buildDefinitionFile, tempFile);

			
            // copy build definition file into buildpath
            string buildDefinitionDirectory = Path.Combine( Path.GetPathRoot( buildDefinitionFile ), Path.GetDirectoryName( buildDefinitionFile ) );
            if ( Directory.Exists( buildDefinitionDirectory ) ) Directory.Delete( buildDefinitionDirectory, true );
            Directory.CreateDirectory( buildDefinitionDirectory );
            File.Move( tempFile, buildDefinitionFile );

            string errorMsg = null;
            if (false == CanBuildOnPath(sourcePath,buildPath, ref errorMsg))
                throw new Exception(errorMsg);

            string buildLibName;
            string createDate;
            if (LibraryManifest.IsNetworkPath(sourcePath)) {
                buildLibName =  "NETWORK_" + libraryManifest.GetBranchOrTrunkName()+ "_BUILD";
                createDate = new DateTime(DatabaseInterface.GetCurrentServerTime()).ToString();
            }
            else {
                buildLibName =  "LOCAL_" + libraryManifest.GetBranchOrTrunkName()+ "_BUILD";
                createDate = libraryManifest.GetDate();
            }


            // Attempt to run LB three times. Try the first time in quiet mode.
            // Disable "quiet mode" on a retry, hoping to get information 
            // useful to determine the source of problem.
            string quietModeArg = "/q";
            int lbExitCode = 0;
            int attemptCount = 0;
            string validateBatchFilePath = buildPath + "\\validate.bat";

            while (true) {
                string validateCmd = LibraryBuilderConfig.GetLbCommandLinePath() + " /c " + quietModeArg + " /nocopy /src:" + sourcePath + " /path:" + buildPath;
                validateCmd += " /name:" + buildLibName + " /date:\"" + createDate +"\"";
            
                //create bat file in buildPath and save
                StreamWriter cmdWriter = new StreamWriter(validateBatchFilePath, false);
                cmdWriter.WriteLine(validateCmd);
                cmdWriter.WriteLine("@set LB__EXIT__CODE=%ERRORLEVEL%");
                cmdWriter.WriteLine("@echo Library validation: LB exited with code: %LB__EXIT__CODE%");
                cmdWriter.WriteLine("@exit %LB__EXIT__CODE%");
                cmdWriter.Close();
            
                lbExitCode = RunBatchWithInternalOutputHandling( validateBatchFilePath, null, buildPath, isQuietMode, (1*60*60*1000), userMsg );
                Console.WriteLine("LB exited with code: " + lbExitCode);

                if (lbExitCode != 0 && ++attemptCount < 3) {
                    Console.WriteLine("LB failed - retrying...");
                    quietModeArg = "";
                    continue;
                }

                break;
            }

            File.Delete(validateBatchFilePath);

            if (lbExitCode != 0) {
                Console.WriteLine("Unable to validate library, terminating build...");
                throw new Exception("Unable to validate library. Please see build log for details.");
            }

            // refresh LibraryInfo file with current datetime
            LibraryManifest lif = new LibraryManifest( 
                pbDataSet.GetBuildProperty("ProductName"),
                pbDataSet.GetBuildProperty("ProductVersion"),
                pbDataSet.GetBuildProperty("ProductPlatform"),
                pbDataSet.GetBuildProperty("BranchOrTrunkName"),
                pbDataSet.GetBuildProperty("ProductDirectory"),
                pbDataSet.GetBuildProperty("LibraryDate") );
            lif.SaveToPath( pbDataSet.GetBuildProperty("BuildPath") );

            buildTimeEntry = DateTime.Now + " - Validate Complete.";
            if (pbDataSet.IsPartialBuild()) {
                string binCopyCmd = Path.Combine(pbDataSet.GetBuildProperty("PBToolsPath"),
                    "CopyBinaries.bat");
                string copyArgs =  " " + sourcePath + " " + buildPath;
                RunBatch(binCopyCmd,copyArgs,buildPath,isQuietMode,(1*60*60*1000),userMsg);
            }
            buildTimes.Add( buildTimeEntry );
            Console.WriteLine( buildTimeEntry );
        }


        public static bool CanBuildOnPath(string sourcePath, string buildPath, ref string errorMsg){
			
            bool returnValue = true;

            if (LibraryManifest.IsNetworkPath(buildPath)) {
                returnValue = false;
                errorMsg = "Builds are not allowed on VMS product paths; ";
                errorMsg += "please select a different path and try again.";
            }
            else {
                if (sourcePath.ToLower() != buildPath.ToLower()) { 
                    LibraryManifest libManifest = new LibraryManifest(sourcePath);
                    string buildLibName = (LibraryManifest.IsNetworkPath(sourcePath))? "NETWORK_" : "LOCAL_";
                    buildLibName += libManifest.GetBranchOrTrunkName() + "_BUILD";
                    //Abort if the path is a registered path of a non-build lib
                    VmsLibrary[] libList = LibraryBuilderConfig.GetLibrariesWithElementValue(LibraryElementNames.libraryPath,buildPath);
                    if (libList != null) {
                        bool canUsePath = false;
                        foreach (VmsLibrary currLib in libList) {
                            if (currLib.Name.ToLower() == buildLibName.ToLower()) 
                                canUsePath = true;
                        }
                        if (false == canUsePath) {
                            returnValue = false;
                            errorMsg = "A local library already exists on the selected build path; ";
                            errorMsg += "please select a different path and try again.";
                        }
                    }
                }
            }
            return returnValue;
        }

        private int PerformBuild(NewMessage userMsg, bool isQuietMode) {

            int buildErrorLevel = 0;

            string buildTimeEntry = DateTime.Now + " - Starting Build.";
            buildTimes.Add( buildTimeEntry );
            Console.WriteLine( buildTimeEntry );   

            // make sure build_temp exists
            string buildTempDirectory = Path.Combine( pbDataSet.GetBuildProperty("BuildPath"), "build_temp" );
            if ( !Directory.Exists( buildTempDirectory ) ) Directory.CreateDirectory( buildTempDirectory );       

            // run the build batch file
            string buildCommandLineArgs = pbDataSet.GetBuildProperty("BuildDefinitionFileName");
            RunBatch(pbDataSet.GetBuildProperty("BuildBatchFileName"),
                buildCommandLineArgs,
                pbDataSet.GetBuildProperty("BuildPath"),isQuietMode,
                (8*60*60*1000),userMsg);

            string buildCompleteTimeEntry = DateTime.Now + " - Build Finished.";
            Console.WriteLine( buildCompleteTimeEntry );

            // add entries into buildTimes from build
            string buildTimesFilename = pbDataSet.GetBuildProperty("BuildTimesFileName");
            if ( File.Exists( buildTimesFilename ) ) {
                StringCollection tempBuildTimes = CollectionUtil.ReadValueFile( pbDataSet.GetBuildProperty("BuildTimesFileName") );
                foreach ( string line in tempBuildTimes )
                    buildTimes.Add(line);
            }
            buildTimes.Add( buildCompleteTimeEntry );

            // get success information
            bool warning = false;
            bool error = false;
            bool complete = false;
            char[] colon = new char[] {':'};
            string status;
            string[] tokens;

            string buildStepStatusFilename = pbDataSet.GetBuildProperty("BuildStepStatusFileName");
            StringCollection buildStepStatus = new StringCollection();
            if ( File.Exists( buildStepStatusFilename ) ) {
                buildStepStatus = CollectionUtil.ReadValueFile( buildStepStatusFilename );
            }

            foreach ( string line in buildStepStatus ) {
                status = null;
                tokens = line.Split( colon );
                if ( tokens.Length > 1 )
                    status = tokens[1].ToLower().Trim();
                if ( status=="error" ) error = true;
                if ( status=="warning" ) warning = true;
                if ( status=="complete" ) complete = true;
            }
            if (!complete) {
                buildErrorLevel = 3;
            }
            else if (error) {
                buildErrorLevel = 2;
            }
            else if (warning) {
                buildErrorLevel = 1;
            }
            Console.WriteLine( GetBuildErrorLevelString( buildErrorLevel ) );

            return buildErrorLevel;

        }


        private void SendEmail( int buildErrorLevel ) {

            DatabaseInterface dbInterface = new DatabaseInterface();

            string toRecipients;
            string[] tempRecipients = CollectionUtil.StringToStringArray( pbDataSet.GetBuildProperty("EmailRecipients"), new char[] {',', ';'} );
            StringCollection processedList = new StringCollection();
            foreach( string recipient in tempRecipients ) {
                if ( recipient==null || recipient.Trim() == "" ) {
                    continue;
                }
                    // don't add "@mobius.com" if "@" already specified
                else if ( recipient.IndexOf("@") >= 0 ) {
                    processedList.Add( recipient.Trim() );
                }
                else {
                    string validRecipient = dbInterface.GetUserEmail(recipient).Trim();
                    if (validRecipient.Length > 0)
                        processedList.Add( validRecipient.Trim() + "@asg.com" );
                }
            }
            toRecipients = CollectionUtil.CollectionToString( processedList, ';' );

            SmtpClient client = new SmtpClient("SMTPServerName");
            string sender = dbInterface.GetUserEmail(Environment.UserName).Trim();
            sender = (sender.Length > 0) ? sender + "@asg.com" : Environment.UserName;
            MailMessage msg = new MailMessage(new MailAddress(sender), new MailAddress(toRecipients));                
            string subject = "Build Report for " + pbDataSet.GetBuildProperty("ProductName") + " " + 
                pbDataSet.GetBuildProperty("ProductVersion") + " " + 
                pbDataSet.GetBuildProperty("ProductPlatform") + " ";
            string branchOrTrunkName = pbDataSet.GetBuildProperty("BranchOrTrunkName");
            if ( branchOrTrunkName.ToUpper() != "DEVELOP" )
                subject += branchOrTrunkName + " ";
            string releaseOrDebug = pbDataSet.GetBuildProperty("ReleaseOrDebug");
            if (releaseOrDebug!=null) {
                if ( (releaseOrDebug.ToUpper() == "RELEASE" && branchOrTrunkName.ToUpper() == "DEVELOP") ||
                    (releaseOrDebug.ToUpper() == "DEBUG" && branchOrTrunkName.ToUpper() != "DEVELOP") ) {
                    subject += releaseOrDebug + " ";
                }
            }

            if ( buildErrorLevel < 2 ) 
                subject += "(Build Succeeded)";
            else
                subject += "(Build Failed)";

            // message body
            string body;
            StringBuilder bodyText = new StringBuilder();
            bodyText.Append( "\r\n" );
            bodyText.Append( "Build Machine:            " + Environment.MachineName + "\r\n" );
            bodyText.Append( "Build User:               " + Environment.UserDomainName + "\\" + Environment.UserName + "\r\n" );
            bodyText.Append( "Build Start Time:         " + startTime + "\r\n" );
            bodyText.Append( "Build Completion Time:    " + completeTime + "\r\n" );
            bodyText.Append( "Build Status:             " + GetBuildErrorLevelString( buildErrorLevel ) + "\r\n" );
            bodyText.Append( "Library Date:             " + pbDataSet.GetBuildProperty("LibraryDate") + "\r\n" );
            bodyText.Append ("-------------------------------------------------------------------------\r\n");

            if (isValidationSuccessful) {
                bodyText.Append( "\r\nBuild Times:\r\n\r\n" );
                string buildTimesFilename = pbDataSet.GetBuildProperty("BuildTimesFileName");
                StringCollection buildTimes = new StringCollection();
                if ( File.Exists( buildTimesFilename ) ) {
                    buildTimes = CollectionUtil.ReadValueFile( buildTimesFilename );
                }
                else {
                    buildTimes.Add( "-- The Build Times Output File, " + buildTimesFilename + ", is Missing --" );
                }
                foreach ( string line in buildTimes )
                    bodyText.Append ( line + "\r\n" );
                bodyText.Append ("-------------------------------------------------------------------------\r\n");

                bodyText.Append( "\r\nBuild Step Results:\r\n\r\n" );
                string buildStepStatusFilename = pbDataSet.GetBuildProperty("BuildStepStatusFileName");
                StringCollection buildStepStatus = new StringCollection();
                if ( File.Exists( buildStepStatusFilename ) ) {
                    buildStepStatus = CollectionUtil.ReadValueFile( pbDataSet.GetBuildProperty("BuildStepStatusFileName") );
                }
                else {
                    buildStepStatus.Add( "-- The Build Step Status Output File, " + buildStepStatusFilename + ", is Missing --" );
                }
                if ( pbDataSet.GetBuildProperty("Populate")!=null && pbDataSet.GetBuildProperty("Populate").ToLower() == "true" ) {
                    buildStepStatus = ChangeLogFilePaths( buildStepStatus, pbDataSet.GetBuildProperty("BuildPath"), pbDataSet.GetBuildProperty("PopulatePath") );
                }
                foreach ( string line in buildStepStatus )
                    bodyText.Append ( line + "\r\n" );
                bodyText.Append ("-------------------------------------------------------------------------\r\n");
            
                bodyText.Append( "\r\nCheckBuild Results:\r\n\r\n" );
                string checkBuildResultsFilename = pbDataSet.GetBuildProperty("CheckBuildResultsFileName");
                StringCollection checkBuildResults = new StringCollection();
                if ( File.Exists( checkBuildResultsFilename ) ) {
                    checkBuildResults = CollectionUtil.ReadValueFile( checkBuildResultsFilename );
                }
                else {
                    checkBuildResults.Add( "-- The CheckBuild Output File, " + checkBuildResultsFilename + ", is Missing --" );
                }
                foreach ( string line in checkBuildResults ) {
                    bodyText.Append ( line + "\r\n" );
                }
            }
            else {
                // unsuccessful validation
                bodyText.Append( "\r\nLibraryBuilder was unable to create the local library on the build machine." );
                bodyText.Append( "\r\nThe build could not start." );
                bodyText.Append( "\r\nIf you get this error consistently, please contact VMS Administrator." );
                bodyText.Append( "\r\n\r\nValidation output file: file://" + GetBuildLogfile() );
                bodyText.Append( "\r\n\r\n" );
            }

            bodyText.Append ("-------------------------------------------------------------------------\r\n");
            
            bodyText.Append( "\r\nBuild Properties:\r\n\r\n" );
            SortedList buildProperties = pbDataSet.GetBuildProperties();
            foreach ( object key in buildProperties.Keys ) {
                if ( ((string)key).ToUpper() != "UNIXPASSWORD" ) {
                    string keyString = (string)key;
                    string valueString = buildProperties[key].ToString();
                    bodyText.Append( keyString + "=" + valueString + "\r\n" );
                }
            }
            bodyText.Append ("-------------------------------------------------------------------------\r\n");

            body = bodyText.ToString();
            // -- end message body

            MailMessage buildEmail = new MailMessage(new MailAddress(sender), new MailAddress(toRecipients));
            buildEmail.Subject = subject;
            buildEmail.Body = body;

            client.Send( buildEmail );
        }


        private void SaveReport( int buildErrorLevel, NewMessage userMsg, bool isQuietMode ) {

            string saveBuildReport = pbDataSet.GetBuildProperty("SaveBuildReport");
            if ( saveBuildReport==null || saveBuildReport.ToUpper() != "TRUE" ) {
                return;
            }

            string buildName = GetBuildName();
            string buildNameDirectory = Path.Combine( LBEnvironment.BuildReportRootDirectory, buildName );
            string buildDateDirectory = Path.Combine( buildNameDirectory, startTime.ToString( "yyyyMMdd_HHmmss" ) );

            StringBuilder buildReportStringBuilder = new StringBuilder();
            HtmlTextWriter reportWriter = new HtmlTextWriter( new StringWriter( buildReportStringBuilder ) );

            reportWriter.WriteLine("<HTML><BODY><PRE>");

            reportWriter.WriteLine( "Build Name:               " + buildName );
            reportWriter.WriteLine( "Build Machine:            " + Environment.MachineName );
            reportWriter.WriteLine( "Build User:               " + Environment.UserDomainName + "\\" + Environment.UserName );
            reportWriter.WriteLine( "Build Start Time:         " + startTime );
            reportWriter.WriteLine( "Build Completion Time:    " + completeTime );
            reportWriter.WriteLine( "Build Status:             " + GetBuildErrorLevelString( buildErrorLevel ) );
            reportWriter.WriteLine( "Library Date:             " + pbDataSet.GetBuildProperty("LibraryDate") );
            reportWriter.WriteLine( "-------------------------------------------------------------------------");
            reportWriter.WriteLine();
            
            if (isValidationSuccessful) {            
                reportWriter.WriteLine( "Build Times:" );
                reportWriter.WriteLine();
                string buildTimesFilename = pbDataSet.GetBuildProperty("BuildTimesFileName");
                StringCollection buildTimes = new StringCollection();
                if ( File.Exists( buildTimesFilename ) ) {
                    buildTimes = CollectionUtil.ReadValueFile( buildTimesFilename );
                }
                else {
                    buildTimes.Add( "-- The Build Times Output File, " + buildTimesFilename + ", is Missing --" );
                }
                foreach ( string line in buildTimes )
                    reportWriter.WriteLine( line );
                reportWriter.WriteLine("-------------------------------------------------------------------------" );

                reportWriter.WriteLine();
                reportWriter.WriteLine( "Build Step Results:" );
                reportWriter.WriteLine();
                string buildStepStatusFilename = pbDataSet.GetBuildProperty("BuildStepStatusFileName");
                StringCollection buildStepStatus = new StringCollection();
                if ( File.Exists( buildStepStatusFilename ) ) {
                    buildStepStatus = CollectionUtil.ReadValueFile( buildStepStatusFilename );
                    buildStepStatus = ChangeToReportFormat( buildStepStatus, buildDateDirectory );
                }
                else {
                    buildStepStatus.Add( "-- The Build Step Status Output File, " + buildStepStatusFilename + ", is Missing --" );
                }
                foreach ( string line in buildStepStatus ) {
                    reportWriter.WriteLine( line );
                }
                reportWriter.WriteLine("-------------------------------------------------------------------------" );
            
                reportWriter.WriteLine();
                reportWriter.WriteLine( "CheckBuild Results:"  );
                reportWriter.WriteLine();
                string checkBuildResultsFilename = pbDataSet.GetBuildProperty("CheckBuildResultsFileName");
                StringCollection checkBuildResults = new StringCollection();
                if ( File.Exists( checkBuildResultsFilename ) ) {
                    checkBuildResults = CollectionUtil.ReadValueFile( checkBuildResultsFilename );
                }
                else {
                    checkBuildResults.Add( "-- The CheckBuild Output File, " + checkBuildResultsFilename + ", is Missing --" );
                }
                foreach ( string line in checkBuildResults )
                    reportWriter.WriteLine( line );
                reportWriter.WriteLine("-------------------------------------------------------------------------" );
            
                reportWriter.WriteLine();
                reportWriter.WriteLine( "Build Properties:"  );
                reportWriter.WriteLine();
                SortedList buildProperties = pbDataSet.GetBuildProperties();
                foreach ( object key in buildProperties.Keys ) {
                    if ( ((string)key).ToUpper() != "UNIXPASSWORD" ) {
                        string keyString = (string)key;
                        string valueString = buildProperties[key].ToString();
                        reportWriter.WriteLine( keyString + "=" + valueString );
                    }
                }
            }
            else {
                // unsuccessful validation
                reportWriter.WriteLine( "LibraryBuilder was unable to create the local library on the build machine." );
                reportWriter.WriteLine( "The build could not start." );
                reportWriter.WriteLine( "If you get this error consistently, please contact VMS Administrator." );
                reportWriter.WriteLine();

                string msg = "Validation output file: Error: file://" + GetBuildLogfile();
                StringCollection coll = new StringCollection();
                coll.Add(msg);
                coll = ChangeToReportFormat( coll, buildDateDirectory );
                if (coll.Count > 0) msg = coll[0];

                reportWriter.WriteLine( msg );
                reportWriter.WriteLine();
            }
                
            reportWriter.WriteLine("-------------------------------------------------------------------------" );

            reportWriter.WriteLine("</PRE></HTML></BODY>");
            reportWriter.Close();

            Directory.CreateDirectory( buildDateDirectory );
            string buildReportFile = Path.Combine( buildDateDirectory, "BuildReport.htm" );
            StreamWriter sw = File.CreateText( buildReportFile );
            sw.Write( buildReportStringBuilder.ToString() );
            sw.Close();

            // copy logs
            // If validation failed, copy only the single log with LB's output.
            string copylogsScript = ( isValidationSuccessful ? "CopyLogs.bat" : "CopyMainLog.bat" );
            string buildPath = pbDataSet.GetBuildProperty("BuildPath");
            RunBatch( Path.Combine(pbDataSet.GetBuildProperty("PBToolsPath"), copylogsScript),
                buildPath + " \"" + buildDateDirectory + "\"",
                buildPath,isQuietMode, (1*60*60*1000),userMsg);

        }

        private StringCollection ChangeLogFilePaths( StringCollection buildStepStatus, string buildPath, string populatePath ) {
            
            StringCollection newBuildStepStatus = new StringCollection();

            string newline;
            foreach ( string line in buildStepStatus ) {
                newline = line.Replace( buildPath, populatePath );
                newBuildStepStatus.Add( newline );
            }

            return newBuildStepStatus;

        }


        private StringCollection ChangeToReportFormat( StringCollection buildStepStatus, string logBaseDirectory ) {
            
            StringCollection newBuildStepStatus = new StringCollection();

            string newline;
            string pathToReplace = " file://" + pbDataSet.GetBuildProperty("BuildPath") + "\\";
            foreach ( string line in buildStepStatus ) {
                try {
                    string[] splitLine = line.Split( new Char[]{':'}, 3 );
                    string link = splitLine[2].Replace( pathToReplace, "" );
                    string hardLink = splitLine[2].Replace( pathToReplace, logBaseDirectory + "\\" );
                    newline = "<A HREF=\"" + link + "\">" + splitLine[0] + "</A>:" + splitLine[1] + " ( " + hardLink + " )" ; 
                    newBuildStepStatus.Add( newline );
                }
                catch (Exception e) {
                    e.GetType();
                }
            }

            return newBuildStepStatus;

        }


        private void RegisterBuildStart() {

            string saveBuildReport = pbDataSet.GetBuildProperty("SaveBuildReport");
            if ( saveBuildReport==null || saveBuildReport.ToUpper() != "TRUE" ) {
                return;
            }

            string buildName = GetBuildName();

            SqlConnection currentConnection = null;
            try {
                currentConnection = new SqlConnection( DatabaseUtil.sqlConnectionString2 );
                currentConnection.Open();

                // add entry if it doesn't exist
                string addEntryCmdText  = 
                  "insert into BUILD_STATUS " +
                  " ( BUILD_NAME, PRODUCT_NAME, PRODUCT_VERSION, PLATFORM_CODE, BUILD_START_TIME, BUILD_COMPLETE_TIME, STATUS, LAST_GOOD_BUILD_START_TIME, BUILD_MACHINE_NAME ) " +
                  "select @build_name, @product_name, @product_version, @platform_code, null, null, null, null, null " +
                  "where not exists ( select 1 from BUILD_STATUS where BUILD_NAME = @build_name )";

                SqlCommand addEntryCmd = currentConnection.CreateCommand();
                addEntryCmd.CommandText = addEntryCmdText;
                addEntryCmd.Parameters.Add("@build_name", SqlDbType.VarChar).Value = buildName;
                addEntryCmd.Parameters.Add("@product_name", SqlDbType.VarChar).Value = pbDataSet.GetBuildProperty("ProductName");
                addEntryCmd.Parameters.Add("@product_version", SqlDbType.VarChar).Value = pbDataSet.GetBuildProperty("ProductVersion");
                addEntryCmd.Parameters.Add("@platform_code", SqlDbType.VarChar).Value = pbDataSet.GetBuildProperty("ProductPlatform");
                addEntryCmd.ExecuteNonQuery();

                // update product info, build host name, start time and status
                string updateCmdText  = 
                  "update BUILD_STATUS " +
                  "set PRODUCT_NAME = @product_name, " +
                      "PRODUCT_VERSION = @product_version, " +
                      "PLATFORM_CODE = @platform_code, " + 
                      "BUILD_START_TIME = @build_start_time, " +
                      "STATUS = 'Incomplete', " +
                      "BUILD_MACHINE_NAME = @build_machine_name " +
                  "where BUILD_NAME = @build_name";

                SqlCommand updateCmd = currentConnection.CreateCommand();
                updateCmd.CommandText = updateCmdText;
                updateCmd.Parameters.Add("@build_name", SqlDbType.VarChar).Value = buildName;
                updateCmd.Parameters.Add("@product_name", SqlDbType.VarChar).Value = pbDataSet.GetBuildProperty("ProductName");
                updateCmd.Parameters.Add("@product_version", SqlDbType.VarChar).Value = pbDataSet.GetBuildProperty("ProductVersion");
                updateCmd.Parameters.Add("@platform_code", SqlDbType.VarChar).Value = pbDataSet.GetBuildProperty("ProductPlatform");
                updateCmd.Parameters.Add("@build_start_time", SqlDbType.DateTime).Value = startTime;
                updateCmd.Parameters.Add("@build_machine_name", SqlDbType.VarChar).Value = Environment.MachineName;
                updateCmd.ExecuteNonQuery();

            }
            catch ( Exception ex ) {
                ex.GetType();
            }
            finally {
                if (currentConnection!=null) currentConnection.Close();
            }

        }

        private void RegisterBuildFinish( int buildErrorLevel ) {

            string saveBuildReport = pbDataSet.GetBuildProperty("SaveBuildReport");
            if ( saveBuildReport==null || saveBuildReport.ToUpper() != "TRUE" ) {
                return;
            }

            string buildName = GetBuildName();
            string status = GetBuildStatusString( buildErrorLevel );

            SqlConnection currentConnection = null;
            try {
                currentConnection = new SqlConnection( DatabaseUtil.sqlConnectionString2 );
                currentConnection.Open();

                // update product info, start time and status
                string updateCmdText  = "update BUILD_STATUS ";
                updateCmdText += "set STATUS = @status, BUILD_COMPLETE_TIME = @build_complete_time ";
                if ( buildErrorLevel == 0 ) {
                    updateCmdText += ", LAST_GOOD_BUILD_START_TIME = BUILD_START_TIME ";
                }
                updateCmdText += "where BUILD_NAME = @build_name ";
                SqlCommand updateCmd = currentConnection.CreateCommand();
                updateCmd.CommandText = updateCmdText;
                updateCmd.Parameters.Add("@build_name", SqlDbType.VarChar).Value = buildName;
                updateCmd.Parameters.Add("@status", SqlDbType.VarChar).Value = status;
                updateCmd.Parameters.Add("@build_complete_time", SqlDbType.DateTime).Value = completeTime;
                updateCmd.ExecuteNonQuery();

            }
            catch ( Exception ex ) {
                ex.GetType();
            }
            finally {
                if (currentConnection!=null) currentConnection.Close();
            }

        }

        private string GetBuildName() {
            string buildName = pbDataSet.GetBuildProperty("ProductDirectory") + " " + 
                pbDataSet.GetBuildProperty("ProductVersion") + " " + 
                pbDataSet.GetBuildProperty("ProductPlatform");
            string branchOrTrunkName = pbDataSet.GetBuildProperty("BranchOrTrunkName");
            buildName += " " + branchOrTrunkName;
            string releaseOrDebug = pbDataSet.GetBuildProperty("ReleaseOrDebug");
            if (releaseOrDebug!=null) {
                if ( (releaseOrDebug.ToUpper() == "RELEASE" && branchOrTrunkName.ToUpper() == "DEVELOP") ||
                    (releaseOrDebug.ToUpper() == "DEBUG" && branchOrTrunkName.ToUpper() != "DEVELOP") ) {
                    buildName += " " + releaseOrDebug;
                }
            }
            return buildName;
        }


        public string GetBuildErrorLevelString( int buildErrorLevel ) {

            string errorString = "";
            
            switch( buildErrorLevel ) {
                case 0:
                    errorString = "The build was successful.";
                    break;
                case 1:
                    errorString = "The build had warnings.";
                    break;
                case 2:
                    errorString = "The build had errors.";
                    break;
                case 3:
                    errorString = "The build did not complete.";
                    break;
            }

            return errorString;

        }

        private string GetBuildStatusString( int buildErrorLevel ) {

            string status = "Failed";
            
            switch( buildErrorLevel ) {
                case BUILDSTATUS_SUCCEEDED:
                    status = "Succeeded";
                    break;
                case BUILDSTATUS_WARNINGS:
                    status = "Warnings";
                    break;
                case BUILDSTATUS_FAILED:
                    status = "Failed";
                    break;
                case BUILDSTATUS_INCOMPLETE:
                    status = "Incomplete";
                    break;
            }

            return status;

        }

        public string GetBuildLogfile() {
            string retval = null ;
            lock( buildLogfileLock ) {
                retval = buildLogfile;
            }
            return retval;
        }

        public void SetBuildLogfile( string setval ) {
            lock( buildLogfileLock ) {
                buildLogfile = setval;
            }
        }
    }
}



