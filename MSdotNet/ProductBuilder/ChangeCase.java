//**********************************************************************
//***** Copyright (c) 1992-2013 Allen Systems Group, Inc.  All rights reserved.
//**********************************************************************
//*****$Workfile:   ChangeCase.java  $
//***** $Archive:   I:\Archives\java\2\ChangeCase.j~va  $
//*****$Revision:   2.1  $
//*****  $Author:   KOFORNAG  $
//*****    $Date:   12 Oct 2006 16:57:32  $
//***** $Modtime:   12 Oct 2006 16:57:22  $
//*****     $Log:   I:\Archives\java\2\ChangeCase.j~va  $
//*****
//*****   Rev 2.1   12 Oct 2006 16:57:32   KOFORNAG
//*****   Merge of Project EISplitProductBuilder
//*****
//*****   Rev 2.0   11 Oct 2006 11:52:18   KOFORNAG
//*****   Create Branch Revision.
//*****
//*****   Rev 1.3   Mar 28 2003 10:17:58   RRUSSELL
//*****Merge of Project EIPRDBLD-Misc26
//*****
//*****   Rev 1.2   12 Mar 2003 12:02:04   ADEVASIA
//*****Merge of Project FWFixChangeCase
//*****
//*****   Rev 1.1   26 Jun 2002 15:12:44   RRUSSELL
//*****Merge of Project EIPRDBLD-SolarisBuild
//*****
//*****   Rev 1.0   26 Jun 2002 10:51:32   RRUSSELL
//*****Initialize Archive
//*****  $Endlog$
//**********************************************************************

// FrontEnd Plus GUI for JAD
// DeCompiled : ChangeCase.class

import java.io.File;
import java.io.PrintStream;

public class ChangeCase
{

    public ChangeCase()
    {
    }

    public static void main(String args[])
    {
        boolean upperCaseFlag = false;
        boolean recurseSubdirFlag = false;
        boolean changeDirFlag = false;
        if(args.length < 1)
        {
            System.out.println("Missing target directory");
            System.out.println("ChangeCase targetDir [files_dirs] [recurse] [upper_lower]");
            System.out.println("files_dirs  - 0 = files, 1 = dirs");
            System.out.println("recurse     - 0 = no,    1 = yes");
            System.out.println("upper_lower - 0 = lower, 1 = upper");
        } else
        {
            if(args.length > 1 && (new Integer(args[1])).intValue() > 0)
                changeDirFlag = true;
            if(args.length > 2 && (new Integer(args[2])).intValue() > 0)
                recurseSubdirFlag = true;
            if(args.length > 3 && (new Integer(args[3])).intValue() > 0)
                upperCaseFlag = true;
            if(changeDirFlag)
                modifyDirCase(args[0], recurseSubdirFlag, upperCaseFlag);
            else
                modifyFileCase(args[0], recurseSubdirFlag, upperCaseFlag);
        }
    }

    public static void modifyFileCase(String targetDir, boolean recurseSubdir, boolean upperCase)
    {
        File f = null;
        File fa[] = null;
        f = new File(targetDir);
        if(f.isDirectory())
            fa = f.listFiles();
        if(fa != null)
        {
            for(int i = 0; i < fa.length; i++)
                if(fa[i].isDirectory() && recurseSubdir)
                    modifyFileCase(fa[i].getAbsolutePath(), recurseSubdir, upperCase);
                else
                    internalChangeCase(fa[i], upperCase);

        }
    }

    public static void modifyDirCase(String targetDir, boolean recurseSubdir, boolean upperCase)
    {
        File f = null;
        File fa[] = null;
        f = new File(targetDir);
        if(f.isDirectory())
        {
            internalChangeCase(f, upperCase);
            fa = f.listFiles();
        }
        if(fa != null)
        {
            for(int i = 0; i < fa.length; i++)
                if(fa[i].isDirectory() && recurseSubdir)
                {
                    internalChangeCase(fa[i], upperCase);
                    modifyDirCase(fa[i].getAbsolutePath(), recurseSubdir, upperCase);
                }

        }
    }

    private static void internalChangeCase(File file, boolean upperCase)
    {
        boolean res = false;
        try
        {
            if(upperCase)
                res = file.renameTo(new File(file.getAbsolutePath().toUpperCase()));
            else
                res = file.renameTo(new File(file.getAbsolutePath().toLowerCase()));
        }
        catch(Exception e)
        {
            e.printStackTrace();
        }
    }
}
