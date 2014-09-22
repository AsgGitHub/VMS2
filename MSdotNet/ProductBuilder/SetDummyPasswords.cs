//**********************************************************************
//***** Copyright (c) 1992-2013 Allen Systems Group, Inc.  All rights reserved.
//**********************************************************************
//*****$Workfile:   SetDummyPasswords.cs  $
//***** $Archive:   I:\Archives\CS\2\SetDummyPasswords.c~  $
//*****$Revision:   2.1  $
//*****  $Author:   KOFORNAG  $
//*****    $Date:   12 Oct 2006 16:57:26  $
//***** $Modtime:   12 Oct 2006 16:57:22  $
//*****     $Log:   I:\Archives\CS\2\SetDummyPasswords.c~  $
//*****
//*****   Rev 2.1   12 Oct 2006 16:57:26   KOFORNAG
//*****   Merge of Project EISplitProductBuilder
//*****
//*****   Rev 2.0   11 Oct 2006 11:52:06   KOFORNAG
//*****   Create Branch Revision.
//*****
//*****   Rev 1.2   03 Sep 2004 17:30:08   KOFORNAG
//*****Merge of Project EILbCommandLine
//*****
//*****   Rev 1.1   Apr 18 2003 16:36:50   RRUSSELL
//*****Merge of Project EIPRDBLD-IntegrateWithLB2
//*****
//*****   Rev 1.0   Apr 09 2003 15:09:04   RRUSSELL
//*****Initialize Archive
//*****  $Endlog$
//**********************************************************************

using System;
using System.Reflection;

using LibraryBuilder;

namespace ProductBuilderTools {

	class SetDummyPasswords {

		[STAThread]
		static int Main(string[] args) {

            int errlvl = 0;
    
            try {
    
                string buildCfgFile = args[0];

                ProductBuildDataSet pbDataSet = ProductBuildDataSet.ReadBuildConfigurationFile( buildCfgFile );

                string unixPassword = pbDataSet.GetBuildProperty("UnixPassword");
                if ( unixPassword!=null ) {
                    pbDataSet.SetBuildProperty( "UnixPassword", "dummy", true );
                }

                pbDataSet.WriteBuildConfigurationFile( buildCfgFile );

            }
            catch (Exception e) {

                errlvl = 1;
                Console.Error.WriteLine("An error occurred in SetDummyPasswords:");
                Console.Error.WriteLine(e.ToString());

            }

            return(errlvl);

		}
	}
}
