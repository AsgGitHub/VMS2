//**********************************************************************
//***** Copyright (c) 1992-2013 Allen Systems Group, Inc.  All rights reserved.
//**********************************************************************
//*****$Workfile:   LocateResourcePackages.cs  $
//***** $Archive:   I:\Archives\CS\2\LocateResourcePackages.c~  $
//*****$Revision:   2.1  $
//*****  $Author:   KOFORNAG  $
//*****    $Date:   12 Oct 2006 16:57:24  $
//***** $Modtime:   12 Oct 2006 16:57:22  $
//*****     $Log:   I:\Archives\CS\2\LocateResourcePackages.c~  $
//*****
//*****   Rev 2.1   12 Oct 2006 16:57:24   KOFORNAG
//*****   Merge of Project EISplitProductBuilder
//*****
//*****   Rev 2.0   11 Oct 2006 11:52:02   KOFORNAG
//*****   Create Branch Revision.
//*****
//*****   Rev 1.1   23 Dec 2002 16:17:50   RRUSSELL
//*****Merge of Project EIPRDBLDFixJava2
//*****
//*****   Rev 1.0   25 Nov 2002 16:23:18   RRUSSELL
//*****Initialize Archive
//*****  $Endlog$
//**********************************************************************

using System;
using System.IO;
using System.Collections;

namespace ProductBuilderTools {

	class LocateResourcePackages {

		[STAThread]
        static int Main(string[] args) {

            int errlvl = 0;
            try {

                // Process Command-line Arguments    
                if ( args.Length != 3 ) {
                    Console.WriteLine();
                    Console.Error.WriteLine("Missing or Invalid Parameter.");
                    Usage();
                    errlvl=1;
                    return(errlvl);
                }
    
                string cfgPath = Path.GetFullPath(args[0]);
                string componentName = args[1];
                string productPath = Path.GetFullPath(args[2]);

                string inputFile = Path.Combine( cfgPath, componentName + "_copy.txt" );
                string outputFile = Path.Combine( cfgPath, componentName + "_copy2package.txt" );
                string sourcePath = productPath;
                if ( Directory.Exists( Path.Combine( sourcePath, "java" ) ) )
                    sourcePath = Path.Combine( sourcePath, "java" ); 

                string fileLine;
                StreamReader fileReader = null;
                StreamWriter fileWriter = null;

                // list of resource files
                ArrayList resourceFilenames = new ArrayList();
                try {
                    fileReader = new StreamReader( inputFile );
                    while ( (fileLine=fileReader.ReadLine()) != null )
                        resourceFilenames.Add( Environment.ExpandEnvironmentVariables( fileLine ) );
                }
                finally {
                    if (fileReader!=null) fileReader.Close();
                }

                // list of java files to parse
                ArrayList javaFilenames = new ArrayList();
                string[] complistFilenames = Directory.GetFiles( cfgPath, componentName + "_complist*.txt" );
                foreach ( string complistFile in complistFilenames ) {
                    try {
                        fileReader = new StreamReader( Path.Combine( cfgPath, complistFile ) );
                        while ( (fileLine=fileReader.ReadLine()) != null ) {
                            if ( Path.GetExtension(fileLine).ToLower().Equals(".java") &&
                                File.Exists( Path.Combine( sourcePath, fileLine ) ) ) {
                                javaFilenames.Add( fileLine );
                            }
                        }
                    }
                    finally {
                        if (fileReader!=null) fileReader.Close();
                    }
                }

                // list of copy to/from pairs
                ArrayList tempCopyCommandData = new ArrayList();
                if ( resourceFilenames.Count > 0 ) {
                    foreach ( string javaFilename in javaFilenames ) {
                        string packageName = Path.GetDirectoryName( javaFilename );
                        try {
                            fileReader = new StreamReader( Path.Combine( sourcePath, javaFilename ) );
                            fileLine = fileReader.ReadToEnd().ToLower();
                            foreach ( string resourceFilename in resourceFilenames ) {
                                string resourceFile = Path.GetFileName( resourceFilename.Trim( new char[] {'\"'} ) );
                                if ( fileLine.IndexOf( resourceFile.ToLower() ) > -1 ) {
                                    string copyCommandEntry = resourceFilename + " " + packageName;
                                    tempCopyCommandData.Add( copyCommandEntry );
                                }
                            }
                        }
                        finally {
                            if (fileReader!=null) fileReader.Close();
                        }
                    }
                }

                // add pairs from *_copy2package.txt
                try {
                    fileReader = new StreamReader( outputFile );
                    while ( (fileLine=fileReader.ReadLine()) != null )
                        tempCopyCommandData.Add( fileLine );
                }
                finally {
                    if (fileReader!=null) fileReader.Close();
                }

                // remove duplicates
                ArrayList copyCommandData = new ArrayList();
                tempCopyCommandData.Sort();
                string previous = "";
                foreach ( string entry in tempCopyCommandData ) {
                    if ( !entry.ToLower().Equals( previous.ToLower() ) )
                        copyCommandData.Add( entry );
                    previous = entry;
                }

                // save to output file
                try {
                    fileWriter = new StreamWriter( outputFile , false );
                    foreach ( string entry in copyCommandData )
                        fileWriter.WriteLine( entry );
                }
                finally {
                    if (fileWriter!=null) fileWriter.Close();
                }

            }
            catch ( Exception e ) {

                errlvl = 1;
                Console.Error.WriteLine("An error occurred in LocateResourcePackages.exe");
                Console.Error.WriteLine(e.ToString());
                Usage();

            }

            return(errlvl);

        }


        static void Usage() {
        
            Console.WriteLine();
            Console.WriteLine("Usage: LocateResourcePackage.exe InputFile OutputFile ProductPath");
            Console.WriteLine();
            Console.WriteLine("   required arguments:");
            Console.WriteLine();
            Console.WriteLine("     CfgPath       path where the jcd output is located");
            Console.WriteLine("                   ex: Q:\\class_temp\\");
            Console.WriteLine("     ComponentName ex: ddrintjar");
            Console.WriteLine("     ProductPath   ex: Q:\\");
            Console.WriteLine();
    
        }
    }
}

