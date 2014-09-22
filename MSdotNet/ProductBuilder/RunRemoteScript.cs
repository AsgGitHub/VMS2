//**********************************************************************
//***** Copyright (c) 1992-2013 Allen Systems Group, Inc.  All rights reserved.
//**********************************************************************
//*****$Workfile:   RunRemoteScript.cs  $
//***** $Archive:   I:\Archives\CS\2\RunRemoteScript.c~  $
//*****$Revision:   2.1  $
//*****  $Author:   KOFORNAG  $
//*****    $Date:   12 Oct 2006 16:57:26  $
//***** $Modtime:   12 Oct 2006 16:57:22  $
//*****     $Log:   I:\Archives\CS\2\RunRemoteScript.c~  $
//*****
//*****   Rev 2.1   12 Oct 2006 16:57:26   KOFORNAG
//*****   Merge of Project EISplitProductBuilder
//*****
//*****   Rev 2.0   11 Oct 2006 11:52:04   KOFORNAG
//*****   Create Branch Revision.
//*****
//*****   Rev 1.2   Sep 05 2003 16:15:26   RRUSSELL
//*****Merge of Project EIPRDBLD-Misc36
//*****
//*****   Rev 1.1   Jul 01 2003 13:17:48   RRUSSELL
//*****Merge of Project EIPRDBLD-SupportZOS
//*****
//*****   Rev 1.0   Apr 28 2003 10:58:18   RRUSSELL
//*****Initialize Archive
//*****  $Endlog$
//**********************************************************************

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Timers;


namespace ProductBuilderTools {

	class RunRemoteScript {

        static int errlvl = 0;
        static int plinkErrorBase = 5120;
        static int errorBase = 4096;
        static int Error_Generic                    = errorBase + 0;
        static int Error_BadServerArg               = errorBase + 1;
        static int Error_BadUsernameArg             = errorBase + 2;
        static int Error_BadPasswordArg             = errorBase + 3;
        static int Error_BadCommandFileArg          = errorBase + 4;
        static int Error_BadCommandLine             = errorBase + 5;
        static int Error_BadRC                      = errorBase + 6;
        static int Error_LoginFailure               = errorBase + 7;
        static int Error_ClientHung                 = errorBase + 8;
        static int Error_MissingPlink               = errorBase + 9;

        static bool alive = false;
        static Process proc = null;

		[STAThread]
        static int Main(string[] args) {

            Timer timer = null;
            
            try {

                // Process Command-line Arguments
                string server = "";
                int port = 23;
                string username = "";
                string password = "";
                string remoteScriptCommandLine = "";
                string commandFile = "";
                string plinkFile = "P:\\ProductBuilder\\tools2\\vendors\\PuTTY\\plink.exe";
                bool useCommandFile = false;
    
                foreach ( string arg in args ) {
                    if ( arg.StartsWith("/svr:") )   server             = arg.Substring(5).Trim();
                    if ( arg.StartsWith("/port:") )  port               = Convert.ToInt32(arg.Substring(6).Trim());
                    if ( arg.StartsWith("/usr:") )   username           = arg.Substring(5).Trim();
                    if ( arg.StartsWith("/pwd:") )   password           = arg.Substring(5).Trim();
                    if ( arg.StartsWith("/plink:") ) plinkFile          = arg.Substring(7).Trim();
                    if ( arg.StartsWith("/cmd:") ) {  
                        if ( arg.StartsWith("/cmd:@") ) {
                            useCommandFile = true;
                            commandFile        = arg.Substring(6).Trim();
                        }
                        else {
                            remoteScriptCommandLine = arg.Substring(5).Trim();
                        }
                    }
                }
                if ( server=="" )   { BadParam(); return(Error_BadServerArg); }
                if ( username=="" ) { BadParam(); return(Error_BadUsernameArg); }
                if ( password=="" ) { BadParam(); return(Error_BadPasswordArg); }

                if ( useCommandFile ) {
                    if ( !File.Exists( commandFile ) ) { BadParam(); return(Error_BadCommandFileArg); }
                
                    remoteScriptCommandLine = "";
                    StreamReader commandFileReader = null;
                    try {
                        commandFileReader = new StreamReader( commandFile );
                        remoteScriptCommandLine=commandFileReader.ReadLine();
                    }
                    finally {
                        if (commandFileReader!=null) commandFileReader.Close();
                    }
                }

                if ( remoteScriptCommandLine == "" ) { BadParam(); return(Error_BadCommandLine); }
                if ( !File.Exists( plinkFile ) ) { BadParam(); return(Error_MissingPlink); }
    
                Console.WriteLine();
                Console.WriteLine("server:       " + server);
                Console.WriteLine("port:         " + port);
                Console.WriteLine("username:     " + username);
                Console.WriteLine("commandFile:  " + commandFile);
                Console.WriteLine("commandLine:  " + remoteScriptCommandLine);
                Console.WriteLine();


                //poll for frozen client app
                timer = new Timer();
                timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
                timer.AutoReset = true;
                timer.Interval = 10800000; // 3 hours
                timer.Enabled = true;
 
                // run the build batch file
                proc = new Process();
                proc.StartInfo.FileName = plinkFile;
                proc.StartInfo.Arguments = "-telnet " + server + " -P " + port.ToString();
                proc.StartInfo.CreateNoWindow = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardInput = true;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.WorkingDirectory = "P:\\ProductBuilder";
                proc.Start();
                StringBuilder currentLine = new StringBuilder();
                int currentByte=0;
                char currentChar=' ';
                string currentString = "";
                bool sentLogin = false;
                bool sentPassword = false;
                bool inLoginFailure = false;
                bool loginFailure = false;
                bool sendingCommand = false;
                bool sentCommand = false;
                bool sendingEchoRC = false;
                bool sentEchoRC = false;
                bool sendingExit = false;
                bool sentExit = false;
                bool parsingRC = false;
                bool parsedRC = false;
                while ( !proc.HasExited && (currentByte=proc.StandardOutput.Read())!=-1 && !loginFailure ) {
                    alive = true;
                    currentChar = Convert.ToChar( currentByte );
                    if ( currentChar!='\r' && currentChar!='\n' ) {
                        currentLine.Append( currentChar );
                    }
                    currentString = currentLine.ToString();
                    
                    if ( !sentLogin && currentString.IndexOf("login: ")>=0 ) {
                        sentLogin = true;
                        proc.StandardInput.Write(username);proc.StandardInput.Write('\r');//proc.StandardInput.Write('\n');
                    }
                    if ( sentLogin && !sentPassword && currentString.IndexOf("Password: ")>=0 ) {
                        sentPassword = true;
                        proc.StandardInput.Write(password);proc.StandardInput.Write('\r');//proc.StandardInput.Write('\n');
                    }
                    if ( (sentLogin || sentPassword) && !sentCommand && 
                        ( currentString=="Login incorrect" || 
                          currentString.IndexOf("You entered an invalid login name or password")>=0 ||
                          currentString.IndexOf("YOU LOGGED IN USING ALL UPPERCASE CHARACTERS")>=0 ) ) {
                        inLoginFailure = true;
                        errlvl = Error_LoginFailure;
                    }
                    if ( sentPassword && !sendingCommand && !sentCommand && currentString.IndexOf("$ ")>=0 ) {
                        sendingCommand = true;
                        proc.StandardInput.Write(remoteScriptCommandLine);proc.StandardInput.Write('\n');
                    }
                    if ( sentCommand && !sendingEchoRC && !sentEchoRC && currentString.IndexOf("$ ")>=0 ) {
                        sendingEchoRC = true;
                        proc.StandardInput.Write("echo $?");proc.StandardInput.Write('\n');
                    }
                    if ( sentEchoRC && !sendingExit && !sentExit && currentString.IndexOf("$ ")>=0 ) {
                        sendingExit = true;
                        proc.StandardInput.Write("exit");proc.StandardInput.Write('\n');
                    }

                    if ( currentChar == '\n' ) {
                        if (sentEchoRC && !parsingRC && !parsedRC) {
                            parsingRC = true;
                        }
                        if (inLoginFailure) {
                            inLoginFailure = false;
                            loginFailure = true;
                        }
                        if (sendingCommand) {
                            //workaround for other Unix OS's since z/OS requires an additional CR when logging in
                            if ( !currentString.EndsWith("$ ") ) {
                                sendingCommand = false;
                                sentCommand = true;
                            }
                        }
                        if (sendingEchoRC) {
                            sendingEchoRC = false;
                            sentEchoRC = true;
                        }
                        if (sendingExit) {
                            sendingExit = false;
                            sentExit = true;
                        }
                        if (parsingRC) {
                            parsingRC = false;
                            parsedRC = true;
                            int tempRC = Error_BadRC;
                            try {
                                string tempRCString = currentString.Trim();
                                tempRC = (tempRCString==null) ? 0 : Convert.ToInt32(tempRCString);
                            }
                            catch (Exception ex) {
                                tempRC = Error_BadRC;
                                ex.GetType(); //dummy to suppress compiler warning
                            }
                            errlvl = tempRC;
                        }
                        Console.WriteLine( currentString );
                        currentLine = new StringBuilder();
                    }
                }
/*
                
                while ( (currentByte=proc.StandardOutput.Read())!=-1 ) {
                    alive = true;
                    currentChar = Convert.ToChar( currentByte );
                    if ( currentChar!='\r' && currentChar!='\n' ) {
                        currentLine.Append( currentChar );
                    }
                    currentString = currentLine.ToString();                    
                    if ( currentChar == '\n' ) {
                        Console.WriteLine( currentString );
                        currentLine = new StringBuilder();
                    }   
                }
*/
                if ( !proc.WaitForExit(5*1000) ) { // 5 seconds
                    proc.Kill();
                }

                if (errlvl==0 && proc.ExitCode!=0)
                    errlvl = plinkErrorBase + proc.ExitCode;
                proc.Close();

            }
            catch ( Exception e ) {

                errlvl = Error_Generic;
                Console.Error.WriteLine("An error occurred in RunRemoteScript.exe");
                Console.Error.WriteLine(e.ToString());
                Usage();

            }
            finally {
                if ( proc!=null )
                    proc.Close();
                if ( timer!=null )
                    timer.Close();

            }

            return(errlvl);

        }


        public static void OnTimedEvent(object source, ElapsedEventArgs e) {
            if ( !alive ) {
                errlvl = Error_ClientHung;
                proc.Kill();
            }
            alive = false;
        }



        static void BadParam() {
            Console.WriteLine();
            Console.Error.WriteLine("Missing or Invalid Parameter.");
            Usage(); 
        }

        static void Usage() {
        
            Console.WriteLine();
            Console.WriteLine("Usage: RunRemoteScript.exe /svr:server [ /port:port ] /usr:username");
            Console.WriteLine("                /pwd:password [ /cmd:command | /cmd:@commandfile ]");
            Console.WriteLine("                [ /plink:plinkfile ]");
            Console.WriteLine();
            Console.WriteLine("        return code is that of remote process, except for ranges starting");
            Console.WriteLine("        with 4096:");
            Console.WriteLine();
            Console.WriteLine("            4096 - 4105   RunRemoteScript.exe errorcode");
            Console.WriteLine("            5120+         plink.exe errorcode + 5120");
            Console.WriteLine();
        }
    }
}

