//**********************************************************************
//***** Copyright (c) 1992-2013 Allen Systems Group, Inc.  All rights reserved.
//**********************************************************************
//*****$Workfile:   SetBuildEnvironment.cs  $
//***** $Archive:   I:\Archives\CS\2\SetBuildEnvironment.c~  $
//*****$Revision:   2.1  $
//*****  $Author:   KOFORNAG  $
//*****    $Date:   12 Oct 2006 16:57:28  $
//***** $Modtime:   12 Oct 2006 16:57:22  $
//*****     $Log:   I:\Archives\CS\2\SetBuildEnvironment.c~  $
//*****
//*****   Rev 2.1   12 Oct 2006 16:57:28   KOFORNAG
//*****   Merge of Project EISplitProductBuilder
//*****
//*****   Rev 2.0   11 Oct 2006 11:52:04   KOFORNAG
//*****   Create Branch Revision.
//*****
//*****   Rev 1.3   03 Sep 2004 17:30:18   KOFORNAG
//*****Merge of Project EILbCommandLine
//*****
//*****   Rev 1.2   Apr 18 2003 16:36:50   RRUSSELL
//*****Merge of Project EIPRDBLD-IntegrateWithLB2
//*****
//*****   Rev 1.1   Mar 24 2003 10:17:20   RRUSSELL
//*****Merge of Project EIPRDBLD-IntegrateWithLB
//*****
//*****   Rev 1.0   19 Feb 2003 13:10:52   RRUSSELL
//*****Initialize Archive
//*****  $Endlog$
//**********************************************************************

using System;
using System.Collections;
using System.Reflection;

using LibraryBuilder;

namespace ProductBuilderTools {

	class SetBuildEnvironment {

		[STAThread]
        static int Main(string[] args) {

            int errlvl = 0;
    
            try {

                // Process Command-line Arguments
                string buildCfgFile = null;
                string outputFile = null;

                if ( args.Length != 2 ) {
                    Console.WriteLine();
                    Console.Error.WriteLine("Missing or Invalid Parameter.");
                    Usage();        
                    errlvl = 1;
                    return(errlvl);
                }
    
                buildCfgFile = args[0];
                outputFile = args[1].Trim();
    
                Console.WriteLine();
                Console.WriteLine("buildCfgFile: " + buildCfgFile);
                Console.WriteLine("outputFile:   " + outputFile);
                Console.WriteLine();

                ProductBuildDataSet pbDataSet = ProductBuildDataSet.ReadBuildConfigurationFile( buildCfgFile );
                SortedList buildProperties = pbDataSet.GetBuildProperties();
                CollectionUtil.WritePropertyFile( buildProperties, outputFile );
    
            }
            catch (Exception e) {

                errlvl = 1;
                Console.Error.WriteLine("An error occurred in SetBuildEnvironment:");
                Console.Error.WriteLine(e.ToString());
                Usage();

            }

            return(errlvl);
        }


        static void Usage() {
 
            Console.WriteLine();
            Console.WriteLine("SetBuildEnvironment.exe - Generate a flat-text list of Build Properties");
            Console.WriteLine();
            Console.WriteLine("Usage: SetBuildEnvironment <buildCfgFile> <outputFile>");
            Console.WriteLine();
            Console.WriteLine("   required arguments:");
            Console.WriteLine();
            Console.WriteLine("     <buildCfgFile>");
            Console.WriteLine("             path to the XML build configuration file");
            Console.WriteLine();
            Console.WriteLine("     <outputFile>");
            Console.WriteLine("             path to the flat-text output file");
            Console.WriteLine();
            Console.WriteLine("   return codes:");
            Console.WriteLine("             0  -  success");
            Console.WriteLine("             1  -  failure");

        }
	}
}
