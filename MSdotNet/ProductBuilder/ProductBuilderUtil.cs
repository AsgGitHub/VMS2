//**********************************************************************
//***** Copyright (c) 1992-2013 Allen Systems Group, Inc.  All rights reserved.
//**********************************************************************
//*****$Workfile:   ProductBuilderUtil.cs  $
//***** $Archive:   I:\Archives\CS\2\ProductBuilderUtil.c~  $
//*****$Revision:   2.1  $
//*****  $Author:   KOFORNAG  $
//*****    $Date:   13 Sep 2006 08:29:04  $
//***** $Modtime:   13 Sep 2006 08:28:46  $
//*****     $Log:   I:\Archives\CS\2\ProductBuilderUtil.c~  $
//*****
//*****   Rev 2.1   13 Sep 2006 08:29:04   KOFORNAG
//*****   Merge of Project EIBranchLibraryBuilder
//*****
//*****   Rev 2.0   12 Sep 2006 17:43:30   KOFORNAG
//*****   Create Branch Revision.
//*****
//*****   Rev 1.6   16 Jun 2005 17:56:44   DMASKOFF
//*****   DMASKOFF - Merge of Project EIVMSLBLinkedProjectsSupport
//*****
//*****   Rev 1.5   04 May 2005 17:27:58   TGALASSO
//*****Merge of Project FILBUnderscoreExt
//*****
//*****   Rev 1.4   03 Sep 2004 17:30:06   KOFORNAG
//*****Merge of Project EILbCommandLine
//*****
//*****   Rev 1.3   Jul 24 2003 17:32:46   RRUSSELL
//*****Merge of Project EIPRDBLDGUI-Misc01
//*****
//*****   Rev 1.2   Apr 18 2003 16:36:42   RRUSSELL
//*****Merge of Project EIPRDBLD-IntegrateWithLB2
//*****
//*****   Rev 1.1   11 Mar 2003 13:47:38   RRUSSELL
//*****Merge of Project EILIBBLD-IntegrateWithPB
//*****
//*****   Rev 1.0   17 Sep 2002 15:25:48   RRUSSELL
//*****Initialize Archive
//*****  $Endlog$
//**********************************************************************

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;

namespace LibraryBuilder {

    public class CollectionUtil {

        // CollectionUtil contains utilities used to serialize information to/from
        // .NET collection classes and flat text files.
        //
        // ReadPropertyFile takes a flat text file of the form <property>=<value>
        //     and reads this into a StringDictionary
        //
        // WritePropertyFile takes any .NET collection that implements IDictionary
        //     and generates a flat text property file
        //
        // ReadValueFile takes a flat text file with a single value per line
        //     and reads this into a StringCollection
        //
        // WriteValueFile takes any .NET collection that implements IEnumerable
        //     and generates a flat text value file
        //
        // CollectionToString takes any .NET collection that implements ICollection
        //     and returns a string of values delimited with the supplied delimiter
        //
        // StringToStringArray takes a string and returns a string array by splitting 
        //     the input string using the supplied delimiters


        public static StringDictionary ReadPropertyFile( string filename ) {

            StringDictionary properties = new StringDictionary();
            
            if ( !File.Exists(filename) )
                throw new Exception("Properties file, " + filename + ", not found.");
            
            StringCollection argErrors = new StringCollection();
            
            StreamReader fileReader = null;
            try {
                fileReader = new StreamReader( filename );	    
                string line;
                string[] prop_val_pair;
                char[] sep = new char[]{'='};
                while ( (line=fileReader.ReadLine()) != null ) {
                    line = line.Trim();
                    prop_val_pair = line.Split(sep);
                    if ( prop_val_pair.Length==2 ) {
                        properties.Add( prop_val_pair[0].Trim(), prop_val_pair[1].Trim() );
                        continue;
                    }
                    if ( line=="" ) {
                        continue;
                    }
                    argErrors.Add( line );
                }
            }
            finally {
                if (fileReader!=null) fileReader.Close();
            }

            if ( argErrors.Count > 0 ) {
                StringBuilder errmsg = new StringBuilder("Invalid properties supplied:\r\n");
                foreach ( string argerr in argErrors )
                    errmsg.Append(argerr + "\r\n");
                throw new Exception(errmsg.ToString());
            }

            return properties;

        }



        public static void WritePropertyFile( IDictionary properties, string filename ) {
            if ( File.Exists( filename ) )
                File.SetAttributes( filename, FileAttributes.Normal );
            StreamWriter fileWriter = null;
            try {
                fileWriter = new StreamWriter( filename, false );
                foreach ( string key in properties.Keys ) {
                    fileWriter.WriteLine(key + "=" + properties[key]);
                }
            }
            finally {
                if (fileWriter!=null) fileWriter.Close();
            }
        }




        public static StringCollection ReadValueFile( string filename ) {

            StringCollection values = new StringCollection();
            
            if ( !File.Exists(filename) )
                throw new Exception("Values file, " + filename + ", not found.");
            
            StreamReader fileReader = null;
            try {
                fileReader = new StreamReader( filename );	    
                string line;
                while ( (line=fileReader.ReadLine()) != null ) {
                    line = line.Trim();
                    if ( line!="" ) {
                        values.Add( line );
                    }  
                }
            }
            finally {
                if (fileReader!=null) fileReader.Close();
            }

            return values;

        }


        public static void WriteValueFile( IEnumerable values, string filename ) {
            if ( File.Exists( filename ) )
                File.SetAttributes( filename, FileAttributes.Normal );
            StreamWriter fileWriter = null;
            try {
                fileWriter = new StreamWriter( filename, false );
                foreach ( object value_ in values ) {
                    fileWriter.WriteLine(value_);
                }
            }
            finally {
                if (fileWriter!=null) fileWriter.Close();
            }
        }


        public static string CollectionToString( ICollection values, char delimiter ) {
            StringBuilder elementValues = new StringBuilder();
            foreach ( string value_ in values ) {
                elementValues.Append( value_.Trim() + delimiter );
            }
            return elementValues.ToString().TrimEnd( new char[] {delimiter} );
        }


        public static string[] StringToStringArray( string values, char[] delimiters ) {
            if (values==null)
                return new string[] {};
            else if (values.Trim()=="")
                return new string[] {};
            else
                return values.Split( delimiters );
        }

    }


    public class DatabaseUtil {

        // DatabaseUtil contains VMS database related utilities
        //
        // DateTime returns the current VMS database server time

        public static string sqlConnectionString  = LBEnvironment.DBConnectStr_VmsQuery;
        public static string sqlConnectionString2 = LBEnvironment.DBConnectStr_VmsUser;

        public static DateTime GetVMSServerTime() {

            DateTime vmsTime = new DateTime(0);
            SqlConnection vmsDatabase = null;
            SqlDataReader datetimeReader = null;

            try {

                vmsDatabase = new SqlConnection( DatabaseUtil.sqlConnectionString );
                vmsDatabase.Open();
                SqlCommand datetimeCmd = new SqlCommand("SELECT currentDate = getdate()", vmsDatabase);
                datetimeReader = datetimeCmd.ExecuteReader();
                datetimeReader.Read();
                vmsTime = datetimeReader.GetDateTime(0);

            }
            finally {
                if (vmsDatabase!=null) vmsDatabase.Close();
                if (datetimeReader!=null) datetimeReader.Close();
            }

            return vmsTime;
        }

    }
    
}

