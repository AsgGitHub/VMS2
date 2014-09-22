//**********************************************************************
//***** Copyright (c) 1992-2013 Allen Systems Group, Inc.  All rights reserved.
//**********************************************************************
//*****$Workfile:   JCDParser.cs  $
//***** $Archive:   I:\Archives\CS\3\JCDParser.c~  $
//*****$Revision:   3.1  $
//*****  $Author:   KOFORNAG  $
//*****    $Date:   12 Oct 2006 16:57:24  $
//***** $Modtime:   12 Oct 2006 16:57:22  $
//*****     $Log:   I:\Archives\CS\3\JCDParser.c~  $
//*****
//*****   Rev 3.1   12 Oct 2006 16:57:24   KOFORNAG
//*****   Merge of Project EISplitProductBuilder
//*****
//*****   Rev 3.0   11 Oct 2006 11:52:00   KOFORNAG
//*****   Create Branch Revision.
//*****
//*****   Rev 1.3   Aug 27 2003 10:02:04   RRUSSELL
//*****Merge of Project EISupportConsistentJDKVersion
//*****
//*****   Rev 1.2   27 Jan 2003 15:09:14   RRUSSELL
//*****Merge of Project EIPRDBLD-Misc22
//*****
//*****   Rev 1.1   23 Dec 2002 16:17:44   RRUSSELL
//*****Merge of Project EIPRDBLDFixJava2
//*****
//*****   Rev 1.0   18 Nov 2002 17:17:32   RRUSSELL
//*****Initialize Archive
//*****  $Endlog$
//**********************************************************************

using System;
using System.IO;

namespace ProductBuilderTools {

	class JCDParser {

		[STAThread]
		static int Main(string[] args) {

            int errlvl = 0;

            try {

                // Process Command-line Arguments    
                if ( args.Length != 2 ) {
                    Console.WriteLine();
                    Console.Error.WriteLine("Missing or Invalid Parameter.");
                    Usage();
                    errlvl=1;
                    return(errlvl);
                }
    
                string jcdFile = Path.GetFullPath(args[0]);
                string outputPath  = Path.GetFullPath(args[1]);

                JCDParserResults pr = new JCDParserResults();
                string java_home = Environment.GetEnvironmentVariable("JAVA_HOME");
                if ( java_home == null ) {
                    java_home = JCDParserResults.JDKToJAVA_HOME(Environment.GetEnvironmentVariable("JDK"));
                }
                pr.ParseJCDFile( jcdFile, 
                                 Environment.GetEnvironmentVariable("CLASSPATH").ToLower(), 
                                 java_home.ToLower() );
                pr.RemoveDuplicateSourceFiles();
                pr.WriteResults( Path.GetFileNameWithoutExtension(jcdFile), outputPath );

            }
            catch ( Exception e ) {

                errlvl = 1;
                Console.Error.WriteLine("An error occurred in JCDParser.exe");
                Console.Error.WriteLine(e.ToString());
                Usage();

            }

            return(errlvl);

		}


        static void Usage() {
        
            Console.WriteLine();
            Console.WriteLine("Usage: JCDParser.exe JCDFile OutputPath");
            Console.WriteLine();
            Console.WriteLine("   required arguments:");
            Console.WriteLine();
            Console.WriteLine("     JCDFile      ex: Q:\\jcd\\esavJar.jcd");
            Console.WriteLine();
            Console.WriteLine("     OutputPath   ex: Q:\\class_temp\\");
            Console.WriteLine("             parser output files are written to this directory");
            Console.WriteLine();
    
        }
	}
}


