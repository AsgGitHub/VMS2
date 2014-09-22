//**********************************************************************
//***** Copyright (c) 1992-2013 Allen Systems Group, Inc.  All rights reserved.
//**********************************************************************
//*****$Workfile:   Lowercase.cs  $
//***** $Archive:   I:\Archives\CS\2\Lowercase.c~  $
//*****$Revision:   2.1  $
//*****  $Author:   KOFORNAG  $
//*****    $Date:   12 Oct 2006 16:57:24  $
//***** $Modtime:   12 Oct 2006 16:57:22  $
//*****     $Log:   I:\Archives\CS\2\Lowercase.c~  $
//*****
//*****   Rev 2.1   12 Oct 2006 16:57:24   KOFORNAG
//*****   Merge of Project EISplitProductBuilder
//*****
//*****   Rev 2.0   11 Oct 2006 11:52:02   KOFORNAG
//*****   Create Branch Revision.
//*****
//*****   Rev 1.1   Apr 19 2003 15:34:16   RRUSSELL
//*****Merge of Project EIPRDBLD-Misc28
//*****
//*****   Rev 1.0   Apr 18 2003 17:41:06   RRUSSELL
//*****Initialize Archive
//*****  $Endlog$
//**********************************************************************

using System;
using System.IO;
using System.Text;

namespace ProductBuilder {

	class Lowercase {

        static int Main(string[] args) {

            int errlvl = 0;
    
            try {

                Console.WriteLine(args[0].ToLower());

            }
            catch (Exception e) {

                e.ToString();
                errlvl = 1;

            }

            return(errlvl);
        
        }
	}
}
