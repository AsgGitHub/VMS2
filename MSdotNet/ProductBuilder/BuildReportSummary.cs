//**********************************************************************
//***** Copyright (c) 1992-2013 Allen Systems Group, Inc.  All rights reserved.
//**********************************************************************
//*****$Workfile:   BuildReportSummary.cs  $
//***** $Archive:   I:\Archives\CS\2\BuildReportSummary.c~  $
//*****$Revision:   2.2  $
//*****  $Author:   KOFORNAG  $
//*****    $Date:   08 Aug 2007 18:23:18  $
//***** $Modtime:   08 Aug 2007 18:23:24  $
//*****     $Log:   I:\Archives\CS\2\BuildReportSummary.c~  $
//*****
//*****   Rev 2.2   08 Aug 2007 18:23:18   KOFORNAG
//*****   Merge of Project EIMigrateToASGMail
//*****
//*****   Rev 2.1   12 Oct 2006 16:57:26   KOFORNAG
//*****   Merge of Project EISplitProductBuilder
//*****
//*****   Rev 2.0   11 Oct 2006 11:51:58   KOFORNAG
//*****   Create Branch Revision.
//*****
//*****   Rev 1.5   21 Dec 2004 17:41:52   DMASKOFF
//*****Merge of Project EIVMSBuildReportExtraData
//*****
//*****   Rev 1.4   03 Sep 2004 17:29:58   KOFORNAG
//*****Merge of Project EILbCommandLine
//*****
//*****   Rev 1.3   28 Jul 2004 17:05:58   KOFORNAG
//*****Merge of Project FIPbChangeSmptAddress
//*****
//*****   Rev 1.2   Aug 26 2003 18:42:40   RRUSSELL
//*****Merge of Project EIBuildReportingSystem2
//*****
//*****   Rev 1.1   Aug 12 2003 16:58:06   RRUSSELL
//*****Merge of Project EIBuildReportingSystem
//*****
//*****   Rev 1.0   Aug 12 2003 16:28:12   RRUSSELL
//*****Initialize Archive
//*****  $Endlog$
//**********************************************************************
//testing TMN 9/22/2014

using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Net.Mail;
using System.Web.UI;
using System.Reflection;
using System.Collections;


using LibraryBuilder;

namespace ProductBuilderTools {

    class BuildReportSummary {

        static string reportName = null;
		static ArrayList hosts = new ArrayList();
		static ArrayList recipients = new ArrayList();
		static bool reset = true;
		static string emailServer = "smtp.asg.com";

        [STAThread]
        static int Main(string[] args) {

            int errlvl = 0;
            for (int i = 0; i<args.Length; i++) {
				if (args[i].ToLower() == "-title") {
					reportName = args[++i];					
				}
				if (args[i].ToLower() == "-host") {
					hosts.Add(args[++i]);					
				}
				if (args[i].ToLower() == "-recipient") 
				{
					recipients.Add(args[++i]);					
				}
				if (args[i].ToLower() == "-emailserver") 
				{
					emailServer = args[++i];					
				}
				if (args[i].ToLower() == "-noreset") {
					reset = false;	
				}
            }
    
            SqlConnection dbConnection = null;
            try {                
                dbConnection = new SqlConnection( DatabaseUtil.sqlConnectionString );

                dbConnection.Open();

                SmtpClient client = new SmtpClient("smtp.asg.com");
                string From = Environment.UserName + "@" + Environment.UserDomainName + "com";
                string To = GetToRecipientsFromDB(dbConnection);

                MailMessage buildEmail = new MailMessage(new MailAddress(From), new MailAddress(To));
                buildEmail.Subject = "[BUILD] Build Report Summary";
                buildEmail.IsBodyHtml=true;
                buildEmail.Body = GetBody(dbConnection);

                client.Send( buildEmail );

            }
            catch (Exception e) {
                errlvl = 1;
                Console.Error.WriteLine("An error occurred in BuildReportSummary:");
                Console.Error.WriteLine(e.ToString());            
            }
            finally {
                if (dbConnection != null) dbConnection.Close();
            }

            return(errlvl);
        }


        private static string GetBody( SqlConnection dbConnection ) {

            DateTime lastRunDateTime = GetLastRunDateTime( dbConnection );

            DataTable buildDataTable = GetBuildDataTable( dbConnection, lastRunDateTime );

            DataTable mergedProjectDataTable = GetMergedProjectDataTable( dbConnection, lastRunDateTime );

            // column indexes in the "build status" table
            const int SUMMARYCOL_BUILDNAME         = 0;
            const int SUMMARYCOL_REPFILEPATH       = 1;
            const int SUMMARYCOL_BUILDSTATUS       = 2;
            const int SUMMARYCOL_BUILDSTARTTIME    = 3;
            const int SUMMARYCOL_LASTGOODSTARTTIME = 4;
            const int SUMMARYCOL_BUILDMACHINENAME  = 5;

            // column indexes in the "merged projects" table
            const int PRJLISTCOL_BUILDNAME    = 0;
            const int PRJLISTCOL_PROJECTNAME  = 1;
            const int PRJLISTCOL_PROJECTDESC  = 2;
            const int PRJLISTCOL_PROJECTOWNER = 3;


            StringBuilder bodyStringBuilder = new StringBuilder();

            HtmlTextWriter bodyWriter = new HtmlTextWriter( new StringWriter( bodyStringBuilder ) );

            bodyWriter.WriteFullBeginTag("html");
            bodyWriter.WriteLine();

            bodyWriter.WriteFullBeginTag("body");
            bodyWriter.WriteLine();

            bodyWriter.WriteFullBeginTag("h1");
            if (reportName == null) reportName = "Nightly Build Report";
            bodyWriter.Write( reportName + " for " + DateTime.Now.ToShortDateString());
            bodyWriter.WriteEndTag("h1");
            bodyWriter.WriteLine();

            bodyWriter.WriteLine();
            bodyWriter.WriteFullBeginTag("h2");
            bodyWriter.Write("Build Status Summary");
            bodyWriter.WriteEndTag("h2");
            bodyWriter.WriteLine();

            bodyWriter.WriteLine("Number of Builds: " + buildDataTable.Rows.Count );

            bodyWriter.WriteBeginTag("table");
            bodyWriter.AddStyleAttribute(HtmlTextWriterStyle.FontWeight, "bold");
            bodyWriter.AddStyleAttribute(HtmlTextWriterStyle.FontFamily, "verdana");
            bodyWriter.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "14pt");
            bodyWriter.WriteAttribute("border","1");
            bodyWriter.WriteAttribute("bordercolor","white");
            bodyWriter.WriteAttribute("cellpadding","2");
            bodyWriter.Write(HtmlTextWriter.TagRightChar);
            bodyWriter.WriteLine();

            bodyWriter.WriteFullBeginTag("thead"); bodyWriter.WriteLine();

            bodyWriter.WriteLine( "<TR><TH>Name</TH><TH>Status</TH><TH>Build Start Time</TH><TH>Last Good Build</TH><TH>Build Machine</TH></TR>" );

            bodyWriter.WriteEndTag("thead"); bodyWriter.WriteLine();

            bodyWriter.WriteFullBeginTag("tbody"); bodyWriter.WriteLine();
            bodyWriter.Indent++;
            bool greyRow = false;
            foreach( DataRow row in buildDataTable.Rows ) {
                greyRow = !greyRow;

                bodyWriter.WriteBeginTag("tr");
                string bgColor = ( greyRow ? "#E0E0E0" : "#F0F0F0" );
                bodyWriter.WriteAttribute("bgcolor", bgColor);

                bodyWriter.Write(HtmlTextWriter.TagRightChar);
                bodyWriter.WriteLine();

                bodyWriter.Indent++;

                bodyWriter.WriteBeginTag("td");
                bodyWriter.WriteAttribute("valign","top");
                bodyWriter.Write(HtmlTextWriter.TagRightChar);

                string currentBuildName = row[SUMMARYCOL_BUILDNAME].ToString();
                bodyWriter.WriteBeginTag("a");
                bodyWriter.WriteAttribute("href", row[SUMMARYCOL_REPFILEPATH].ToString() ); 
                bodyWriter.Write(HtmlTextWriter.TagRightChar);
                bodyWriter.Write( currentBuildName );
                bodyWriter.WriteEndTag("a");

                bodyWriter.WriteEndTag("td");
                bodyWriter.WriteLine();

                for (int j = 2; j < 6; j++) {

                    bodyWriter.WriteBeginTag("td");
                    bodyWriter.WriteAttribute("valign","top");
                    bodyWriter.Write(HtmlTextWriter.TagRightChar);

                    if ( j == SUMMARYCOL_BUILDSTATUS && (string)row[j] != "Succeeded" ) {
                        // unsuccessful build - see if the list of merged projects since last good build is non-empty, and if so, create a link to the list
                        bool mergedProjectsExist = false;
                        for (int k = 0; k < mergedProjectDataTable.Rows.Count; k++) {
                            if (mergedProjectDataTable.Rows[k][PRJLISTCOL_BUILDNAME].ToString() == currentBuildName) {
                                mergedProjectsExist = true;
                                break;
                            }
                        }

                        string statusText = "<FONT COLOR=\"Red\"><B>" + row[j] + "</B></FONT>";
                        if (mergedProjectsExist) statusText = "<a href=\"#" + GenerateAnchorName(currentBuildName) + "\">" + statusText + "</a>";

                        bodyWriter.Write( statusText );
                    }
                    else {
                        bodyWriter.Write( row[j] );
                    }

                    bodyWriter.WriteEndTag("td");
                    bodyWriter.WriteLine();
                }

                bodyWriter.Indent--;
                bodyWriter.WriteEndTag("tr");
                bodyWriter.WriteLine();
            }
            bodyWriter.Indent--;
            bodyWriter.WriteEndTag("tbody"); bodyWriter.WriteLine();

            // ...
            bodyWriter.WriteEndTag("table");
            bodyWriter.WriteLine();
       
            bodyWriter.WriteLine();
            bodyWriter.WriteFullBeginTag("h2");
            bodyWriter.Write("Projects Merged Since the Last Good Build");
            bodyWriter.WriteEndTag("h2");
            bodyWriter.WriteLine();

            bodyWriter.WriteFullBeginTag("dl"); bodyWriter.WriteLine();
            
            string lastProduct = "";
            foreach ( DataRow row in mergedProjectDataTable.Rows ) {
                string thisProduct = (string)row[PRJLISTCOL_BUILDNAME];
                if (thisProduct != lastProduct) {                    
                    bodyWriter.WriteLine("<a name=\"" + GenerateAnchorName(thisProduct) + "\">");
                    bodyWriter.WriteFullBeginTag("dt"); bodyWriter.WriteLine("<H3>" + thisProduct + "</H3>");
                    lastProduct = thisProduct;
                }
                bodyWriter.WriteFullBeginTag("dd"); bodyWriter.WriteLine( "<B>{0}</B> - {1} (Owner: {2})", row[PRJLISTCOL_PROJECTNAME], row[PRJLISTCOL_PROJECTDESC], row[PRJLISTCOL_PROJECTOWNER] );
            }
            bodyWriter.WriteEndTag("dl"); bodyWriter.WriteLine();

            bodyWriter.WriteEndTag("body");
            bodyWriter.WriteLine();

            bodyWriter.WriteEndTag("html");
            bodyWriter.WriteLine();

            UpdateLastRunDateTime( dbConnection );

            return bodyStringBuilder.ToString();
        }


        private static DataTable GetBuildDataTable( SqlConnection dbConnection, DateTime lastRunDateTime ) {
            
            DataTable buildDataTable = new DataTable();
            buildDataTable.Columns.Add();
            buildDataTable.Columns.Add();
            buildDataTable.Columns.Add();
            buildDataTable.Columns.Add();
            buildDataTable.Columns.Add();
            buildDataTable.Columns.Add();

            SqlDataReader buildDataTableReader = null;

            try {
                string buildDataTableCmdString = 
                  "select BUILD_NAME, BUILD_START_TIME, STATUS, LAST_GOOD_BUILD_START_TIME, BUILD_MACHINE_NAME " +
                  "from BUILD_STATUS b, BUILD_HOSTS h " +
                  "where ( (b.BUILD_START_TIME > @lastRunDateTime) or  (b.BUILD_COMPLETE_TIME > @lastRunDateTime) )" +
                  "and b.BUILD_MACHINE_NAME = h.MACHINE_NAME " + 
                  "and h.SHOW_ON_BUILD_REPORT = 1 ";
				StringBuilder buildDTCStringBuilder = new StringBuilder(buildDataTableCmdString);
				 
                // Restrict to build hosts specified on command line, if any
				for (int i=0; i<hosts.Count; i++) {
					if (i==0) 
						buildDTCStringBuilder.Append("and h.MACHINE_NAME in (");
					else
						buildDTCStringBuilder.Append(", ");
					buildDTCStringBuilder.Append("'"+hosts[i]+"'");
					if (i == hosts.Count - 1)
						buildDTCStringBuilder.Append(") ");
				}
				buildDTCStringBuilder.Append("order by BUILD_NAME ");

                // column indexes in the query above
                const int QCOL_BUILD_NAME           = 0;
                const int QCOL_BUILD_START_TIME     = 1;
                const int QCOL_BUILD_STATUS         = 2;
                const int QCOL_LAST_GOOD_START_TIME = 3;
                const int QCOL_BUILD_MACHINE_NAME   = 4;

                SqlCommand buildDataTableCmd = dbConnection.CreateCommand();
                buildDataTableCmd.CommandText = buildDTCStringBuilder.ToString();
                buildDataTableCmd.Parameters.Add("@lastRunDateTime", SqlDbType.DateTime).Value = lastRunDateTime;
                buildDataTableReader = buildDataTableCmd.ExecuteReader();
                while (buildDataTableReader.Read()) {
                    string buildName = buildDataTableReader.GetString(QCOL_BUILD_NAME);
                    DateTime buildStartTime = (!buildDataTableReader.IsDBNull(QCOL_BUILD_START_TIME)) ? buildDataTableReader.GetDateTime(QCOL_BUILD_START_TIME) : new DateTime(2000,1,1);
                    string status = buildDataTableReader.GetString(QCOL_BUILD_STATUS);
                    DateTime lastGoodBuildStartTime = (!buildDataTableReader.IsDBNull(QCOL_LAST_GOOD_START_TIME)) ? buildDataTableReader.GetDateTime(QCOL_LAST_GOOD_START_TIME) : new DateTime(2000,1,1);
                    string reportFile = "p:\\productbuilder\\buildreports\\" + buildName + "\\" + buildStartTime.ToString( "yyyyMMdd_HHmmss" ) + "\\BuildReport.htm";
                    string buildMachineName = buildDataTableReader.GetString(QCOL_BUILD_MACHINE_NAME);

                    buildDataTable.Rows.Add( new object[] {
                        buildName, 
                        reportFile, 
                        status, 
                        buildStartTime.ToString("MM/dd/yyy HH:mm:ss"), 
                        lastGoodBuildStartTime.ToString("MM/dd/yyy HH:mm:ss"),
						buildMachineName
                    } );
                }

            }
            finally {
                if (buildDataTableReader != null) buildDataTableReader.Close();
            }

            return buildDataTable;
        }


        private static DataTable GetMergedProjectDataTable( SqlConnection dbConnection, DateTime lastRunDateTime ) {
            DataTable mergedProjectDataTable = new DataTable();
            mergedProjectDataTable.Columns.Add();
            mergedProjectDataTable.Columns.Add();
            mergedProjectDataTable.Columns.Add();
            mergedProjectDataTable.Columns.Add();

            SqlDataReader mergedProjectDataTableReader = null;

            try {

                const string mergedProjectDataTableCmdText  = 
                  "select B.BUILD_NAME, P.PROJECT_NAME, P.DESCRIPTION, P.OWNED_BY, P.MERGE_DATE " +
                  "from BUILD_STATUS B, PROJECT P, PROJECT_PRODUCT PD, BUILD_HOSTS H " +
                  "where B.PRODUCT_NAME = PD.PRODUCT_NAME " +
                  "and B.PRODUCT_VERSION = PD.PRODUCT_VERSION " +
                  "and B.PLATFORM_CODE = PD.PLATFORM_CODE " +
                  "and P.PROJECT_NAME = PD.PROJECT_NAME " +
                  "and P.TYPE = 'Standard' " +
                  "and B.STATUS <> 'Succeeded' " +
                  "and B.BUILD_START_TIME > @lastRunDateTime " +
                  "and (P.MERGE_DATE > B.LAST_GOOD_BUILD_START_TIME or (B.LAST_GOOD_BUILD_START_TIME is NULL and P.MERGE_DATE is not NULL)) " +
                  "and B.BUILD_MACHINE_NAME = H.MACHINE_NAME " +
                  "and H.SHOW_ON_BUILD_REPORT = 1 " +
                  "order by B.BUILD_NAME, P.MERGE_DATE ";

                // column indexes in the query above
                const int QCOL_BUILD_NAME          = 0;
                const int QCOL_PROJECT_NAME        = 1;
                const int QCOL_PROJECT_DESCRIPTION = 2;
                const int QCOL_PROJECT_OWNER       = 3;

                SqlCommand mergedProjectDataTableCmd = dbConnection.CreateCommand();
                mergedProjectDataTableCmd.CommandText = mergedProjectDataTableCmdText;
                mergedProjectDataTableCmd.Parameters.Add("@lastRunDateTime", SqlDbType.DateTime).Value = lastRunDateTime;
                mergedProjectDataTableReader = mergedProjectDataTableCmd.ExecuteReader();

                while (mergedProjectDataTableReader.Read()) {
                    string buildName          = mergedProjectDataTableReader.GetString(QCOL_BUILD_NAME);
                    string projectName        = mergedProjectDataTableReader.GetString(QCOL_PROJECT_NAME);
                    string projectDescription = mergedProjectDataTableReader.GetString(QCOL_PROJECT_DESCRIPTION);
                    string projectOwner       = mergedProjectDataTableReader.GetString(QCOL_PROJECT_OWNER);
                    
                    mergedProjectDataTable.Rows.Add( new object[] {
                        buildName, 
                        projectName, 
                        projectDescription, 
                        projectOwner
                    } );
                }

            }
            finally {
                if (mergedProjectDataTableReader != null) mergedProjectDataTableReader.Close();
            }

            return mergedProjectDataTable;
        }


        private static string GetToRecipientsFromDB( SqlConnection dbConnection ) {

            string emailRecipients = "";

            SqlDataReader emailRecipientsReader = null;

            try {

                const string emailRecipientsCmdText = 
                  "select SMTP_ADDRESS " +
                  "from BUILD_REPORT_RECIPIENT ";

                SqlCommand emailRecipientsCmd = dbConnection.CreateCommand();
                emailRecipientsCmd.CommandText = emailRecipientsCmdText;
                emailRecipientsReader = emailRecipientsCmd.ExecuteReader();
                while (emailRecipientsReader.Read()) {
                    emailRecipients += emailRecipientsReader.GetString(0).Trim() + ";";
                }

            }
            finally {
                if (emailRecipientsReader!=null) emailRecipientsReader.Close();
            }

            return emailRecipients;

        }
		private static string GetToRecipients( ArrayList recipients ) 
		{

			string emailRecipients = "";

			for (int i=0; i<recipients.Count; i++) 
                    emailRecipients += recipients[i]+";";				

			return emailRecipients;
		}


        private static DateTime GetLastRunDateTime( SqlConnection dbConnection ) {
            DateTime lastRunDateTime = new DateTime(0);

            SqlDataReader datetimeReader = null;

            try {
                // make sure there's a row in the table
                string sql = 
                  "insert into BUILD_REPORT_LAST_RUN_TIME ( LAST_RUN_TIME ) " +
                  "select '1/1/2000' where not exists ( select * from BUILD_REPORT_LAST_RUN_TIME )";
                SqlCommand cmd = dbConnection.CreateCommand();
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();

                sql = "select LAST_RUN_TIME from BUILD_REPORT_LAST_RUN_TIME";
                cmd = dbConnection.CreateCommand();
                cmd.CommandText = sql;
                datetimeReader = cmd.ExecuteReader();
                datetimeReader.Read();
                lastRunDateTime = datetimeReader.GetDateTime(0);
                datetimeReader.Close();
                datetimeReader = null;
            }
            finally {
                if (datetimeReader != null) datetimeReader.Close();
            }

            return lastRunDateTime;
        }


        private static void UpdateLastRunDateTime( SqlConnection conn ) {
			// Command line can suppress this
			if (reset == false) return;
            // update last_run_time
            const string sql = "update BUILD_REPORT_LAST_RUN_TIME set LAST_RUN_TIME = getdate()";
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }

        private static string GenerateAnchorName(string buildName) {
            return buildName.Replace(' ', '_').Replace('\t', '_').Replace('.', '_');
        }


    }
}

