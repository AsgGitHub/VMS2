//**********************************************************************
//***** Copyright (c) 1992-2013 Allen Systems Group, Inc.  All rights reserved.
//**********************************************************************
//*****$Workfile:   TouchDirectories.cs  $
//***** $Archive:   I:\Archives\CS\1\VMS.Source.MSdotNet.ProductBuilder.TouchDirectories.c~  $
//*****$Revision:   1.1  $
//*****  $Author:   shermank  $
//*****    $Date:   17 Oct 2013 10:35:50  $
//***** $Modtime:   17 Oct 2013 10:35:44  $
//*****     $Log:   I:\Archives\CS\1\VMS.Source.MSdotNet.ProductBuilder.TouchDirectories.c~  $
//*****
//*****   Rev 1.1   17 Oct 2013 10:35:50   shermank
//*****   Merge of Project EIVMS40ProductBuilder
//*****
//*****   Rev 2.1   12 Oct 2006 16:57:26   KOFORNAG
//*****   Merge of Project EISplitProductBuilder
//*****
//*****   Rev 2.0   11 Oct 2006 11:52:06   KOFORNAG
//*****   Create Branch Revision.
//*****
//*****   Rev 1.1   Jul 24 2003 18:24:50   RRUSSELL
//*****Merge of Project EIPRDBLD-Misc33
//*****
//*****   Rev 1.0   Jul 23 2003 17:00:58   RRUSSELL
//*****Initialize Archive
//*****  $Endlog$
//**********************************************************************
using System;
using System.IO;

namespace ProductBuilderTools {

    class TouchDirectories {

        [STAThread]
        static int Main(string[] args) {

            int errlvl = 0;
            
            try {

                // Process Command-line Arguments
                string rootDirectory = args[0];
                string dateTimeString = args[1];

                DateTime dateTime = Convert.ToDateTime( dateTimeString );

                string[] directories = Directory.GetDirectories( rootDirectory );
                foreach ( string directory in directories ) {
                    Directory.SetCreationTime( directory, dateTime );
                    Directory.SetLastAccessTime( directory, dateTime );
                    Directory.SetLastWriteTime( directory, dateTime );
                }

            }
            catch ( Exception e ) {

                errlvl = 1;
                Console.Error.WriteLine("An error occurred in TouchDirectories.exe");
                Console.Error.WriteLine(e.ToString());
                Usage();

            }

            return(errlvl);

        }


        static void Usage() {
        
            Console.WriteLine();
            Console.WriteLine("Usage: TouchDirectories.exe rootDirectory dateTime" );
            Console.WriteLine();
            Console.WriteLine("        touch subdirectories of rootDirectory with dateTime");
            Console.WriteLine();
            Console.WriteLine("        ex: touchdirectories c:\temp \"10-30-2003 14:45:06\"");
            Console.WriteLine();
        
        }

    }

}
