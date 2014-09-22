//**********************************************************************
//***** Copyright (c) 1992-2013 Allen Systems Group, Inc.  All rights reserved.
//**********************************************************************
//*****$Workfile:   ProductBuildDataSet.cs  $
//***** $Archive:   I:\Archives\CS\2\ProductBuildDataSet.c~  $
//*****$Revision:   2.4  $
//*****  $Author:   shermank  $
//*****    $Date:   26 Mar 2009 10:11:46  $
//***** $Modtime:   26 Mar 2009 10:11:44  $
//*****     $Log:   I:\Archives\CS\2\ProductBuildDataSet.c~  $
//*****
//*****   Rev 2.4   26 Mar 2009 10:11:46   shermank
//*****   Merge of Project EIVMSSupportASGUserName
//*****
//*****   Rev 2.3   01 Dec 2006 17:42:44   KOFORNAG
//*****   Merge of Project EIBranchProjectsSchemaRelease
//*****
//*****   Rev 2.2   22 Nov 2006 14:56:46   KOFORNAG
//*****   Merge of Project EIBranchProjectsPb
//*****
//*****   Rev 2.1   13 Sep 2006 08:29:02   KOFORNAG
//*****   Merge of Project EIBranchLibraryBuilder
//*****
//*****   Rev 2.0   12 Sep 2006 17:43:28   KOFORNAG
//*****   Create Branch Revision.
//*****
//*****   Rev 1.8   04 May 2005 17:27:58   TGALASSO
//*****Merge of Project FILBUnderscoreExt
//*****
//*****   Rev 1.7   23 Sep 2004 18:07:50   KOFORNAG
//*****Merge of Project FIPreprodPartialBuilds
//*****
//*****   Rev 1.6   03 Sep 2004 17:30:04   KOFORNAG
//*****Merge of Project EILbCommandLine
//*****
//*****   Rev 1.5   Oct 17 2003 16:15:50   RRUSSELL
//*****Merge of Project EILIBBLD-Misc01
//*****
//*****   Rev 1.4   Jul 24 2003 17:32:44   RRUSSELL
//*****Merge of Project EIPRDBLDGUI-Misc01
//*****
//*****   Rev 1.3   Apr 18 2003 16:36:36   RRUSSELL
//*****Merge of Project EIPRDBLD-IntegrateWithLB2
//*****
//*****   Rev 1.2   Mar 21 2003 18:47:04   RRUSSELL
//*****Merge of Project EILIBBLD-IntegrateWithPB2
//*****
//*****   Rev 1.1   11 Mar 2003 13:47:34   RRUSSELL
//*****Merge of Project EILIBBLD-IntegrateWithPB
//*****
//*****   Rev 1.0   18 Feb 2003 15:42:00   RRUSSELL
//*****Initialize Archive
//*****  $Endlog$
//**********************************************************************

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Xml;


namespace LibraryBuilder {

    public class ProductBuildDataSet : DataSet {

        #region Static Members
        public static DataSetObjectNames on = new DataSetObjectNames();
        private static ProductBuildDataSet baseDataSet = null;
        #endregion


        #region Inner Classes

        public class DataSetObjectNames {
        
            public string BuildDefinition = "BuildDefinition";
            public string BuildProperty = "BuildProperty";
            public string BuildProperty_Name = "Name";
            public string BuildProperty_Value = "Value";
            public string BuildProperty_AllowEdit = "AllowEdit";
            public string Component = "Component";
            public string Component_Name = "Name";
            public string Component_Descriptor = "Descriptor";
            public string Component_DescriptorExtension = "DescriptorExtension";
            public string Component_Build = "Build";
            public string Target = "Target";
            public string Target_ComponentName = "ComponentName";
            public string Target_TargetName = "TargetName";
            public string Target_TargetExtension = "TargetExtension";
            public string Target_Check = "Check";
            public string ComponentExclusion = "ComponentExclusion";
            public string ComponentExclusion_Name = "Name";
            public string TargetExclusion = "TargetExclusion";
            public string TargetExclusion_Name = "Name";
            public string TargetExtensionExclusion = "TargetExtensionExclusion";
            public string TargetExtensionExclusion_Name = "Name";
            public string ComponentTargets = "ComponentTargets";

        }
        
        private class LibraryInfo {

            public string productName;
            public string productVersion;
            public string productPlatform;
            public string branchOrTrunckName;
            public string productDirectory;
            public string libraryPath;
            public string updateDate;

            public LibraryInfo( 
                string productName,
                string productVersion,
                string productPlatform,
                string branchOrTrunckName,
                string productDirectory,
                string libraryPath,
                string updateDate ) {

                this.productName = productName;
                this.productVersion = productVersion;
                this.productPlatform = productPlatform;
                this.branchOrTrunckName = branchOrTrunckName;
                this.productDirectory = productDirectory;
                this.libraryPath = libraryPath;
                this.updateDate = updateDate;
                if ( updateDate==null || updateDate==DateTime.MinValue.ToString() ) {
                    this.updateDate = DatabaseUtil.GetVMSServerTime().ToString();
                }
            }

        }
        #endregion


        #region Constructor
        public ProductBuildDataSet() {
            this.DataSetName = on.BuildDefinition;
            this.Tables.Add( on.BuildProperty );
            this.Tables.Add( on.Component );
            this.Tables.Add( on.Target );
            this.Tables.Add( on.ComponentExclusion );
            this.Tables.Add( on.TargetExclusion );
            this.Tables.Add( on.TargetExtensionExclusion );
            
            this.Tables[on.BuildProperty].Columns.Add(on.BuildProperty_Name, Type.GetType("System.String"));
            this.Tables[on.BuildProperty].Columns.Add(on.BuildProperty_Value, Type.GetType("System.String"));
            this.Tables[on.BuildProperty].Columns.Add(on.BuildProperty_AllowEdit, Type.GetType("System.Boolean"));
            this.Tables[on.BuildProperty].Columns[on.BuildProperty_AllowEdit].DefaultValue = true;
            this.Tables[on.BuildProperty].PrimaryKey = new DataColumn[] {this.Tables[on.BuildProperty].Columns[on.BuildProperty_Name]};

            this.Tables[on.Component].Columns.Add(on.Component_Name, Type.GetType("System.String"));
            this.Tables[on.Component].Columns.Add(on.Component_Descriptor, Type.GetType("System.String"));
            this.Tables[on.Component].Columns.Add(on.Component_DescriptorExtension, Type.GetType("System.String"));
            this.Tables[on.Component].Columns.Add(on.Component_Build, Type.GetType("System.Boolean"));
            this.Tables[on.Component].PrimaryKey = new DataColumn[] {this.Tables[on.Component].Columns[on.Component_Name]};

            this.Tables[on.Target].Columns.Add(on.Target_ComponentName, Type.GetType("System.String"));
            this.Tables[on.Target].Columns.Add(on.Target_TargetName, Type.GetType("System.String"));
            this.Tables[on.Target].Columns.Add(on.Target_TargetExtension, Type.GetType("System.String"));
            this.Tables[on.Target].Columns.Add(on.Target_Check, Type.GetType("System.Boolean"));
            this.Tables[on.Target].PrimaryKey = new DataColumn[] {this.Tables[on.Target].Columns[on.Target_ComponentName],
                                                                     this.Tables[on.Target].Columns[on.Target_TargetName]};

            this.Tables[on.ComponentExclusion].Columns.Add(on.ComponentExclusion_Name, Type.GetType("System.String"));
            this.Tables[on.ComponentExclusion].PrimaryKey = new DataColumn[] {this.Tables[on.ComponentExclusion].Columns[on.ComponentExclusion_Name]};

            this.Tables[on.TargetExclusion].Columns.Add(on.TargetExclusion_Name, Type.GetType("System.String"));
            this.Tables[on.TargetExclusion].PrimaryKey = new DataColumn[] {this.Tables[on.TargetExclusion].Columns[on.TargetExclusion_Name]};

            this.Tables[on.TargetExtensionExclusion].Columns.Add(on.TargetExtensionExclusion_Name, Type.GetType("System.String"));
            this.Tables[on.TargetExtensionExclusion].PrimaryKey = new DataColumn[] {this.Tables[on.TargetExtensionExclusion].Columns[on.TargetExtensionExclusion_Name]};

            this.Relations.Add(on.ComponentTargets, this.Tables[on.Component].Columns[on.Component_Name], this.Tables[on.Target].Columns[on.Target_ComponentName], true);
            this.Relations[on.ComponentTargets].Nested = true;
        }
        #endregion


        #region Static Object Initializer Methods

        static public ProductBuildDataSet GetProductBuildDataSetWithoutComplist(
            string profileName,
            string productName,
            string productVersion,
            string productPlatform,
            string branchOrTrunkName,
            string productDirectory,
            string libraryPath,
            string updateDate) {

            ProductBuildDataSet pbDataSet = new ProductBuildDataSet();

            LibraryInfo libraryInfo = new LibraryInfo(productName, productVersion, productPlatform, branchOrTrunkName, productDirectory, libraryPath, updateDate);

            pbDataSet.Merge( GetProfileDataSet("default (Admin)"), false, MissingSchemaAction.Ignore );
            pbDataSet.Merge( GetProductSpecificDataSet(libraryInfo), false, MissingSchemaAction.Ignore );
            pbDataSet.Merge( GetProfileDataSet(profileName), false, MissingSchemaAction.Ignore );
            // Override P:\ProductBuilder environment variables in the default profile
            string PBPath = Environment.GetEnvironmentVariable("PBPath");
            if (PBPath == null)
                PBPath = "C:\\ASG\\VMS\\ProductBuilder";  // Default productbuilder client path
            if (PBPath.Contains(" "))
                PBPath = "\""+PBPath+"\"";
            pbDataSet.SetBuildProperty("PBPath", PBPath, false);
            pbDataSet.SetBuildProperty("PBToolsPath", PBPath+"\\tools2", false);
            pbDataSet.SetBuildProperty("PBDataPath", PBPath + "\\Data2", false);
            pbDataSet.InitLibraryData(libraryInfo);
            pbDataSet.FinalizeProperties();

            pbDataSet.Tables[on.Target].Clear();
            pbDataSet.Tables[on.Component].Clear(); 
            pbDataSet.AcceptChanges();

            return pbDataSet;
        }


        static public ProductBuildDataSet ReadBuildConfigurationFile( string filename ) {
            ProductBuildDataSet pbDataSet = new ProductBuildDataSet();
            XmlTextReader xtr = null;
            try {
                if ( File.Exists( filename ) ) {
                    xtr = new XmlTextReader( filename );
                    pbDataSet.ReadXml( xtr );
                }
                else {
                    throw new Exception("The build configuration file, " + filename + ", does not exist.");
                }
            }
            finally {
                if (xtr!=null) xtr.Close();
            }
            return pbDataSet;
        }


        static public ProductBuildDataSet GetProfileDataSet( string profileName ) {

            ProductBuildDataSet profileDataSet = new ProductBuildDataSet();
            XmlTextReader xtr = null;
            try {
                XmlParserContext xpc = new XmlParserContext( null, new XmlNamespaceManager( new NameTable() ), null, XmlSpace.None );
                xtr = new XmlTextReader( GetBuildProfile( profileName ), XmlNodeType.Document, xpc );
                profileDataSet.ReadXml( xtr );
                profileDataSet.AcceptChanges();
                profileDataSet.SetBuildProperty( "BuildProfileName", profileName, false );
            }
            finally {
                if (xtr!=null) xtr.Close();
            }
            return profileDataSet;

        }

        static private ProductBuildDataSet GetProductSpecificDataSet( LibraryInfo libraryInfo ) {

            ProductBuildDataSet productSpecificDataSet = new ProductBuildDataSet();
            XmlTextReader xtr = null;
            try {
                string productPropertiesFilename = GetProductPropertiesFilename( libraryInfo );
                if ( File.Exists( productPropertiesFilename ) ) {
                    xtr = new XmlTextReader( GetProductPropertiesFilename( libraryInfo ) );
                    productSpecificDataSet.ReadXml( xtr );
                }
                string emailRecipients="";
                productSpecificDataSet.SetBuildProperty("EmailRecipients", emailRecipients, true);
            }
            finally {
                if (xtr!=null) xtr.Close();
            }
            return productSpecificDataSet;

        }

        static private ProductBuildDataSet GetComponentDataSet( LibraryInfo libraryInfo ) {

            ProductBuildDataSet componentDataSet = new ProductBuildDataSet();

            SqlConnection vmsDatabase = null;
            SqlDataReader complistReader = null;

            try {

                vmsDatabase = new SqlConnection( DatabaseUtil.sqlConnectionString );
                vmsDatabase.Open();

                SqlCommand complistCmd = new SqlCommand("GET_BUILD_COMPILATION_LIST", vmsDatabase);
                complistCmd.CommandType = CommandType.StoredProcedure;
                complistCmd.CommandTimeout = 180;
                complistCmd.Parameters.Add("@productName",        SqlDbType.VarChar).Value = libraryInfo.productName;
                complistCmd.Parameters.Add("@productVer",         SqlDbType.VarChar).Value = libraryInfo.productVersion;
                complistCmd.Parameters.Add("@platCode",           SqlDbType.VarChar).Value = libraryInfo.productPlatform;
                complistCmd.Parameters.Add("@level", SqlDbType.VarChar).Value = libraryInfo.branchOrTrunckName;
                complistCmd.Parameters.Add("@date",               SqlDbType.DateTime).Value = (libraryInfo.updateDate!=null) ? (Object)libraryInfo.updateDate : DBNull.Value;
                complistCmd.Parameters.Add("RETURN_VALUE",        SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                complistReader = complistCmd.ExecuteReader();
                while (complistReader.Read()) {
                    string descriptorName = complistReader[1].ToString();
                    string targetName = complistReader[2].ToString();
                    int extensionPos = descriptorName.LastIndexOf(".") + 1;
                    string descriptorExtension = (extensionPos > 0) ? descriptorName.Substring(extensionPos).ToLower() : "";
                    extensionPos = targetName.LastIndexOf(".") + 1;
                    string targetExtension = (extensionPos > 0) ? targetName.Substring(extensionPos).ToLower() : "";
                    componentDataSet.Tables[on.Component].LoadDataRow( new object[] { complistReader[0], descriptorName, descriptorExtension, true }, true );
                    componentDataSet.Tables[on.Target].LoadDataRow( new object[] { complistReader[0], targetName, targetExtension, true }, true );
                }

            }
            finally {
                if (vmsDatabase!=null) vmsDatabase.Close();
                if (complistReader!=null) complistReader.Close();
            }

            return componentDataSet;
        }

        #endregion


        #region Member Functions


        private void InitLibraryData( LibraryInfo li ) {
            string localProductDirectoryRoot = GetBuildProperty("LocalProductDirectoryRoot");
            string productPath = li.productDirectory + "\\v" + li.productVersion.Replace('.', '_') + "." + li.productPlatform;
            string buildPath = li.libraryPath;
            string updateDate = li.updateDate;
            if ( buildPath.ToUpper().IndexOf(LBEnvironment.NetworkProductRootDir.ToUpper()) >= 0 ) {
                buildPath = Path.Combine( localProductDirectoryRoot, productPath + "\\NETWORK_" + li.branchOrTrunckName + "_BUILD"  );
                //if a direct copy from the network, use the current database datetime
                //refresh the LibraryInfo file in ProductBuild.cs
                updateDate = DatabaseUtil.GetVMSServerTime().ToString();
            }
            string stablePath = Path.Combine( LBEnvironment.NetworkProductRootDir, productPath + "\\" + "STABLE" );

            SetBuildProperty( "ProductName", li.productName, false );
            SetBuildProperty( "ProductVersion", li.productVersion, false );
            SetBuildProperty( "ProductPlatform", li.productPlatform, false );
            SetBuildProperty( "BranchOrTrunkName", li.branchOrTrunckName, false );
            SetBuildProperty( "ProductDirectory", li.productDirectory, false );
            SetBuildProperty( "LibraryDate", updateDate, false );
            SetBuildProperty( "BuildPath", buildPath, true );
            SetBuildProperty( "SourcePath", li.libraryPath, false );
            SetBuildProperty( "PopulatePath", li.libraryPath, true );
            SetBuildProperty( "StablePath", stablePath, false );

            string toRecipients = ProcessEmailRecipients( GetBuildProperty("EmailRecipients"), true );
            SetBuildProperty( "EmailRecipients", toRecipients, true );

        }

        public void FinalizeProperties() {
            string buildPath = GetBuildProperty("BuildPath");
            if ( Path.GetPathRoot( buildPath ) == "Q:" )
                throw new Exception("Cannot map " + buildPath + " to Q:" );
            buildPath = Path.GetFullPath( buildPath );
            string buildDefinitionFileName = Path.Combine( buildPath, "build_cfg\\build_definition.xml" );
            string buildTimesFileName = Path.Combine( buildPath, "build_temp\\build_times.txt" );
            string buildLogFileName = Path.Combine( buildPath, "build_cfg\\build.log" );
            string buildStepStatusFileName = Path.Combine( buildPath, "build_temp\\buildstep_status.txt" );
            string checkBuildResultsFileName = Path.Combine( buildPath, "build_temp\\checkbuild_results.txt" );
            string buildBatchFileName = Path.Combine( buildPath, this.GetBuildBatchFileName() );
            SetBuildProperty( "BuildPath", buildPath, true );
            SetBuildProperty( "BuildTimesFileName", buildTimesFileName, false );
            SetBuildProperty( "BuildLogFileName", buildLogFileName, false );
            SetBuildProperty( "BuildStepStatusFileName", buildStepStatusFileName, false );
            SetBuildProperty( "CheckBuildResultsFileName", checkBuildResultsFileName, false );
            SetBuildProperty( "BuildDefinitionFileName", buildDefinitionFileName, false );
            SetBuildProperty( "BuildBatchFileName", buildBatchFileName, false );

            string toRecipients = ProcessEmailRecipients( GetBuildProperty("EmailRecipients"), false );
            SetBuildProperty( "EmailRecipients", toRecipients, true );


            //Remove before merge
            //SetBuildProperty( "PBPath", "P:\\projects\\EIBranchProjectsPb", false );
            //SetBuildProperty( "PBToolsPath", "P:\\projects\\EIBranchProjectsPb", false );
            //end remove

            SetCompatibilityProperties();
        }


        private void SetCompatibilityProperties() {

            // these properties must exist in the default profile or be loaded from the library Info file
            string ProdBld_Name = GetBuildProperty("ProductName");
            string ProdBld_Ver = GetBuildProperty("ProductVersion");
            string ProdBld_Platform = GetBuildProperty("ProductPlatform");
            string ProdBld_BranchOrTrunkName = GetBuildProperty("BranchOrTrunkName");
            string ProdBld_BuildOrMake = GetBuildProperty("BuildOrMake");
            string ProdBld_ReleaseOrDebug = GetBuildProperty("ReleaseOrDebug");
            string ProdBld_MaxFileAge = GetBuildProperty("MaximumTargetAge");
            string ProdBld_Path = GetBuildProperty("PBPath");
            string ProdBld_ToolsPath = GetBuildProperty("PBToolsPath");
            SetBuildProperty( "ProdBld_Name", ProdBld_Name, false );
            SetBuildProperty( "ProdBld_Ver", ProdBld_Ver, false );
            SetBuildProperty( "ProdBld_Platform", ProdBld_Platform, false );
            SetBuildProperty( "ProdBld_BranchOrTrunkName", ProdBld_BranchOrTrunkName, false );
            SetBuildProperty( "ProdBld_BuildOrMake", ProdBld_BuildOrMake, false );
            SetBuildProperty( "ProdBld_ReleaseOrDebug", ProdBld_ReleaseOrDebug, false );
            SetBuildProperty( "ProdBld_MaxFileAge", ProdBld_MaxFileAge, false );
            SetBuildProperty( "ProdBld_Path", ProdBld_Path, false );
            SetBuildProperty( "ProdBld_ToolsPath", ProdBld_ToolsPath, false );

            //this is done for backward compatibilty with older versions of BuildMedia.bat
            //without this field the enviroment variable will not be set, and media building will fail,
            //resulting overall build failure. 
            SetBuildProperty( "ProductLevel", ProdBld_BranchOrTrunkName, false );

            // the following properties might not exist - check before assignment
            string buildPath = GetBuildProperty("BuildPath");
            if ( buildPath != null ) {
                string ProdBld_BuildPath = buildPath;
                string ProdBld_HomeOrAway = ( ProdBld_BuildPath.ToUpper().IndexOf(LBEnvironment.NetworkProductRootDir.ToUpper()) >= 0 ) ? "Home" : "Away";
                SetBuildProperty( "ProdBld_BuildPath", ProdBld_BuildPath, false );
                SetBuildProperty( "ProdBld_HomeOrAway", ProdBld_HomeOrAway, false );
            }
        }


        public void LoadComplist() {
            string productName = GetBuildProperty("ProductName");
            string productVersion = GetBuildProperty("ProductVersion");
            string productPlatform = GetBuildProperty("ProductPlatform");
            string productBranchOrTrunkName = GetBuildProperty("BranchOrTrunkName");
            string productDirectory = GetBuildProperty("ProductDirectory");
            string libraryPath = GetBuildProperty("BuildPath");
            string updateDate = GetBuildProperty("LibraryDate");
            LibraryInfo libraryInfo = new LibraryInfo(productName, productVersion, productPlatform, productBranchOrTrunkName, productDirectory, libraryPath, updateDate);
            Merge( GetComponentDataSet(libraryInfo), false, MissingSchemaAction.Ignore );
            ResolveExclusionLists();
            AcceptChanges();
        }


        public void ApplyCommandLineProperties( Hashtable commandLineBuildProperties ) {
            foreach ( string key in commandLineBuildProperties.Keys ) {
                SetBuildProperty( (string)key, (string)commandLineBuildProperties[key], true );
            }
        }

        public bool IsPartialBuild(){
            bool returnValue = false;
            foreach (DataRow componentRow in this.Tables[on.Component].Rows) {
                if (Convert.ToBoolean(componentRow[on.Component_Build]) == false) {
                    returnValue = true;
                    break;
                }
            }
            return returnValue;
        }

        private void ResolveExclusionLists() {
            
            // ComponentExclusions
            foreach ( DataRow componentExclusionRow in this.Tables[on.ComponentExclusion].Rows ) {
                string excludedComponentName = componentExclusionRow[on.ComponentExclusion_Name].ToString();
                foreach ( DataRow componentRow in this.Tables[on.Component].Rows ) {
                    if ( excludedComponentName.ToLower() == componentRow[on.Component_Name].ToString().ToLower() ) {
                        componentRow[on.Component_Build] = false;
                    }
                }
                foreach ( DataRow targetRow in this.Tables[on.Target].Rows ) {
                    if ( excludedComponentName.ToLower() == targetRow[on.Target_ComponentName].ToString().ToLower() ) {
                        targetRow[on.Target_Check] = false;
                    }
                }
            }

            // TargetExclusions
            foreach ( DataRow targetExclusionRow in this.Tables[on.TargetExclusion].Rows ) {
                string excludedTargetName = targetExclusionRow[on.TargetExclusion_Name].ToString();
                foreach ( DataRow targetRow in this.Tables[on.Target].Rows ) {
                    if ( excludedTargetName.ToLower() == targetRow[on.Target_TargetName].ToString().ToLower() ) {
                        targetRow[on.Target_Check] = false;
                    }
                }
            }

            // TargetExtensionExclusions
            foreach ( DataRow targetExtensionExclusionRow in this.Tables[on.TargetExtensionExclusion].Rows ) {
                string excludedTargetExtensionName = targetExtensionExclusionRow[on.TargetExtensionExclusion_Name].ToString();
                foreach ( DataRow targetRow in this.Tables[on.Target].Rows ) {
                    if ( excludedTargetExtensionName.ToLower() == targetRow[on.Target_TargetExtension].ToString().ToLower() ) {
                        targetRow[on.Target_Check] = false;
                    }
                }
            }
        }


        public string GetBuildConfigurationFilename() {
            return GetBuildProperty("BuildDefinitionFileName");
        }


        public void WriteBuildConfigurationFile( string filename ) {
            string buildConfigurationDirectory = Path.Combine( Path.GetPathRoot(filename), Path.GetDirectoryName(filename) );
            if ( !Directory.Exists( buildConfigurationDirectory ) ) {
                Directory.CreateDirectory( buildConfigurationDirectory );
            }
            if ( File.Exists( filename ) )
                File.SetAttributes( filename, FileAttributes.Normal );
            XmlTextWriter textWriter = new XmlTextWriter(filename, null);
            textWriter.Formatting = Formatting.Indented;
            this.WriteXml( textWriter, XmlWriteMode.WriteSchema );
            textWriter.Close();
        }

        public bool SourcePathEqualsBuildPath() {
            string sourcePath = GetBuildProperty("SourcePath");
            string buildPath = GetBuildProperty("BuildPath");
            return ( Path.GetFullPath(sourcePath).ToUpper() == Path.GetFullPath(buildPath).ToUpper() );
        }

        public string GetBuildProperty( string name ) {
            DataRow row = this.Tables[on.BuildProperty].Rows.Find( name );
            return (row!=null) ? row[on.BuildProperty_Value].ToString() : null;
        }

        public void SetBuildProperty( string name, string value, bool allowEdit ) {
            bool readOnly = (name == "buildPath")? false : true;
            DataRow oldRow = this.Tables[on.BuildProperty].Rows.Find( name );
            if ( oldRow != null ) {
                this.Tables[on.BuildProperty].Rows.Remove( oldRow );
            }
            this.Tables[on.BuildProperty].LoadDataRow(new Object[] {name, value, allowEdit}, readOnly);
        }

        public SortedList GetBuildProperties() {
            SortedList buildProperties = new SortedList();
            foreach ( DataRow row in this.Tables[on.BuildProperty].Rows ) {
                buildProperties.Add( row[on.BuildProperty_Name], row[on.BuildProperty_Value] );
            }
            return buildProperties;
        }

        public void ListBuildProperties() {
            foreach (DataRow row in this.Tables[on.BuildProperty].Rows)
                Console.WriteLine(row[on.BuildProperty_Name]+"="+row[on.BuildProperty_Value]);
        }
        public ArrayList GetTargets( bool check ) {
            ArrayList targets = new ArrayList();
            string filterString = ( check ) ? "Check = true" : "Check = false";
            DataRow[] targetRows = this.Tables[on.Target].Select( filterString );
            foreach ( DataRow targetRow in targetRows ) {
                targets.Add( targetRow[on.Target_TargetName] );
            }
            targets.Sort();
            return targets;

        }

        public string GetBuildBatchFileName() {
            return "bat\\build_" + GetBuildProperty("ProductDirectory") + "_" 
                + GetBuildProperty("ProductVersion").Replace(".", "") + "_" 
                + GetBuildProperty("ProductPlatform") + ".bat";
        }

        public bool SourceBuildBatchFileExists() {
            string sourceBuildBatchFileName = Path.Combine( GetBuildProperty("SourcePath"), GetBuildBatchFileName() );
            return File.Exists( sourceBuildBatchFileName );
        }


        public bool SaveProfile( string profileName ) {
            bool savedProfile = false;
            SqlConnection currentConnection = null;
            try {

                int rowsAffected;

                string saveProfileCmdText  = "update BUILD_PROFILE ";
                saveProfileCmdText += "set XML_TEXT = @xml_text ";
                saveProfileCmdText += "where PROFILE_NAME + ' (' + PROFILE_NAMESPACE + ')' = @profile_name ";
                saveProfileCmdText += "and OWNER = @owner ";

                currentConnection = new SqlConnection( DatabaseUtil.sqlConnectionString2 );
                currentConnection.Open();
                SqlCommand saveProfileCmd = currentConnection.CreateCommand();
                saveProfileCmd.CommandText = saveProfileCmdText;
                saveProfileCmd.Parameters.Add("@profile_name", SqlDbType.VarChar).Value = profileName;
                DatabaseInterface databaseInterface = new DatabaseInterface();
                saveProfileCmd.Parameters.Add("@owner", SqlDbType.VarChar).Value = databaseInterface.FilterUserName(Environment.UserName);
                saveProfileCmd.Parameters.Add("@xml_text", SqlDbType.Text).Value = this.GetXml();

                savedProfile = ( (rowsAffected=saveProfileCmd.ExecuteNonQuery()) == 1 );

            }
            catch ( Exception ex ) {
                ex.GetType();
            }
            finally {
                if (currentConnection!=null) currentConnection.Close();
            }
            return savedProfile;
        }

        #endregion


        #region Static Utility Functions
        
        public static void SetBaseDataSet( ProductBuildDataSet pbDataSet ) {
            ProductBuildDataSet.baseDataSet = (ProductBuildDataSet)pbDataSet.Copy();
        }

        public static ProductBuildDataSet GetBaseDataSet() {
            return ProductBuildDataSet.baseDataSet;
        }

        public static ArrayList GetBuildProfileList( string ownerID ) {

            ArrayList buildProfileList = null;
            SqlConnection currentConnection = null;
            SqlDataReader buildProfileListReader = null;
            try {
                buildProfileList = new ArrayList();
                string buildProfileListCmdText  = "select PROFILE_NAME + ' (' + PROFILE_NAMESPACE + ')' ";
                buildProfileListCmdText += "from BUILD_PROFILE ";
                buildProfileListCmdText += "where PROFILE_NAMESPACE = @ownerID or PROFILE_NAMESPACE = 'Public' ";
                buildProfileListCmdText += "order by PROFILE_NAME ";

                currentConnection = new SqlConnection( DatabaseUtil.sqlConnectionString );
                currentConnection.Open();
                SqlCommand buildProfileListCmd = currentConnection.CreateCommand();
                buildProfileListCmd.CommandText = buildProfileListCmdText;
                buildProfileListCmd.Parameters.Add("@ownerID", SqlDbType.VarChar).Value = ownerID;

                buildProfileListReader = buildProfileListCmd.ExecuteReader();
                while (buildProfileListReader.Read()) {
                    buildProfileList.Add( buildProfileListReader.GetString(0) );
                }
            }
            finally {
                if (currentConnection!=null) currentConnection.Close();
                if (buildProfileListReader!=null) buildProfileListReader.Close();
            }
            return buildProfileList;

        }


        public static bool IsBuildProfileEditable( string profileName ) {

            bool isEditable = false;
            SqlConnection currentConnection = null;
            SqlDataReader isEditableReader = null;
            try {
                string isEditableCmdText  = "select count(*) ";
                isEditableCmdText += "from BUILD_PROFILE ";
                isEditableCmdText += "where PROFILE_NAME + ' (' + PROFILE_NAMESPACE + ')' = @profileName ";
                isEditableCmdText += "and OWNER = @ownerID ";

                currentConnection = new SqlConnection( DatabaseUtil.sqlConnectionString );
                currentConnection.Open();
                SqlCommand isEditableCmd = currentConnection.CreateCommand();
                isEditableCmd.CommandText = isEditableCmdText;
                isEditableCmd.Parameters.Add("@profileName", SqlDbType.VarChar).Value = profileName;
                DatabaseInterface databaseInterface = new DatabaseInterface();
                isEditableCmd.Parameters.Add("@ownerID", SqlDbType.VarChar).Value = databaseInterface.FilterUserName(Environment.UserName);

                isEditableReader = isEditableCmd.ExecuteReader();
                if (isEditableReader.Read()) {
                    isEditable = (isEditableReader.GetInt32(0)==1);
                }
            }
            finally {
                if (currentConnection!=null) currentConnection.Close();
                if (isEditableReader!=null) isEditableReader.Close();
            }
            return isEditable;

        }


        private static string GetBuildProfile( string profileName ) {

            string buildProfile = null;
            SqlConnection currentConnection = null;
            SqlDataReader buildProfileReader = null;
            try {
                string buildProfileCmdText  = "select XML_TEXT ";
                buildProfileCmdText += "from BUILD_PROFILE ";
                buildProfileCmdText += "where PROFILE_NAME + ' (' + PROFILE_NAMESPACE + ')' = @profileName ";

                currentConnection = new SqlConnection( DatabaseUtil.sqlConnectionString );
                currentConnection.Open();
                SqlCommand buildProfileCmd = currentConnection.CreateCommand();
                buildProfileCmd.CommandText = buildProfileCmdText;
                buildProfileCmd.Parameters.Add("@profileName", SqlDbType.VarChar).Value = profileName;

                buildProfileReader = buildProfileCmd.ExecuteReader();
                if (buildProfileReader.Read()) {
                    buildProfile = buildProfileReader.GetString(0);
                }
            }
            finally {
                if (currentConnection!=null) currentConnection.Close();
                if (buildProfileReader!=null) buildProfileReader.Close();
            }
            return buildProfile;

        }

        static private string GetProductPropertiesFilename( LibraryInfo li ) {
            return Path.Combine( li.libraryPath, 
                "xml\\buildcfg_" + li.productDirectory + "_" + li.productVersion.Replace(".", "") + "_" + li.productPlatform + ".xml" );
        }

        static public string GetBuildConfigurationFilename( string buildPath ) {
            return Path.Combine( buildPath, "build_cfg\\build_definition.xml" );
        }


        public static ArrayList GetFilteredComplist( string buildPath, string filterString ) {
            string buildDefinitionFile = ProductBuildDataSet.GetBuildConfigurationFilename( buildPath );
            ProductBuildDataSet pbDataSet = ProductBuildDataSet.ReadBuildConfigurationFile( buildDefinitionFile );
            ArrayList filteredComplist = new ArrayList();
            DataRow[] complistRows = pbDataSet.Tables[on.Component].Select(filterString);
            foreach ( DataRow complistRow in complistRows ) {
                filteredComplist.Add( complistRow[on.Component_Descriptor] );
            }
            return filteredComplist;
        }


        private static string ProcessEmailRecipients( string inputString, bool addSender ) {

            string[] tempRecipients = CollectionUtil.StringToStringArray( inputString, new char[] {',', ';'} );
            StringCollection processedList = new StringCollection();
            foreach( string recipient in tempRecipients ) {
                if ( recipient==null || recipient.Trim() == "" ) {
                    continue;
                }
                else {
                    processedList.Add( recipient.Trim() );
                }
            }
            string sender = Environment.UserName;
            bool senderInList = false;
            foreach( string recipient in processedList ) {
                string tempRecipient = recipient;
                int atIndex = recipient.IndexOf('@');
                if ( atIndex != -1 ) {
                    tempRecipient = recipient.Substring( 0, atIndex );
                }
                if ( tempRecipient.ToUpper() == sender.ToUpper() ) {
                    senderInList = true;
                    break;
                }
            }
            if (addSender && !senderInList) {
                processedList.Add( sender.ToLower() );
            }
            return CollectionUtil.CollectionToString( processedList, ';' );
        }

        public static bool CreateProfile( string profileName ) {

            bool createdProfile = false;
            SqlConnection currentConnection = null;
            try {
                int rowsAffected;

                int parenIndex = profileName.IndexOf( '(' );
                string tempProfileName = profileName.Substring( 0, parenIndex-1 );
                string profileNamespace = profileName.Substring( parenIndex+1, profileName.Length-parenIndex-2 );

                string createProfileCmdText  = "insert into BUILD_PROFILE ";
                createProfileCmdText += "values ( @profile_namespace, @profile_name, @owner, @xml_text ) ";

                currentConnection = new SqlConnection( DatabaseUtil.sqlConnectionString2 );
                currentConnection.Open();
                SqlCommand createProfileCmd = currentConnection.CreateCommand();
                createProfileCmd.CommandText = createProfileCmdText;
                createProfileCmd.Parameters.Add("@profile_name", SqlDbType.VarChar).Value = tempProfileName;
                createProfileCmd.Parameters.Add("@profile_namespace", SqlDbType.VarChar).Value = profileNamespace;
                DatabaseInterface databaseInterface = new DatabaseInterface();
				createProfileCmd.Parameters.Add("@owner", SqlDbType.VarChar).Value = databaseInterface.FilterUserName(Environment.UserName);
                createProfileCmd.Parameters.Add("@xml_text", SqlDbType.Text).Value = new ProductBuildDataSet().GetXml();

                createdProfile = ( (rowsAffected=createProfileCmd.ExecuteNonQuery()) == 1 );

            }
            catch ( Exception ex ) {
                ex.GetType();
            }
            finally {
                if (currentConnection!=null) currentConnection.Close();
            }
            return createdProfile;

        }

        public static bool DeleteProfile( string profileName ) {
            bool deletedProfile = false;
            SqlConnection currentConnection = null;
            try {
                int rowsAffected;

                string deleteProfileCmdText  = "delete from BUILD_PROFILE ";
                deleteProfileCmdText += "where PROFILE_NAME + ' (' + PROFILE_NAMESPACE + ')' = @profileName ";
                deleteProfileCmdText += "and OWNER = @owner ";

                currentConnection = new SqlConnection( DatabaseUtil.sqlConnectionString2 );
                currentConnection.Open();
                SqlCommand deleteProfileCmd = currentConnection.CreateCommand();
                deleteProfileCmd.CommandText = deleteProfileCmdText;
                deleteProfileCmd.Parameters.Add("@profileName", SqlDbType.VarChar).Value = profileName;
                DatabaseInterface databaseInterface = new DatabaseInterface();
				deleteProfileCmd.Parameters.Add("@owner", SqlDbType.VarChar).Value = databaseInterface.FilterUserName(Environment.UserName);

                deletedProfile = ( (rowsAffected=deleteProfileCmd.ExecuteNonQuery()) == 1 );

            }
            catch ( Exception ex ) {
                ex.GetType();
            }
            finally {
                if (currentConnection!=null) currentConnection.Close();
            }
            return deletedProfile;
        }


        #endregion
       
    }
}
