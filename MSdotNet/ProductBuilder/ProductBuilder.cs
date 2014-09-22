//**********************************************************************
//***** Copyright (c) 1992-2013 Allen Systems Group, Inc.  All rights reserved.
//**********************************************************************
//*****$Workfile:   ProductBuilder.cs  $
//***** $Archive:   I:\Archives\CS\2\ProductBuilder.c~  $
//*****$Revision:   2.2  $
//*****  $Author:   KOFORNAG  $
//*****    $Date:   22 Nov 2006 14:56:46  $
//***** $Modtime:   22 Nov 2006 14:57:02  $
//*****     $Log:   I:\Archives\CS\2\ProductBuilder.c~  $
//*****
//*****   Rev 2.2   22 Nov 2006 14:56:46   KOFORNAG
//*****   Merge of Project EIBranchProjectsPb
//*****
//*****   Rev 2.1   12 Oct 2006 16:57:24   KOFORNAG
//*****   Merge of Project EISplitProductBuilder
//*****
//*****   Rev 2.0   11 Oct 2006 11:52:02   KOFORNAG
//*****   Create Branch Revision.
//*****
//*****   Rev 1.5   07 Jul 2005 18:00:24   KOFORNAG
//*****   Merge of Project FILbPreventDuplicateQueue
//*****
//*****   Rev 1.4   16 Jun 2005 18:12:06   DMASKOFF
//*****   DMASKOFF - Merge of Project EIVMSPBRebuildPB
//*****
//*****   Rev 1.3   03 Sep 2004 17:30:06   KOFORNAG
//*****Merge of Project EILbCommandLine
//*****
//*****   Rev 1.2   Apr 18 2003 16:36:38   RRUSSELL
//*****Merge of Project EIPRDBLD-IntegrateWithLB2
//*****
//*****   Rev 1.1   Mar 24 2003 10:17:18   RRUSSELL
//*****Merge of Project EIPRDBLD-IntegrateWithLB
//*****
//*****   Rev 1.0   17 Sep 2002 15:25:44   RRUSSELL
//*****Initialize Archive
//*****  $Endlog$
//**********************************************************************

using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Messaging;
using System.Reflection;
using LibraryBuilder;



namespace ProductBuilder
{

	class ProductBuilder {
		static int errorLevel = 0;

		[STAThread]
		static int Main(string[] args) {

            if ( args.Length < 1 ) {
                errorLevel = 3;
                Usage();
                return(errorLevel);
            }

            try {
                string path = null;
                Hashtable commandLineProperties = new Hashtable();

                ProcessCommandLineArguments( args, ref path, commandLineProperties);

                LibraryManifest libraryInfo = new LibraryManifest( path );

                // default to Development profile
                string profileName = "Development (Public)";
                string commandLineProfileName = (string)commandLineProperties["BuildProfileName"];
                profileName = (commandLineProfileName!=null) ? commandLineProfileName : profileName;

                ProductBuildDataSet pbDataSet = ProductBuildDataSet.GetProductBuildDataSetWithoutComplist(
                                                                        profileName,
                                                                        libraryInfo.GetName(),
                                                                        libraryInfo.GetVersion(),
                                                                        libraryInfo.GetPlatform(),
                                                                        libraryInfo.GetBranchOrTrunkName(),
                                                                        libraryInfo.GetDirectory(),
                                                                        path,
                                                                        libraryInfo.GetDate());

                pbDataSet.ApplyCommandLineProperties( commandLineProperties );
                pbDataSet.LoadComplist();
                pbDataSet.FinalizeProperties();
                pbDataSet.WriteBuildConfigurationFile( pbDataSet.GetBuildConfigurationFilename() );

                string buildPath = pbDataSet.GetBuildProperty("BuildPath");
                VmsProduct currProduct = new VmsProduct(libraryInfo.GetName(),libraryInfo.GetVersion(),libraryInfo.GetPlatform());
				VmsLibrary currLibrary = new VmsLibrary(currProduct,libraryInfo.GetBranchOrTrunkName());
                ControllerObject controllerObject = new ControllerObject();
				string currentKey = Util.GetServiceKey();
				bool actionSucceeded = false;
				string errorMessage = null;
				if (controllerObject.BuildLibrary(currLibrary,currentKey,buildPath,out errorMessage,false)) {
    			    NewMessage.ProcessMessages(currentKey, ref errorMessage, ref actionSucceeded, false, ref errorLevel);
 					if (actionSucceeded) 
						Console.WriteLine("    Successfully built " + currLibrary.ShortLibraryName );
					else
						Console.WriteLine("    Error building " + currLibrary.ShortLibraryName + ":" + errorMessage);
				}
				else
					Console.WriteLine("    Error building " + currLibrary.ShortLibraryName + ":" + errorMessage);
            }
            catch (Exception e) {
                errorLevel = 3;
                Console.Error.WriteLine("An error occurred in ProductBuilder:");
                Console.Error.WriteLine(e.ToString());
                Usage();
            }
            return(errorLevel);
        }

        static void ProcessCommandLineArguments( string[] args,                               
            ref string path,
            Hashtable commandLineProperties ) {
 
            path = Path.GetFullPath( args[0] );

            StringCollection argErrors = new StringCollection();
            string[] prop_val_pair;
            char[] sep = new char[]{'='};
            for ( int i=1; i<args.Length; i++) {
                string arg = args[i];
                prop_val_pair = arg.Split(sep);
                if ( prop_val_pair.Length==2 ) {
                    commandLineProperties.Add( prop_val_pair[0], prop_val_pair[1] );
                    continue;
                }
                argErrors.Add( arg );
            }

            if ( argErrors.Count > 0 ) {
                StringBuilder errmsg = new StringBuilder("Invalid arguments supplied:\r\n");
                foreach ( string argerr in argErrors )
                    errmsg.Append(argerr + "\r\n");
                throw new Exception(errmsg.ToString());
            }
            
            Console.WriteLine();
            foreach ( DictionaryEntry pair in commandLineProperties ) {
                Console.WriteLine(pair.Key + "=" + pair.Value);
            }

        }



        static void Usage() {
 
            Console.WriteLine();
            Console.WriteLine("BuildProduct.exe - build Mobius product");
            Console.WriteLine();
            Console.WriteLine("Usage: BuildProduct path [ [property1=value1] [property2=value2] ... ]");
            Console.WriteLine();
            Console.WriteLine("     path");
            Console.WriteLine("             path can be any resolvable path except paths starting with Q:");
            Console.WriteLine();
            Console.WriteLine("     propertyn=valuen");
            Console.WriteLine("             Property/value pairs are used to override product or profile defaults");
            Console.WriteLine("             For more information, see ProductBuilder documentation at:");
            Console.WriteLine();
            Console.WriteLine("                 G:\\Develop\\Guide\\ProductBuilder User's Guide.doc");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("     return codes:");
            Console.WriteLine("             0  -  build success");
            Console.WriteLine("             1  -  build success with warnings");
            Console.WriteLine("             2  -  build failure");
            Console.WriteLine("             3  -  build process failure");
            Console.WriteLine();

        }


    }
}


