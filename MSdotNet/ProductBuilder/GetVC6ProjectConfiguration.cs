//**********************************************************************
//***** Copyright (c) 1992-2013 Allen Systems Group, Inc.  All rights reserved.
//**********************************************************************
//*****$Workfile:   GetVC6ProjectConfiguration.cs  $
//***** $Archive:   I:\Archives\CS\2\GetVC6ProjectConfiguration.c~  $
//*****$Revision:   2.1  $
//*****  $Author:   KOFORNAG  $
//*****    $Date:   12 Oct 2006 16:57:26  $
//***** $Modtime:   12 Oct 2006 16:57:22  $
//*****     $Log:   I:\Archives\CS\2\GetVC6ProjectConfiguration.c~  $
//*****
//*****   Rev 2.1   12 Oct 2006 16:57:26   KOFORNAG
//*****   Merge of Project EISplitProductBuilder
//*****
//*****   Rev 2.0   11 Oct 2006 11:51:58   KOFORNAG
//*****   Create Branch Revision.
//*****
//*****   Rev 1.2   Apr 18 2003 16:36:16   RRUSSELL
//*****Merge of Project EIPRDBLD-IntegrateWithLB2
//*****
//*****   Rev 1.1   Mar 24 2003 10:17:14   RRUSSELL
//*****Merge of Project EIPRDBLD-IntegrateWithLB
//*****
//*****   Rev 1.0   17 Sep 2002 15:25:42   RRUSSELL
//*****Initialize Archive
//*****  $Endlog$
//**********************************************************************

using System;
using System.IO;
using System.Text;

namespace ProductBuilderTools {

    class GetVC6ProjectConfiguration {

        [STAThread]
        static int Main(string[] args) {

            int errlvl = 0;

            StreamReader makefile = null;
    
            try {

                // Process Command-line Arguments
                string makefilePath;
                string cfg;
    
                if ( args.Length != 2 ) {       
                    errlvl = 2;
                    return(errlvl);
                }
    
                makefilePath = args[0];
                cfg          = args[1];
    
                // Determine correct casing for configuration
                string cfgStringLine = "!MESSAGE \"" + Path.GetFileNameWithoutExtension(makefilePath) + " - Win32 " + cfg + "\"";
                string caseCorrectedCfgString = null;

                makefile = new StreamReader( makefilePath );    
                string makefileLine;     
                while ( (makefileLine=makefile.ReadLine()) != null ) {
                    if ( makefileLine.ToLower().StartsWith(cfgStringLine.ToLower()) )
                        caseCorrectedCfgString = makefileLine.Split(new char[]{'\"'})[1];     
                }
    
                // Echo ouput and finish
                Console.WriteLine( caseCorrectedCfgString );
  
                if ( caseCorrectedCfgString==null ) {
                    errlvl = 1;
                }
    
            }
            catch (Exception e) {
                e.ToString();
                errlvl = 2;
            }
            finally {
                if (makefile!=null) makefile.Close();
            }

            return(errlvl);
        }


        static void Usage() {
 
            Console.WriteLine();
            Console.WriteLine("GetVC6ProjectConfiguration.exe - Get case corrected Project Configuration string");
            Console.WriteLine("     from VC6 makefile");
            Console.WriteLine();
            Console.WriteLine("Usage: GetVC6ProjectConfiguration makefilePath cfg");
            Console.WriteLine();
            Console.WriteLine("   required arguments:");
            Console.WriteLine();
            Console.WriteLine("     makefilePath ( ex: q:\\mak\\ACREATE.mak )");
            Console.WriteLine();
            Console.WriteLine("     cfg          ( release/debug/etc. ) ");
            Console.WriteLine();
            Console.WriteLine("   output:");
            Console.WriteLine();
            Console.WriteLine("     project configuration string");
            Console.WriteLine("                  case corrected project configuration string");
            Console.WriteLine("                  ( ex: acreate - Win32 Release )");
    
        }

    }
}

