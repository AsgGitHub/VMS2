//**********************************************************************
//***** Copyright (c) 1992-2013 Allen Systems Group, Inc.  All rights reserved.
//**********************************************************************
//*****$Workfile:   JCDParserResults.cs  $
//***** $Archive:   I:\Archives\CS\2\JCDParserResults.c~  $
//*****$Revision:   2.1  $
//*****  $Author:   KOFORNAG  $
//*****    $Date:   12 Oct 2006 16:57:22  $
//***** $Modtime:   12 Oct 2006 16:57:22  $
//*****     $Log:   I:\Archives\CS\2\JCDParserResults.c~  $
//*****
//*****   Rev 2.1   12 Oct 2006 16:57:22   KOFORNAG
//*****   Merge of Project EISplitProductBuilder
//*****
//*****   Rev 2.0   11 Oct 2006 11:52:00   KOFORNAG
//*****   Create Branch Revision.
//*****
//*****   Rev 1.5   28 Feb 2006 12:15:06   CSOKOL
//*****   Merge of Project EWProductBuilderWSDL2Java
//*****
//*****   Rev 1.4   Nov 14 2003 16:20:58   RRUSSELL
//*****Merge of Project EWCustomManifestFiles
//*****
//*****   Rev 1.3   Aug 27 2003 10:02:04   RRUSSELL
//*****Merge of Project EISupportConsistentJDKVersion
//*****
//*****   Rev 1.2   27 Jan 2003 15:09:14   RRUSSELL
//*****Merge of Project EIPRDBLD-Misc22
//*****
//*****   Rev 1.1   23 Dec 2002 16:17:46   RRUSSELL
//*****Merge of Project EIPRDBLDFixJava2
//*****
//*****   Rev 1.0   18 Nov 2002 17:17:34   RRUSSELL
//*****Initialize Archive
//*****  $Endlog$
//**********************************************************************

using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml;

namespace ProductBuilderTools {


    public class JCDParserResults {


        private class Complist {

            public class ComplistKey {
                
                public string tool;
                public string java_home;
                public string classpath;

                public ComplistKey( string tool, string java_home, string classpath ) {
                    this.tool = tool.ToLower();
                    this.java_home = java_home.ToLower();
                    this.classpath = classpath.ToLower();
                }

                public override bool Equals( Object complistKey ) {
                    ComplistKey ck = (ComplistKey)complistKey;
                    return ( this.tool==ck.tool && 
                        this.java_home==ck.java_home &&
                        this.classpath==ck.classpath );
                }

                public override int GetHashCode() {
                    string keyString = tool + "|" + java_home + "|" + classpath;
                    return keyString.GetHashCode();
                }

            }

            public ComplistKey complistKey;
            public ArrayList files;
       
            public Complist( ComplistKey complistKey ) {
                this.complistKey = complistKey;
                this.files = new ArrayList();
            }

        }

        private class JCDElementNames {
        
            public string jcd = "javaComponentDescriptor";
            public string target = "target";
            public string jdk = "jdk";
            public string java_home = "java_home";
            public string classpath = "classpath";
            public string path = "path";
            public string nestedPackages = "nestedPackages";
            public string package = "package";
            public string complist = "complist";
            public string javac = "javac";
            public string file = "file";
            public string rmic = "rmic";
            public string jarIncludes = "jarIncludes";
            public string productFiles = "productFiles";
            public string manifest = "manifest";
            public string library = "library";
            public string packageSteps = "packageSteps";
            public string packageStep = "packageStep";
            public string services = "services";
            public string service = "service";

        }

        private class CopyEntry {
            public string from;
            public string to;
        }

        private class ServiceEntry {
            public string service;
            public string version;
            public string java_home;
            public string classpath;
        }

        private JCDElementNames en;
        private string target;
        private ArrayList complists;
        private ArrayList copyList;
        private ArrayList serviceList;
        private ArrayList packageStepList;
        private string component_java_home;
        private string manifest;


        public JCDParserResults() {

            en = new JCDElementNames();
            complists = new ArrayList();
            copyList = new ArrayList();
            serviceList = new ArrayList();
            packageStepList = new ArrayList();
            target = null;
            component_java_home = null;
            manifest = null;

        }



        public void ParseJCDFile( string jcdFile, string classpath, string java_home ) {

            // load file
            XmlDocument doc = new XmlDocument();
            doc.Load( new XmlTextReader( jcdFile ) );            
            
            // target
            XmlNode targetNode = doc[en.jcd][en.target];
            if ( target==null && targetNode!=null ) {
                // only set once in top-level package
                target = ((XmlElement)targetNode).GetAttribute("name");
            }

            // java_home
            XmlNode jdkNode = doc[en.jcd][en.jdk];
            if ( jdkNode!=null ) {
                java_home = JCDParserResults.JDKToJAVA_HOME(((XmlElement)jdkNode).GetAttribute("version"));
            }
            XmlNode java_homeNode = doc[en.jcd][en.java_home];
            if ( java_homeNode!=null ) {
                java_home = ((XmlElement)java_homeNode).GetAttribute("path");
            }
            java_home = java_home.ToLower();
            // only set once in top-level package
            if ( component_java_home == null ) {
                component_java_home = java_home;
            }

            // classpath
            XmlNode classpathParentNode = doc[en.jcd][en.classpath];
            if ( classpathParentNode!=null ) {
                classpath = "";
                XmlNodeList classpathNodes = classpathParentNode.ChildNodes;
                foreach (XmlNode classpathNode in classpathNodes) {
                    if ( classpathNode.LocalName == en.path ) {
                        classpath += ((XmlElement)classpathNode).GetAttribute("value") + ";";
                    }
                }
                classpath = classpath.TrimEnd( new char[] {';'} );
                classpath = Environment.ExpandEnvironmentVariables( classpath );
            }
            classpath = classpath.ToLower();

            // services
            XmlNode servicesParentNode = doc[en.jcd][en.services];
            if (servicesParentNode != null) {
                foreach (XmlNode serviceNode in servicesParentNode.ChildNodes) {
                    if (serviceNode.LocalName == en.service) {
                        ServiceEntry e = new ServiceEntry();
                        XmlNode taskJdkNode = serviceNode[en.jdk];
                        XmlNode taskJava_HomeNode = serviceNode[en.java_home];
                        XmlNode taskClasspathParentNode = serviceNode[en.classpath];
                       
							   if (taskJava_HomeNode != null)
								    e.java_home = ((XmlElement)taskJava_HomeNode).GetAttribute("path");
							   else if (taskJdkNode != null)
                            e.java_home = JCDParserResults.JDKToJAVA_HOME(((XmlElement)taskJdkNode).GetAttribute("version"));
                        else
                            e.java_home = java_home;
                    
                        e.classpath = classpath;
                        if (taskClasspathParentNode != null) {
                            XmlNodeList classpathNodes = taskClasspathParentNode.ChildNodes;
                           
                            e.classpath = "";
                            foreach (XmlNode classpathNode in classpathNodes) {
                                if ( classpathNode.LocalName == en.path )
                                    e.classpath += ((XmlElement)classpathNode).GetAttribute("value") + ";";
                            }
                            e.classpath = e.classpath.TrimEnd( new char[] {';'} );
                            e.classpath = Environment.ExpandEnvironmentVariables(e.classpath);
                        }

                        e.service = ((XmlElement)serviceNode).GetAttribute("name");
                        e.version = ((XmlElement)serviceNode).GetAttribute("version");

                        serviceList.Add(e);
                    }
                }
            }

            // compilation tasks
            XmlNode complistParentNode = doc[en.jcd][en.complist];
            if ( complistParentNode!=null ) {
                XmlNodeList complistTaskNodes = complistParentNode.ChildNodes;
                foreach (XmlNode complistTaskNode in complistTaskNodes) {
                    if ( complistTaskNode.NodeType == XmlNodeType.Comment )
                        continue;
                    string taskTool = complistTaskNode.LocalName;

                    XmlNode taskJdkNode = complistTaskNode[en.jdk];
                    string taskJava_Home = (taskJdkNode!=null) ? 
                        JCDParserResults.JDKToJAVA_HOME(((XmlElement)taskJdkNode).GetAttribute("version")) : 
                        java_home;
                    XmlNode taskJava_HomeNode = complistTaskNode[en.java_home];
                    taskJava_Home = (taskJava_HomeNode!=null) ? 
                        ((XmlElement)taskJava_HomeNode).GetAttribute("path") : 
                        taskJava_Home;
                    
                    XmlNode taskClasspathParentNode = complistTaskNode[en.classpath];
                    string taskClasspath = classpath;
                    if ( taskClasspathParentNode!=null ) {
                        taskClasspath = "";
                        XmlNodeList classpathNodes = taskClasspathParentNode.ChildNodes;
                        foreach (XmlNode classpathNode in classpathNodes) {
                            if ( classpathNode.LocalName == en.path ) {
                                taskClasspath += ((XmlElement)classpathNode).GetAttribute("value") + ";";
                            }
                        }
                        taskClasspath = taskClasspath.TrimEnd( new char[] {';'} );
                        taskClasspath = Environment.ExpandEnvironmentVariables( taskClasspath );
                    }
                    Complist.ComplistKey ck = new Complist.ComplistKey( taskTool, taskJava_Home, taskClasspath );
                    Complist complist = null;
                    foreach (Complist cl in complists) {
                        if ( cl.complistKey.Equals(ck) ) {
                            complist = cl;
                        }
                    }
                    if ( complist==null ) {
                        complist = new Complist(ck);
                        complists.Add( complist );
                    }
                    XmlNodeList complistFileNodes = complistTaskNode.ChildNodes;
                    foreach (XmlNode complistFileNode in complistFileNodes) {
                        if ( complistFileNode.LocalName!=en.file )
                            continue;
                        string filename = DepackageFilename( ((XmlElement)complistFileNode).GetAttribute("name") );
                        complist.files.Add( filename );
                    }
                }
            }

            // nested packages
            XmlNode nestedPackageParentNode = doc[en.jcd][en.nestedPackages];
            if ( nestedPackageParentNode!=null ) {
                XmlNodeList nestedPackageNodes = nestedPackageParentNode.ChildNodes;
                foreach (XmlNode nestedPackageNode in nestedPackageNodes) {
                    if ( nestedPackageNode.LocalName != en.package )
                        continue;
                    string packageDescriptorName = ((XmlElement)nestedPackageNode).GetAttribute("descriptor");
                    //check if the package descriptor is in the same directory as the parent
                    string packageDirectory = Path.Combine( Path.GetPathRoot( jcdFile ), Path.GetDirectoryName( jcdFile ) );
                    string packageFileName = Path.Combine( packageDirectory, packageDescriptorName );
                    //if not, check in sibling pkg directory
                    if ( !File.Exists( packageFileName ) ) {
                        int lastDirectoryPos = packageDirectory.LastIndexOfAny( new char[] {'\\', '/'} );
                        if ( lastDirectoryPos > -1 ) {
                            string packageDirectoryParent = packageDirectory.Substring( 0, lastDirectoryPos+1 );
                            packageDirectory = Path.Combine( packageDirectoryParent, "pkg" );
                            packageFileName = Path.Combine( packageDirectory, packageDescriptorName );
                        }
                    }
                    if ( !File.Exists( packageFileName ) ) {
                        throw new Exception("Package descriptor, " + packageDescriptorName + ", not found");
                    }
                    this.ParseJCDFile( packageFileName, classpath, java_home );
                }
            }

            XmlNode jarIncludesNode = doc[en.jcd][en.jarIncludes];
            if ( jarIncludesNode != null ) {
                // product files
                XmlNode productFilesParentNode = jarIncludesNode[en.productFiles];
                if ( productFilesParentNode!=null ) {
                    XmlNodeList productFileNodes = productFilesParentNode.ChildNodes;
                    foreach (XmlNode productFileNode in productFileNodes) {
                        if ( productFileNode.LocalName==en.file ) {
                            CopyEntry ce = new CopyEntry();
                            string filename = DepackageFilename(((XmlElement)productFileNode).GetAttribute("name"));
                            ce.from = "%BuildPath%\\" + Path.GetExtension( filename ).TrimStart(new char[] {'.'}) + Path.DirectorySeparatorChar + filename;
                            XmlNode packageNode = productFileNode[en.package];
                            if ( packageNode != null ) {
                                string packagePath = ((XmlElement)packageNode).GetAttribute("name");
                                ce.to = packagePath.Replace( '.', '\\' );
                            }
                            copyList.Add(ce);
                        }
                        else if ( productFileNode.LocalName==en.manifest ) {
                            XmlElement manifestFileNode = (XmlElement)productFileNode[en.file];
                            string filename = DepackageFilename(manifestFileNode.GetAttribute("name"));
                            manifest = "%BuildPath%\\" + Path.GetExtension( filename ).TrimStart(new char[] {'.'}) + Path.DirectorySeparatorChar + filename;
                        }
                    }
                }

                // library files
                XmlNodeList libraryNodes = jarIncludesNode.ChildNodes;    
                foreach (XmlNode libraryNode in libraryNodes) {   
                    if ( libraryNode.LocalName != en.library )
                        continue;
                    string libraryPath = libraryNode[en.path].GetAttribute("value");
                    XmlNodeList fileNodes = libraryNode.ChildNodes;
                    foreach (XmlNode fileNode in fileNodes) {
                        if ( fileNode.LocalName != en.file )
                            continue;
                        CopyEntry ce = new CopyEntry();
                        string filename = ((XmlElement)fileNode).GetAttribute("name");
                        ce.from = Path.Combine( libraryPath, filename );
                        ce.to = Path.GetDirectoryName( filename );
                        copyList.Add(ce);
                    }
                }
            }

            // packageSteps
            XmlNode packageStepsParentNode = doc[en.jcd][en.packageSteps];
            if ( packageStepsParentNode!=null ) {
                XmlNodeList packageSteps = packageStepsParentNode.ChildNodes;
                foreach (XmlNode packageStep in packageSteps) {
                    string packageStepName = ((XmlElement)packageStep).GetAttribute("name");
                    packageStepList.Add( packageStepName );
                }
            }
        }


        public void RemoveDuplicateSourceFiles() {
            foreach ( Complist complist in complists ) {
                int i, j;
                for ( i=0; i<complist.files.Count; i++ ) {
                    while( i != (j=complist.files.LastIndexOf(complist.files[i])) ) {
                        complist.files.RemoveAt(j);    
                    }    
                }
            }
        }


        public void WriteResults( string componentID, string outputPath ) {

            int complistFileCounter = 0;
            StreamWriter fileWriter = null;
            string filename;
          
            // write target
            filename = Path.Combine( outputPath, componentID + "_target.txt" );
            try {
                fileWriter = new StreamWriter( filename, false );
                fileWriter.WriteLine( target );
            }
            finally {
                if (fileWriter!=null) fileWriter.Close();
            }

            // write build commands
            filename = Path.Combine( outputPath, componentID + "_buildcmd.txt" );
            try {
                fileWriter = new StreamWriter( filename, false );

                foreach ( ServiceEntry service in serviceList ) {
                    string filename2 = Path.Combine( outputPath, String.Format(componentID + "_service{0:D3}.txt", complistFileCounter ));
                    StreamWriter fileWriter2 = null;

                    fileWriter.WriteLine( "\"service\" \"" + filename2 + "\" \"" + 
							                     service.java_home + "\" \"" + service.classpath + "\"");

                    try {
                        fileWriter2 = new StreamWriter( filename2, false );

                        fileWriter2.WriteLine("\"" + MakeWSDLFilename(service) + "\" " +
                                              "\"" + MakeServicePackage(service) + "\" " +
                                              "\"" + MakeServiceDirectory(service) + "\"");
                    }
                    finally {
                        if (fileWriter2 != null) 
                            fileWriter2.Close();
                    }

                    complistFileCounter++;
                }

                foreach ( Complist complist in complists ) {
                    string filename2 = Path.Combine( outputPath, String.Format(componentID + "_complist{0:D3}.txt", complistFileCounter ));
                    fileWriter.WriteLine( "\"" + complist.complistKey.tool + "\" \"" +
                                           filename2 + "\" \"" +
                                           complist.complistKey.java_home + "\" \"" +
                                           complist.complistKey.classpath + "\"" );
                    StreamWriter fileWriter2 = null;
                    try {
                        fileWriter2 = new StreamWriter( filename2, false );
                        foreach ( String file in complist.files ) {
                            fileWriter2.WriteLine( file );
                        }
                    }
                    finally {
                        if (fileWriter2!=null) fileWriter2.Close();
                    }
                    complistFileCounter++;
                }
            }
            finally {
                if (fileWriter!=null) fileWriter.Close();
            }

            // write copy to known package list
            filename = Path.Combine( outputPath, componentID + "_copy2package.txt" );
            try {
                fileWriter = new StreamWriter( filename, false );
                foreach ( CopyEntry ce in copyList ) {
                    if ( ce.to != null )
                    fileWriter.WriteLine( "\"" + ce.from + "\" \"" + ce.to + "\"" );
                }
            }
            finally {
                if (fileWriter!=null) fileWriter.Close();
            }

            // write copy to unknown package list
            filename = Path.Combine( outputPath, componentID + "_copy.txt" );
            try {
                fileWriter = new StreamWriter( filename, false );
                foreach ( CopyEntry ce in copyList ) {
                    if ( ce.to == null )
                        fileWriter.WriteLine( "\"" + ce.from + "\"" );
                }
            }
            finally {
                if (fileWriter!=null) fileWriter.Close();
            }

            // write package commands
            filename = Path.Combine( outputPath, componentID + "_package.txt" );
            fileWriter = null;
            try {
                if ( packageStepList.Count > 0 ) {
                    fileWriter = new StreamWriter( filename, false );
                    foreach ( string ps in packageStepList ) {
                        fileWriter.WriteLine( ps );
                    }
                }
            }
            finally {
                if (fileWriter!=null) fileWriter.Close();
            }

            // write component JAVA_HOME
            filename = Path.Combine( outputPath, componentID + "_JAVA_HOME.txt" );
            fileWriter = null;
            try {
                fileWriter = new StreamWriter( filename, false );
                fileWriter.WriteLine( " \"" + component_java_home + "\"" );
            }
            finally {
                if (fileWriter!=null) fileWriter.Close();
            }

            // write manifest file
            if ( manifest!=null && manifest!="" ) {
                filename = Path.Combine( outputPath, componentID + "_manifest.txt" );
                fileWriter = null;
                try {
                    fileWriter = new StreamWriter( filename, false );
                    fileWriter.WriteLine( " \"" + manifest + "\"" );
                }
                finally {
                    if (fileWriter!=null) fileWriter.Close();
                }
            }

        }


        public string DepackageFilename( string filename ) {
            if ( filename.IndexOf( '.' ) == filename.LastIndexOf( '.' ) )
                return filename;

            StringBuilder depackagedName = new StringBuilder( filename );
            depackagedName = depackagedName.Replace( '.', '\\' );
            depackagedName[filename.LastIndexOf( '.' )] = '.';
            return depackagedName.ToString();
        }


        // for backwards compatibility with jdk element
        public static string JDKToJAVA_HOME ( string jdk ) {
            string java_home = "P:\\VENDORS\\Sun\\JDK" + jdk.Replace(".","");
            return java_home;
        }

        private string MakeWSDLFilename( ServiceEntry service ) {
            return service.service.Substring(0, 1).ToUpper() + 
                   service.service.Substring(1) + "Service" +
                   service.version.Replace('/', '_') +
                   ".wsdl";
        }

        private string MakeServicePackage( ServiceEntry service ) {
            return "com.mobius." + service.service + ".v" + service.version.Replace('/', '_');
        }

        private string MakeServiceDirectory( ServiceEntry service ) {
            return "com\\mobius\\" + service.service + "\\v" + service.version.Replace('/', '_');
        }
    }
}
