//**********************************************************************
//***** Copyright (c) 1992-2013 Allen Systems Group, Inc.  All rights reserved.
//**********************************************************************
//*****$Workfile:   ResolveComplist.cs  $
//***** $Archive:   I:\Archives\CS\2\ResolveComplist.c~  $
//*****$Revision:   2.1  $
//*****  $Author:   KOFORNAG  $
//*****    $Date:   12 Oct 2006 16:57:28  $
//***** $Modtime:   12 Oct 2006 16:57:22  $
//*****     $Log:   I:\Archives\CS\2\ResolveComplist.c~  $
//*****
//*****   Rev 2.1   12 Oct 2006 16:57:28   KOFORNAG
//*****   Merge of Project EISplitProductBuilder
//*****
//*****   Rev 2.0   11 Oct 2006 11:52:04   KOFORNAG
//*****   Create Branch Revision.
//*****
//*****   Rev 1.3   03 Sep 2004 17:30:16   KOFORNAG
//*****Merge of Project EILbCommandLine
//*****
//*****   Rev 1.2   Apr 18 2003 16:36:46   RRUSSELL
//*****Merge of Project EIPRDBLD-IntegrateWithLB2
//*****
//*****   Rev 1.1   Mar 24 2003 10:17:20   RRUSSELL
//*****Merge of Project EIPRDBLD-IntegrateWithLB
//*****
//*****   Rev 1.0   17 Sep 2002 15:25:50   RRUSSELL
//*****Initialize Archive
//*****  $Endlog$
//**********************************************************************

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;

using LibraryBuilder;

namespace ProductBuilderTools {

    class ResolveComplist {

        [STAThread]
        static int Main(string[] args) {

            int errlvl = 0;
    
            try {

                // Process Command-line Arguments
                bool returnAll = false;
                string buildPath = null;
                string complistFilterFileName = null;
                string outputComplistFileName = null;
                string descExtension = null;
                string targetExtension = null;
    
                if ( args.Length < 1 ) {
                    Console.WriteLine();
                    Console.Error.WriteLine("Missing or Invalid Parameter.");
                    Usage();        
                    errlvl = 2;
                    return(errlvl);
                }
    
                buildPath = args[0];
                foreach ( string arg in args ) {
                    if ( arg.StartsWith("/f:") ) complistFilterFileName    = arg.Substring(3).Trim();
                    if ( arg.StartsWith("/o:") ) outputComplistFileName    = arg.Substring(3).Trim();
                    if ( arg.StartsWith("/d:") ) descExtension             = arg.Substring(3).Trim();
                    if ( arg.StartsWith("/t:") ) targetExtension           = arg.Substring(3).Trim();
                    if ( arg == "/a" )           returnAll                 = true;
                }
                if ( complistFilterFileName=="" )    complistFilterFileName = null;
                if ( outputComplistFileName=="" )    outputComplistFileName = null;
                if ( descExtension=="" )             descExtension = null;
                if ( targetExtension=="" )           targetExtension = null;
    
                Console.WriteLine();
                Console.WriteLine("buildPath:              " + buildPath);
                Console.WriteLine("complistFilterFileName: " + complistFilterFileName);
                Console.WriteLine("outputComplistFile:     " + outputComplistFileName);
                Console.WriteLine("descExtension:          " + descExtension);
                Console.WriteLine("targetExtension:        " + targetExtension);
                Console.WriteLine();
    
                string selectFilter = "";
                if ( !returnAll )
                    selectFilter += "Build = true and ";
                if ( descExtension != null ) {
                    selectFilter += "DescriptorExtension = '" + descExtension + "' and ";
                }
                if ( targetExtension != null ) {
                    selectFilter += "Child.TargetExtension = '" + targetExtension + "'";
                }
                if ( selectFilter.LastIndexOf("and ")==selectFilter.Length-4 ) {
                    selectFilter = selectFilter.Substring(0, selectFilter.Length-4);
                }

                // perform descriptor and target extension filtering
                ArrayList complist = ProductBuildDataSet.GetFilteredComplist( buildPath, selectFilter );

                // perform file filtering
                if ( complistFilterFileName!=null ) {
                    StringCollection complistFilterFiles = CollectionUtil.ReadValueFile( complistFilterFileName );
                    foreach ( string component in complist ) {
                        bool matched = false;
                        foreach ( string complistFilterFile in complistFilterFiles ) {
                            if ( component.ToUpper()==complistFilterFile.ToUpper() ) {
                                matched = true;
                                break;
                            }
                        }
                        if ( matched )
                            continue;
                        complist.Remove( component );
                    }
                }

                CollectionUtil.WriteValueFile( complist, outputComplistFileName );

                if (complist.Count < 1) {
                    Console.WriteLine("Complist is empty.");
                    errlvl = 1;
                }

                foreach ( string component in complist ) {
                    Console.WriteLine(component);
                }
    
            }
            catch (Exception e) {

                errlvl = 2;
                Console.Error.WriteLine("An error occurred in ResolveComplist:");
                Console.Error.WriteLine(e.ToString());
                Usage();

            }

            return(errlvl);
        }


        static void Usage() {
 
            Console.WriteLine();
            Console.WriteLine("ResolveComplist.exe - generate complist as a function of VMS query generated");
            Console.WriteLine("     complist, externally supplied complist, and inclusion or exclusion lists");
            Console.WriteLine();
            Console.WriteLine("Usage: ResolveComplist <buildPath> /f:<complistFilterFile>");
            Console.WriteLine("          /o:<outputComplistFile> /d:<descExtension> /t:<targetExtension>");
            Console.WriteLine();
            Console.WriteLine("   required argument:");
            Console.WriteLine();
            Console.WriteLine("     <buildPath>");
            Console.WriteLine("             path to build source root directory -- defaults to current directory");
            Console.WriteLine();
            Console.WriteLine("   optional arguments:");
            Console.WriteLine();
            Console.WriteLine("     /f:<complistFilterFile>");
            Console.WriteLine("             complist is filtered on component descriptors listed in this file");
            Console.WriteLine();
            Console.WriteLine("     /o:<outputComplistFile>");
            Console.WriteLine("             resolved complist output file");
            Console.WriteLine();
            Console.WriteLine("     /d:<descExtension>");
            Console.WriteLine("             complist is filtered on component descriptor extension");
            Console.WriteLine();
            Console.WriteLine("     /t:<targetExtension>");
            Console.WriteLine("             complist is filtered on target descriptor extension");
            Console.WriteLine();
            Console.WriteLine("     /a");
            Console.WriteLine("             return all matching components, even if not built");
            Console.WriteLine();
            Console.WriteLine("   return codes:");
            Console.WriteLine("             0  -  success");
            Console.WriteLine("             1  -  empty compilation list");
            Console.WriteLine("             2  -  error");

        }

    }

}

