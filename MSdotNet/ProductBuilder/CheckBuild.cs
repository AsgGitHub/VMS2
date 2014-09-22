//**********************************************************************
//***** Copyright (c) 1992-2013 Allen Systems Group, Inc.  All rights reserved.
//**********************************************************************
//*****$Workfile:   CheckBuild.cs  $
//***** $Archive:   I:\Archives\CS\2\CheckBuild.c~  $
//*****$Revision:   2.2  $
//*****  $Author:   KOFORNAG  $
//*****    $Date:   22 Nov 2006 14:56:44  $
//***** $Modtime:   22 Nov 2006 14:57:02  $
//*****     $Log:   I:\Archives\CS\2\CheckBuild.c~  $
//*****
//*****   Rev 2.2   22 Nov 2006 14:56:44   KOFORNAG
//*****   Merge of Project EIBranchProjectsPb
//*****
//*****   Rev 2.1   12 Oct 2006 16:57:26   KOFORNAG
//*****   Merge of Project EISplitProductBuilder
//*****
//*****   Rev 2.0   11 Oct 2006 11:51:58   KOFORNAG
//*****   Create Branch Revision.
//*****
//*****   Rev 1.3   03 Sep 2004 17:30:10   KOFORNAG
//*****Merge of Project EILbCommandLine
//*****
//*****   Rev 1.2   Apr 18 2003 16:36:06   RRUSSELL
//*****Merge of Project EIPRDBLD-IntegrateWithLB2
//*****
//*****   Rev 1.1   Mar 24 2003 10:17:10   RRUSSELL
//*****Merge of Project EIPRDBLD-IntegrateWithLB
//*****
//*****   Rev 1.0   19 Feb 2003 12:43:26   RRUSSELL
//*****Initialize Archive
//*****  $Endlog$
//**********************************************************************

using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using LibraryBuilder;
using System.Reflection;

namespace ProductBuilderTools {

	class CheckBuild {

		[STAThread]
        static int Main(string[] args) {

            int errlvl = 0;
    
            try {

                // Process Command-line Arguments
                string buildPath = null;
                string buildUserProfile = null;
                string maxFileAge = null;
                string exclusionFile = null;
                string buildCfgFile = null;
    
                if ( args.Length < 1 ) {
                    Console.WriteLine();
                    Console.Error.WriteLine("Missing or Invalid Parameter.");
                    Usage();        
                    errlvl = 2;
                    return(errlvl);
                }
    
                buildPath = args[0];
                foreach ( string arg in args ) {
                    if ( arg.ToLower().StartsWith("/p:") ) buildUserProfile = arg.Substring(3).Trim();
                    if ( arg.ToLower().StartsWith("/a:") ) maxFileAge       = arg.Substring(3).Trim();
                    if ( arg.ToLower().StartsWith("/e:") ) exclusionFile    = arg.Substring(3).Trim();
                    if ( arg.ToLower().StartsWith("/f:") ) buildCfgFile     = arg.Substring(3).Trim();
                }
  
                Console.WriteLine();
                Console.WriteLine("buildPath:        " + buildPath);
                Console.WriteLine("buildUserProfile: " + buildUserProfile);
                Console.WriteLine("maxFileAge:       " + maxFileAge);
                Console.WriteLine("exclusionFile:    " + exclusionFile);
                Console.WriteLine("buildCfgFile:     " + buildCfgFile);
                Console.WriteLine();

                //build dataset
                ProductBuildDataSet pbDataSet = null;
                if ( buildCfgFile != null ) {
                    pbDataSet = ProductBuildDataSet.ReadBuildConfigurationFile( buildCfgFile );
                }
                else {
                    LibraryManifest li = new LibraryManifest( buildPath );
                    if ( buildUserProfile==null ) {
                        buildUserProfile = "Development";
                    }
                    pbDataSet = ProductBuildDataSet.GetProductBuildDataSetWithoutComplist( buildUserProfile,
                        li.GetName(),
                        li.GetVersion(),
                        li.GetPlatform(),
                        li.GetBranchOrTrunkName(),
                        li.GetDirectory(),
                        buildPath,
                        li.GetDate() );
                    pbDataSet.LoadComplist();
                    pbDataSet.FinalizeProperties();
                }

                if (maxFileAge==null)
                    maxFileAge = pbDataSet.GetBuildProperty("MaximumTargetAge");
                //hack for backwards compatibility
                int colonIndex = maxFileAge.IndexOf(':');
                int periodIndex = maxFileAge.IndexOf('.');
                if ( periodIndex==-1 || colonIndex<periodIndex ) {
                    maxFileAge = maxFileAge.Substring( 0, colonIndex ) + "." + maxFileAge.Substring( colonIndex+1 );
                }

                ArrayList checkedTargets = pbDataSet.GetTargets(true);
                ArrayList uncheckedTargets = pbDataSet.GetTargets(false);

                if ( exclusionFile!=null ) {
                    StringCollection excludedFiles = CollectionUtil.ReadValueFile( exclusionFile );
                    foreach ( string excludedFile in excludedFiles ) {
                        foreach ( string checkedTarget in checkedTargets ) {
                            if ( excludedFile.ToLower() == checkedTarget.ToLower() ) {
                                checkedTargets.Remove( checkedTarget );
                                uncheckedTargets.Add( checkedTarget );
                            }
                        }
                    }
                    uncheckedTargets.Sort();
                }

                //check the build
                Console.WriteLine();
                Console.WriteLine( "Checking product , {0}, {1}, {2}, {3}", 
                    new object[]{ pbDataSet.GetBuildProperty("ProductName"),
                                  pbDataSet.GetBuildProperty("ProductVersion"),
                                  pbDataSet.GetBuildProperty("ProductPlatform"),
                                  pbDataSet.GetBuildProperty("BranchOrTrunkName"),
                                  buildPath} );
                Console.WriteLine( "  in {0}", buildPath );
                Console.WriteLine();

                int totalTargets = checkedTargets.Count + uncheckedTargets.Count;
                int foundTargets = 0;
                int duplicateTargets = 0;
                int excludedTargets = uncheckedTargets.Count;
                int missingTargets = 0;
                int overAgeTargets = 0;
                bool buildSucceeded = true;

                string binPath = Path.Combine( buildPath, "bin" );

                DateTime currentTime = DateTime.Now;
                DateTime checkTime = currentTime - TimeSpan.Parse(maxFileAge);
                for( int targetIndex=0; targetIndex<checkedTargets.Count; targetIndex++ ) {
                    string target = (string)checkedTargets[targetIndex];
                    if ( targetIndex>0 && target.ToLower()==((string)checkedTargets[targetIndex-1]).ToLower() ) {
                        duplicateTargets++;
                        continue;
                    }
                    string targetFilename = Path.Combine( binPath, target );
                    if ( !File.Exists( targetFilename ) ) {
                        buildSucceeded = false;
                        missingTargets++;
                        Console.WriteLine( "{0,-32} Missing", target );
                        continue;
                    }
                    foundTargets++;
                    DateTime creationTime = File.GetLastWriteTime(targetFilename);
                    if ( creationTime < checkTime ) {
                        buildSucceeded = false;
                        overAgeTargets++;
                        TimeSpan overAgeTime = currentTime - creationTime;
                        string overAgeString = "";
                        if ( overAgeTime.Days > 0 ) overAgeString += String.Format( "{0,4}d", overAgeTime.Days ) ;
                        if ( overAgeTime.Hours > 0 ) overAgeString += String.Format( "{0:D2}h", overAgeTime.Hours ) ;
                        overAgeString += String.Format( "{0:D2}m", overAgeTime.Minutes ) ;
                        Console.WriteLine( "{0,-32} OverAge:{1}", target, overAgeString );
                    }
                }

                //results summary
                Console.WriteLine();
                Console.WriteLine("CheckBuild Statistics:");
                Console.WriteLine("Product           : {0}", pbDataSet.GetBuildProperty("ProductName") );
                Console.WriteLine("Version           : {0}", pbDataSet.GetBuildProperty("ProductVersion") );
                Console.WriteLine("Platform          : {0}", pbDataSet.GetBuildProperty("ProductPlatform") );
                Console.WriteLine("BranchOrTrunkName : {0}", pbDataSet.GetBuildProperty("BranchOrTrunkName") );
                Console.WriteLine("Target Dir        : {0}", buildPath );
                Console.WriteLine("Max File Age      : {0} (DD.HH:MM:SS)", maxFileAge );
                Console.WriteLine("Targets           : {0}", totalTargets );
                Console.WriteLine("Found             : {0}", foundTargets );
                Console.WriteLine("Duplicates        : {0}", duplicateTargets );
                Console.WriteLine("Excluded          : {0}", excludedTargets );
                Console.WriteLine("Missing           : {0}", missingTargets );
                Console.WriteLine("OverAge           : {0}", overAgeTargets );
                string successString = (buildSucceeded) ? "Successful Build." : "Build Failed!!!";
                Console.WriteLine("Results           : {0}", successString );

                errlvl = (buildSucceeded) ? 0 : 1;

            }
            catch (Exception e) {

                errlvl = 2;
                Console.Error.WriteLine("An error occurred in CheckBuild:");
                Console.Error.WriteLine(e.ToString());
                Usage();

            }

            return(errlvl);
        }


        static void Usage() {
 
            Console.WriteLine();
            Console.WriteLine("CheckBuild.exe - check the build bin directory for VMS-listed targets");
            Console.WriteLine();
            Console.WriteLine("Usage: CheckBuild <buildPath> [ /f:<buildCfgFile> ] [ /p:<buildUserProfile> ] ");
            Console.WriteLine("           [ /a:<maxFileAge> ] [ /e:<exclusionFile> ] ");
            Console.WriteLine();
            Console.WriteLine("   required argument:");
            Console.WriteLine();
            Console.WriteLine("     <buildPath>");
            Console.WriteLine("             path to the product directory");
            Console.WriteLine();
            Console.WriteLine("   optional arguments:");
            Console.WriteLine();
            Console.WriteLine("     /f:<buildCfgFile>");
            Console.WriteLine("             path to the build configuration file.  If buildCfgFile is");
            Console.WriteLine("             specified, the target checklist contained in this file will");
            Console.WriteLine("             be used to check the build.  Otherwise, a default target");
            Console.WriteLine("             checklist will be generated by VMS.");
            Console.WriteLine();
            Console.WriteLine("     /p:<buildUserProfile>");
            Console.WriteLine("             build user profile used to generate a target checklist when");
            Console.WriteLine("             buildCfgFilename is not specified.  Defaults to 'Development'.");
            Console.WriteLine();
            Console.WriteLine("     /a:<maxFileAge>");
            Console.WriteLine("             sets or overrides the maxFileAge from buildCfgFile. Format is");
            Console.WriteLine("             [dd.hh:mm:ss] or [dd:hh:mm:ss]");
            Console.WriteLine();
            Console.WriteLine("     /e:<exclusionFile>");
            Console.WriteLine("             exclusionFile contains a list of additional files to be excluded.");
            Console.WriteLine();
            Console.WriteLine("   return codes:");
            Console.WriteLine("             0  -  build success");
            Console.WriteLine("             1  -  build failure");
            Console.WriteLine("             2  -  CheckBuild error");

        }
	}
}
