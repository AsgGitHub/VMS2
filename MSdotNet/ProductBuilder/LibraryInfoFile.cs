//**********************************************************************
//***** Copyright (c) 1992-2013 Allen Systems Group, Inc.  All rights reserved.
//**********************************************************************
//*****$Workfile:   LibraryInfoFile.cs  $
//***** $Archive:   I:\Archives\CS\2\LibraryInfoFile.c~  $
//*****$Revision:   2.1  $
//*****  $Author:   KOFORNAG  $
//*****    $Date:   12 Oct 2006 16:57:24  $
//***** $Modtime:   12 Oct 2006 16:57:22  $
//*****     $Log:   I:\Archives\CS\2\LibraryInfoFile.c~  $
//*****
//*****   Rev 2.1   12 Oct 2006 16:57:24   KOFORNAG
//*****   Merge of Project EISplitProductBuilder
//*****
//*****   Rev 2.0   11 Oct 2006 11:52:00   KOFORNAG
//*****   Create Branch Revision.
//*****  $Endlog$
//**********************************************************************
using System;
using System.IO;

namespace ProductBuilderLibrary {

    public class LibraryInfoFile {

        private static string libraryInfoFilename = "LibraryInfo.cfg";

        private string name = "";
        private string version = "";
        private string platform = "";
        private string level = "";
        private string directory = "";
        private string date = "";

        private static string nameLabel = "ProductName";
        private static string versionLabel = "ProductVersion";
        private static string platformLabel = "ProductPlatform";
        private static string levelLabel = "ProductLevel";
        private static string directoryLabel = "ProductDirectory";
        private static string dateLabel = "UpdateDate";
        
        public LibraryInfoFile( 
                string name, 
                string version, 
                string platform, 
                string level, 
                string directory,
                string date ) {
            this.name = name;
            this.version = version;
            this.platform = platform;
            this.level = level;
            this.directory = directory;
            this.date = date;
        }

        public LibraryInfoFile(string filepath) {
            LoadFromPath(filepath);
        }


        public void LoadFromPath( string filepath ) {

            string filename = Path.Combine( filepath, libraryInfoFilename );
            
            if ( !File.Exists(filename) )
                throw new Exception("Values file, " + filename + ", not found.");
            
            StreamReader fileReader = null;
            try {
                fileReader = new StreamReader( filename );	    
                string line;
                while ( (line=fileReader.ReadLine()) != null ) {
                    line = line.Trim();
                    int equalPos = line.IndexOf("=");
                    if (equalPos >= 0) {
                        string label = line.Substring(0, equalPos);
                        string item = line.Substring(equalPos+1, line.Length-equalPos-1);
                        if (label.ToUpper() == nameLabel.ToUpper())      name = item;
                        if (label.ToUpper() == versionLabel.ToUpper())   version = item;
                        if (label.ToUpper() == platformLabel.ToUpper())  platform = item;
                        if (label.ToUpper() == levelLabel.ToUpper())     level = item;
                        if (label.ToUpper() == directoryLabel.ToUpper()) directory = item;
                        if (label.ToUpper() == dateLabel.ToUpper())      date = item;
                    } 
                }
            }
            finally {
                if (fileReader!=null) fileReader.Close();
            }

        }


        public void SaveToPath(string filepath) {

            string filename = Path.Combine( filepath, libraryInfoFilename );

            if ( File.Exists( filename ) )
                File.SetAttributes( filename, FileAttributes.Normal );
            StreamWriter fileWriter = null;
            try {
                fileWriter = new StreamWriter( filename, false );
                fileWriter.WriteLine(nameLabel      + "=" + name);
                fileWriter.WriteLine(versionLabel   + "=" + version);
                fileWriter.WriteLine(platformLabel  + "=" + platform);
                fileWriter.WriteLine(levelLabel     + "=" + level);
                fileWriter.WriteLine(directoryLabel + "=" + directory);
                fileWriter.WriteLine(dateLabel      + "=" + date);
            }
            finally {
                if (fileWriter!=null) fileWriter.Close();
            }
        }

        public string GetName() {
            return name;
        }

        public void SetName( string name ) {
            this.name = name;
        }

        public string GetVersion() {
            return version;
        }

        public void SetVersion( string version ) {
            this.version = version;
        }

        public string GetPlatform() {
            return platform;
        }

        public void SetPlatform( string platform ) {
            this.platform = platform;
        }

        public string GetLevel() {
            return level;
        }

        public void SetLevel( string level ) {
            this.level = level;
        }

        public string GetDirectory() {
            return directory;
        }

        public void SetDirectory( string directory ) {
            this.directory = directory;
        }

        public string GetDate() {
            return date;
        }

        public void SetDate( string date ) {
            this.date = date;
        }


    }

}
