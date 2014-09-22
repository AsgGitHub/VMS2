//**********************************************************************
//***** Copyright (c) 1992-2013 Allen Systems Group, Inc.  All rights reserved.
//**********************************************************************
//*****$Workfile:   ImportComponents.cs  $
//***** $Archive:   I:\Archives\CS\1\ImportComponents.c~  $
//*****$Revision:   1.3  $
//*****  $Author:   KOFORNAG  $
//*****    $Date:   24 Apr 2007 10:23:06  $
//***** $Modtime:   24 Apr 2007 10:23:48  $
//*****     $Log:   I:\Archives\CS\1\ImportComponents.c~  $
//*****
//*****   Rev 1.3   24 Apr 2007 10:23:06   KOFORNAG
//*****   Merge of Project EIComponentImportsWithZip
//*****
//*****   Rev 1.2   05 Mar 2007 09:43:08   KOFORNAG
//*****   Merge of Project EIBuildImportsUpdate
//*****
//*****   Rev 1.1   14 Feb 2007 09:54:54   KOFORNAG
//*****   Merge of Project EIBranchProjectsBI
//*****
//*****   Rev 1.0   04 Jan 2007 14:21:30   KOFORNAG
//*****   Initialize Archive
//*****  $Endlog$
//**********************************************************************
using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Text;
using LibraryBuilder;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace ProductBuilderTools {


    class ImportComponents	{
        enum CopyItemType {FILE, DIRECTORY, WILDCARD, UNIDENTIFIED };
       
        static public int XCopyDirToQDrive(string dirToCopyFrom, string destinationOnQ, bool copySubDirectories){
            int returnValue = 0; 

            int waitTime = 2*60*60*1000; //2 hrs to copy should be more than enough;
            StringBuilder cmdArgs = new StringBuilder(" /D/F/H/Y/R");
            if (copySubDirectories) 
                cmdArgs.Append("/S");
            cmdArgs.Append(" \"" +  dirToCopyFrom + "\" \"" + destinationOnQ + "\"") ;
  
            //output for logs
            Console.WriteLine("xcopy.exe " + cmdArgs.ToString());
            Console.WriteLine(" ");

            using (Process buildProcess = new Process()) {
                buildProcess.StartInfo.FileName =  "xcopy.exe";
                buildProcess.StartInfo.Arguments = cmdArgs.ToString();
                buildProcess.StartInfo.CreateNoWindow = false;
                buildProcess.StartInfo.UseShellExecute = false;
                buildProcess.Start();

                bool exited = buildProcess.WaitForExit(waitTime);
                if (!exited) { 
                    buildProcess.Kill();
                    returnValue = -1;
                }
                else {
                    returnValue = buildProcess.ExitCode;
                }
                buildProcess.Close();
            }

            return returnValue;
        }



        static public void DeleteFiles(string dirToDelete, bool deleteSubDirectories){

            string delCommand = "del /f/q " + dirToDelete;
            
            if (deleteSubDirectories)  
                delCommand = (Path.GetFileName(dirToDelete) == "*.*")? "rd /s/q " :  "del /s/f/q " + dirToDelete;
            
            //output for logs
            Console.WriteLine(" ");
            Console.WriteLine(delCommand);

            int waitTime = 10*60*1000; //10 minutes to delete file;
            using (Process buildProcess = new Process()) {
                buildProcess.StartInfo.FileName =  "cmd.exe";
                buildProcess.StartInfo.Arguments =  " /c " + delCommand;
                buildProcess.StartInfo.CreateNoWindow = false;
                buildProcess.StartInfo.UseShellExecute = false;

                buildProcess.Start();

                bool exited = buildProcess.WaitForExit(waitTime);

                if (!exited)  
                    buildProcess.Kill();
                buildProcess.Close();
            }
            Console.WriteLine(" ");
        }



        [STAThread]
		static int Main(string[] commandArgs) {
            int returnValue = 0;

            try {
                if (commandArgs.Length < 1) { //error, no arg. supplied
                    string errMsg = "No argument supplied.\n";
                           errMsg += "\tUsage: " + Process.GetCurrentProcess().ProcessName + " <Build Import Manifest>\n";
                           errMsg += "\tFor example, " + Process.GetCurrentProcess().ProcessName + " BuildImports.xml";
                    throw new Exception(errMsg);  
                }
                if (!File.Exists(commandArgs[0]))
                    throw new Exception("File not found: " + commandArgs[0]);
                //Get build attributes. Note: This module always expects q:\ to be mapped to the build path by
                //LibraryBuilder prior to invocation.
                LibraryManifest currManifest = new LibraryManifest(LBEnvironment.BuildMapDrive);
                string buildDefinitionFile = ProductBuildDataSet.GetBuildConfigurationFilename(LBEnvironment.BuildMapDrive);
                ProductBuildDataSet pbDataSet = ProductBuildDataSet.ReadBuildConfigurationFile(buildDefinitionFile);
                string buildProfile = pbDataSet.GetBuildProperty("BuildProfileName");

                //Parse the xml file.
                XmlDocument xmlManifestDoc =  new XmlDocument();
                xmlManifestDoc.Load(commandArgs[0]);
                //file may or may not contain version info
                XmlNode versionNode = xmlManifestDoc.DocumentElement.Attributes.GetNamedItem("version");
                int fileVersion = (versionNode == null)? 1 : int.Parse(versionNode.InnerText);
                //version 3 or higher is implemented in LB and uses zip file implementation
                if (fileVersion >= 3) {
                    FileSystemInterface.ValidateComponentImportStep(LBEnvironment.BuildMapDrive,
                        currManifest.GetBranchOrTrunkName(),
                        commandArgs[0],false,true,false);
                }
                else {
                    foreach (XmlNode importNode in xmlManifestDoc.DocumentElement.ChildNodes) { 
                        if (importNode.NodeType != XmlNodeType.Comment) {
                            string prodName = importNode.Attributes["sourceProduct"].InnerText;
                            string prodVer = importNode.Attributes["sourceVersion"].InnerText;
                            string platCode = importNode.Attributes["sourcePlatform"].InnerText;
                            string branchOrTrunkName = importNode.Attributes["sourceBranchOrTrunkName"].InnerText;
                            string itemToCopy = importNode.SelectSingleNode("source").Attributes["path"].InnerText;
                            string destPath = importNode.SelectSingleNode("destination").Attributes["path"].InnerText;;
                            bool copySubdirectories = false;
                            bool cleanAtDestination = false;
                            if (fileVersion > 1) { 
                                copySubdirectories = bool.Parse(importNode.SelectSingleNode("source").Attributes["copySubDirectories"].InnerText.ToLower());
                                cleanAtDestination = bool.Parse(importNode.SelectSingleNode("source").Attributes["cleanDestination"].InnerText.ToLower());
                            }

                            //Trim spaces and path characters
                            itemToCopy = itemToCopy.Trim(new char[] {' ', '\\', '/'});
                            destPath = destPath.Trim(new char[] {' ', '\\', '/'});
                    
                            string productPath = LBEnvironment.NetworkProductRootDir + "\\" + prodName + "\\V"+ prodVer.Replace(".", "_") + "." + platCode; 
                            if (!Directory.Exists(productPath))
                                throw new Exception("Product directory not found: " + productPath);
                    
                            //Compute and identify - directory, file or wildcard - what to copy. Compute the destination as well. 
                            // Also indicate whether listed path exists.
                            string sourceFullPath = productPath + "\\" + branchOrTrunkName + "\\" + itemToCopy;;
                            string destFullpath = Path.Combine(LBEnvironment.BuildMapDrive, destPath);
                            string alternateSourceFullPath = productPath + "\\" + "DEVELOP" + "\\" + itemToCopy;;
                            bool usingAlternateCopyPath = false;
                            CopyItemType copyItemType = CopyItemType.UNIDENTIFIED;
                            if (itemToCopy.IndexOf("*") > 1){
                                copyItemType = CopyItemType.WILDCARD;
                                if (!Directory.Exists(Path.GetDirectoryName(sourceFullPath))) {
                                    usingAlternateCopyPath = true;
                                    alternateSourceFullPath = productPath + "\\" + "DEVELOP" + "\\" + itemToCopy;
                                }
                            } 
                            else if (Directory.Exists(sourceFullPath)) {
                                copyItemType = CopyItemType.DIRECTORY;
                            }
                            else if (Directory.Exists(alternateSourceFullPath)){
                                copyItemType = CopyItemType.DIRECTORY;
                                usingAlternateCopyPath = true;
                            }
                            else if (File.Exists(sourceFullPath)) {
                                copyItemType = CopyItemType.FILE;
                            } 
                            else if (File.Exists(alternateSourceFullPath)) {
                                copyItemType = CopyItemType.FILE;
                                usingAlternateCopyPath = true;
                            }
                            else { //Listed path not valid
                                throw new Exception("Unable to find import source: " + sourceFullPath);
                            }
                            //Create the destination directory
                            Directory.CreateDirectory(destFullpath);

                            //For QA profiles or non-DEVELOP builds, a build must fail if a listed path cannot be found. Accept no substitutes:
                            //A listed path must not be from DEVELOP, PREPROD, PROD or CANDIDATE and it must exist
                            if (buildProfile.ToUpper().StartsWith("QA") || (currManifest.GetBranchOrTrunkName().ToUpper() != "DEVELOP")){
                        
                                if ((branchOrTrunkName.ToUpper() == "DEVELOP") || (branchOrTrunkName.ToUpper() == "CANDIDATE")
                                    || (branchOrTrunkName.ToUpper() == "PREPROD") || (branchOrTrunkName.ToUpper() == "PROD")
                                    || (branchOrTrunkName.ToUpper() == "STABLE")) {
                            
                                    string errMsg = "Cannot build QA level library or profile without a branch library import source. ";
                                    errMsg += "Please check your entry in " + commandArgs[0] + " for ";
                                    errMsg += prodName + " " + prodVer + " " + platCode + " " + branchOrTrunkName;
                                    throw new Exception(errMsg);       
                                }
                                if (usingAlternateCopyPath)
                                    throw new Exception("Unable to find import source: " + sourceFullPath);
                            }

                            string sourceItem = (usingAlternateCopyPath)? alternateSourceFullPath : sourceFullPath;
                            //If copying a directory add a wildcard
                            if (copyItemType == CopyItemType.DIRECTORY)
                                sourceItem = Path.Combine(sourceItem, "*.*"); 
                            //if clean copy was requested, delete source files' paths at destination    
                            if (cleanAtDestination && Directory.Exists(destFullpath)) {
                                ImportComponents.DeleteFiles(Path.Combine(destFullpath, Path.GetFileName(sourceItem)), copySubdirectories); 
                            }

                            returnValue = ImportComponents.XCopyDirToQDrive(sourceItem, destFullpath, copySubdirectories); 
                            if (returnValue != 0)
                                throw new Exception("xcopy returned an error(" + returnValue.ToString() + ") while copying " + sourceItem + " to " + destFullpath);
                        }
                    }//foreach
                }
            }
            catch(Exception currExcept){
                Console.WriteLine("Error occurred during build import: " + currExcept.Message);
                if (returnValue == 0)
                    returnValue = 1;
            }
            return returnValue;
		}
	}
}
