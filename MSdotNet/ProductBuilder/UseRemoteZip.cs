//**********************************************************************
//***** Copyright (c) 1992-2013 Allen Systems Group, Inc.  All rights reserved.
//**********************************************************************
//*****$Workfile:   UseRemoteZip.cs  $
//***** $Archive:   I:\Archives\CS\1\UseRemoteZip.c~  $
//*****$Revision:   1.1  $
//*****  $Author:   KOFORNAG  $
//*****    $Date:   18 Sep 2007 15:03:48  $
//***** $Modtime:   18 Sep 2007 15:04:30  $
//*****     $Log:   I:\Archives\CS\1\UseRemoteZip.c~  $
//*****
//*****   Rev 1.1   18 Sep 2007 15:03:48   KOFORNAG
//*****   Merge of Project FWEfficientBuildZipFTP
//*****
//*****   Rev 1.0   17 Sep 2007 15:38:20   KOFORNAG
//*****   Initialize Archive
//*****  $Endlog$
//**********************************************************************
using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;


namespace UseRemoteZip {

	class UseRemoteZip {

        static string PLINK_COMMAND = @"P:\ProductBuilder\tools2\vendors\putty\plink";
        static string LOCAL_CHECKSUM_COMMAND = @"P:\ProductBuilder\tools2\vendors\unix_utils\md5sum -b";
        static string PLINK_OPTIONS = "-batch -ssh";
        static string REMOTE_CHECKSUM_COMMAND = "md5sum -b";
        static string REMOTE_COPY_COMMAND = "cp";
        static string LOG_FILE = @"c:\VMSTEMP\buildtest.log";
        static string FTP_BATCH_FILE = @"C:\VMSTEMP\ftpBatch.bat";
        static string PLINK_CONN_STRING = "";
        static string ZIP_CATALOG_FILENAME = @"Q:\build_zip_catalog.txt";
        static string CAT_ZIP_COMMAND = @"P:\ProductBuilder\tools2\vendors\info-zip\zip -m";
        static string CREATE_CAT_COMMAND = @"P:\ProductBuilder\tools2\vendors\info-zip\unzip -l";
        static string REMOTE_CAT_UNZIP_COMMAND = "unzip -p";
        static string LOCAL_CAT_UNZIP_COMMAND = @"P:\ProductBuilder\tools2\vendors\info-zip\unzip -p";

        [STAThread]
		static void Main(string[] args) {

            bool remoteCopyExists = false;
            try {
                if (args.Length != 2) {
                    Console.WriteLine("Wrong number of arguments supplied.");
                    Usage();
                }
                else {
                    string completeFtpCommandString = args[0];
                    string remoteReferenceFileLocation =  args[1];

                    //parse username and password from command string
                    string userName = GetArgValue("/usr", completeFtpCommandString);
                    string userPwd = GetArgValue("/pwd", completeFtpCommandString);
                    string serverName = GetArgValue("/svr", completeFtpCommandString);
                    string localFileLocation = GetLocalFilePath(completeFtpCommandString);
                    string remoteFileLocation = GetRemoteFilePath(completeFtpCommandString);

                    if ((userName.Length < 1) || (userPwd.Length < 1))
                        throw new Exception("Missing username or password.");
 
                    //Compute plink connection information
                    StringBuilder plinkCmd = new StringBuilder();
                    plinkCmd.Append(PLINK_COMMAND + " ");     //Command
                    plinkCmd.Append(PLINK_OPTIONS + " ");     //options   
                    plinkCmd.Append(" -l " + userName);       //user name 
                    plinkCmd.Append(" -pw " + userPwd + " "); //password
                    plinkCmd.Append(serverName + " ");        //server
                    PLINK_CONN_STRING = plinkCmd.ToString();

                    //append zip catalog to the payload
                    AppendZipCatalog(localFileLocation);

                    string remoteFileCmdString = PLINK_CONN_STRING + " " + REMOTE_CAT_UNZIP_COMMAND + " " + remoteFileLocation + " " + Path.GetFileName(ZIP_CATALOG_FILENAME) + " | " +  PLINK_CONN_STRING + " " + REMOTE_CHECKSUM_COMMAND +  " 1>" + LOG_FILE + " 2>&1";
                    string localFileCmdString = LOCAL_CAT_UNZIP_COMMAND + " " +  localFileLocation + " " + Path.GetFileName(ZIP_CATALOG_FILENAME) + " | " + LOCAL_CHECKSUM_COMMAND + " 1>" + LOG_FILE + " 2>&1";
                    string referenceFileCmdString = PLINK_CONN_STRING + " " + REMOTE_CAT_UNZIP_COMMAND + " " + remoteReferenceFileLocation + " " + Path.GetFileName(ZIP_CATALOG_FILENAME) + " | " +  REMOTE_CHECKSUM_COMMAND +  " 1>" + LOG_FILE + " 2>&1";
 
                    string localHashString = GetHashString(localFileCmdString);
                    //Console.WriteLine("Local hash: " + localHashString); 
                    //Console.WriteLine("Remote hash: " + GetHashString(remoteFileCmdString)); 
                    if (RemoteFileExists(remoteFileLocation) && (GetHashString(remoteFileCmdString) == localHashString))
                        remoteCopyExists = true;
                    else if (RemoteFileExists(remoteReferenceFileLocation) && (GetHashString(referenceFileCmdString)) == localHashString) {
                        remoteCopyExists = CopyFromExport(remoteReferenceFileLocation, remoteFileLocation);
                    }
                    else {
                        remoteCopyExists = false;
                    }
  
                    string currMsg;
                    if (remoteCopyExists)
                        currMsg = "Equivalent file exists at server...skipping " + localFileLocation + " tranfer.";
                    else
                        currMsg = "Equivalent file not found at server...tranferring " + localFileLocation + " to " + remoteFileLocation;

                    Console.WriteLine("Command arg = " + args[0] + " " +  args[1]);
                    Console.WriteLine(currMsg);
                }
            }
            catch(Exception currExcept){
                remoteCopyExists = false;
                Console.WriteLine("Unable to check remote file status for build.  Please see error:");
                Console.WriteLine("\t" + currExcept.Message);
            }
            if (remoteCopyExists)
                Environment.ExitCode = 1;
            else
                Environment.ExitCode = 0;
        }

        static void Usage(){
            Console.WriteLine("Usage: FtpBuildFileIf completeFtpCommandString remoteReferenceFileLocation");   
            Console.WriteLine();
            Console.WriteLine("completeFtpCommandString:");
            Console.WriteLine("                 The complete ftp command that will be sent if file must be FTP'ed");
            Console.WriteLine("remoteReferenceFileLocation:");
            Console.WriteLine("                 Location of a remote file that can be used instead FTP'ing current file");
        }

        static string GetArgValue(string token, string commandString){
            string returnValue = "";
            
            int tokenStart = commandString.IndexOf(token);
            if (tokenStart >= 0) {
                StringBuilder strBuff = new StringBuilder();
                while ((tokenStart < commandString.Length)
                    && (commandString[tokenStart] != ':')) 
                    tokenStart++;
                tokenStart++;
                while ((tokenStart < commandString.Length)
                    && (commandString[tokenStart] != ' ')
                    && (commandString[tokenStart] != ':'))
                    strBuff.Append(commandString[tokenStart++]);

                returnValue = strBuff.ToString();
            }
            return returnValue;
        }


        static string GetLocalFilePath(string commandString){
            //this would be the first file after the "/put" option
            StringBuilder returnValue = new StringBuilder();
            //Get the location of string after /put
            int tokenStart = commandString.IndexOf("/put") + 4;
            while ((tokenStart < commandString.Length) && (commandString[tokenStart] == ' ')) {tokenStart++;};
            while ((tokenStart < commandString.Length) && (commandString[tokenStart] != ' ')) {
                returnValue.Append(commandString[tokenStart++]);
            }
            return returnValue.ToString();
        }


        static string GetRemoteFilePath(string commandString){
            //this would be the second file after the "/put" option
            StringBuilder returnValue = new StringBuilder();
            //Get the location of the second space delimited string after /put
            int tokenStart = commandString.IndexOf("/put") + 4;
            while ((tokenStart < commandString.Length) && (commandString[tokenStart] == ' ')) {tokenStart++;};
            //skip the first string
            while ((tokenStart < commandString.Length) && (commandString[tokenStart] != ' ')) {tokenStart++;};
            //and the second space
            while ((tokenStart < commandString.Length) && (commandString[tokenStart] == ' ')) {tokenStart++;};
            while ((tokenStart < commandString.Length) && (commandString[tokenStart] != ' ')) {
                returnValue.Append(commandString[tokenStart++]);
            }
            return returnValue.ToString();
        }


        /// <returns>Returns exit code of the application.</returns>
        static string GetHashString(string commandString) {

            string returnValue = "";
            
            RunCommand(commandString);
            //read output file to search for hash code
            using (StreamReader currReader = File.OpenText(LOG_FILE)) {
                string line = currReader.ReadToEnd();
                if (line.Length > 0) {
                    line = line.TrimStart(new char[] {' ', '/', '\\', '*'}) ;
                    returnValue = line.Substring(0, line.IndexOf(' '));
                }
                currReader.Close();
            }
            return returnValue;
        }


        static bool CopyFromExport(string source, string destination){
            bool returnValue = false;

            string remoteCopyCmd = PLINK_CONN_STRING + " " + REMOTE_COPY_COMMAND +  " " + source + " " + destination + " 1>" + LOG_FILE + " 2>&1";
            RunCommand(remoteCopyCmd);
            //read output file to search for success indicator, which should be an empty file
            using (StreamReader currReader = File.OpenText(LOG_FILE)) {
                string line = currReader.ReadToEnd().Trim(new char[] {' ', '/', '\\', '*'}) ;
                returnValue = (line.Length == 0);
                currReader.Close();
            }
            if (returnValue)
                returnValue = RemoteFileExists(destination);

            return returnValue;
        }


        static bool RemoteFileExists(string remoteFilePath) {
            bool returnValue = false;
            //Watch out! Path.Get..() replaces UNIX's "/" with Windows "\\", we must restore
            string remoteFileDir = Path.GetDirectoryName(remoteFilePath).Replace("\\", "/");
            string cmdString = PLINK_CONN_STRING + " ls " + remoteFileDir + " 1>" + LOG_FILE + " 2>&1";
            RunCommand(cmdString);

            //search for the file in the log file
            using (StreamReader currReader = File.OpenText(LOG_FILE)) {
                string line = currReader.ReadToEnd().Trim(new char[] {' ', '/', '\\', '*'}) ;
                returnValue = (line.ToLower().IndexOf(Path.GetFileName(remoteFilePath.Trim(new char[] {' ', '/', '\\', '*'}).ToLower())) >= 0);
                currReader.Close();
            }
            return returnValue;
        }


        static void RunCommand(string commandString){
            
            if (!Directory.Exists(Path.GetDirectoryName(FTP_BATCH_FILE)))
                Directory.CreateDirectory(Path.GetDirectoryName(FTP_BATCH_FILE));

            using (StreamWriter currWriter = File.CreateText(FTP_BATCH_FILE)){
                currWriter.WriteLine(commandString);
                currWriter.Flush();
                currWriter.Close();
            }
            using (Process buildProcess = new Process()) {
                buildProcess.StartInfo.FileName =  FTP_BATCH_FILE;
                buildProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                buildProcess.Start();
                buildProcess.WaitForExit(); 
                int exitCode = buildProcess.ExitCode;
                buildProcess.Close();
                if (exitCode!= 0) {
                    throw new Exception("LibraryBuilder process returned: [" + exitCode.ToString()  + "] while processing =>" + commandString);
                }
            }
        }


        static void AppendZipCatalog(string zipPath){
            string unzipCmd = CREATE_CAT_COMMAND + " " + zipPath + " > " + ZIP_CATALOG_FILENAME;
            string zipCmd = CAT_ZIP_COMMAND + " " + zipPath + " " + ZIP_CATALOG_FILENAME;
            RunCommand(unzipCmd);
            RunCommand(zipCmd);
        }

    }
}
