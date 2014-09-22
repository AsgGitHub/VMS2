//**********************************************************************
//***** Copyright (c) 1992-2013 Allen Systems Group, Inc.  All rights reserved.
//**********************************************************************
//*****$Workfile:   SmtpSendEmail.cs  $
//***** $Archive:   I:\Archives\CS\2\SmtpSendEmail.c~  $
//*****$Revision:   2.3  $
//*****  $Author:   KOFORNAG  $
//*****    $Date:   09 Aug 2007 12:49:32  $
//***** $Modtime:   09 Aug 2007 12:49:38  $
//*****     $Log:   I:\Archives\CS\2\SmtpSendEmail.c~  $
//*****
//*****   Rev 2.3   09 Aug 2007 12:49:32   KOFORNAG
//*****   Merge of Project EIMigrateToAsgMail0
//*****
//*****   Rev 2.2   08 Aug 2007 18:23:20   KOFORNAG
//*****   Merge of Project EIMigrateToASGMail
//*****
//*****   Rev 2.1   12 Oct 2006 16:57:26   KOFORNAG
//*****   Merge of Project EISplitProductBuilder
//*****
//*****   Rev 2.0   11 Oct 2006 11:52:06   KOFORNAG
//*****   Create Branch Revision.
//*****
//*****   Rev 1.1   28 Oct 2004 11:11:30   DMASKOFF
//*****Merge of Project EIVMSPBSmtpSendEmail
//*****
//*****   Rev 1.0   21 Oct 2004 17:08:30   DMASKOFF
//*****Initialize Archive
//*****  $Endlog$
//**********************************************************************

using System;
using System.IO;
using System.Text;
using System.Net.Mail;
using System.Reflection;


namespace ProductBuilderTools {

    class SmtpSendEmail {

        static void ShowUsage(string msg) {
            
            if (msg.Length > 0) {
                Console.WriteLine(msg);
                Console.WriteLine("");
            }

            Console.WriteLine("Usage: SmtpSendMail.exe <params>");
            Console.WriteLine("Params are case-insensitive, have a full and and short form, can be specified as /param or -param");
            Console.WriteLine("Required params:");
            Console.WriteLine("    -r or -recipients <recipient-list>");
            Console.WriteLine("Optional params:");
            Console.WriteLine("    -s or -subject    <subject>"); 
            Console.WriteLine("    -b or -body       <body-file-name>"); 
            Console.WriteLine("    -a or -attach     <attach-file-list>");
            Console.WriteLine("    -smtp             <smtp-server>");
            Console.WriteLine("    -format           text|html");
            Console.WriteLine("Note: -attach param is recognized as an argument but ignored" );
            Console.WriteLine("");
            Console.WriteLine("Example:");
            Console.WriteLine("SmtpSendEmail /S \"The subject line.\" /B \"Q:\\BodyTextFile.Txt\" /A \"Q:\\Attachment1.Txt;Q:\\Attachment2.Txt\" /R \"dhertz;ldelia\" /SMTP smtp.mobius.com");
            Console.WriteLine("");
        }

        [STAThread]
        static int Main(string[] args) {

            try {
                string subject = "";
                string bodyFileName = "";
                string recipientIds = "";
                string smtpServer = "imail.mobius.com";
                string mailFormat = "text";

                int i = 0;
                while (true) {
                    if (i >= args.Length) break;

                    string argName = args[i].ToLower();
                    if ( argName.Length > 0 && (argName[0] == '/' || argName[0] == '-') ) {
                        argName = argName.Substring(1);
                    }
                    else {
                        ShowUsage("Unrecognized argument: " + args[i]);
                        return 1;
                    }
                    
                    if (argName == "h" || argName == "?") {
                        ShowUsage("");
                        return 0;
                    }

                    i++;
                    if (i >= args.Length) break;

                    string argVal = args[i];
                    i++;

                    if (argName == "s" || argName == "subject")
                        subject = argVal;

                    else if (argName == "b" || argName == "body")
                        bodyFileName = argVal;

                    else if (argName == "a" || argName == "attach") { } // attachment list is not supported

                    else if (argName == "r" || argName == "recipients")
                        recipientIds = argVal;

                    else if (argName == "smtp")
                        smtpServer = argVal;

                    else if (argName == "format")
                        mailFormat = argVal;

                    else {
                        ShowUsage("Unrecognized argument: " + args[i]);
                        return 1;
                    }
                }

                if (recipientIds.Length <= 0) {
                    ShowUsage("No recipient list specified - program terminated");
                    return 2;
                }

                LibraryBuilder.DatabaseInterface dbInterface = new LibraryBuilder.DatabaseInterface();

                // Construct recipient list
                char[] delims = { ',', ';' };
                string[] recipients = recipientIds.Split(delims);
                string recipAddrs = "";
                string addrDelim = "";
                string currentEmailAddress;
                foreach (string recipientId in recipients) {
                    string recipId = recipientId.Trim();
                    if (recipId.Length <= 0) continue;
                    if (recipId.IndexOf("@") < 0)
                       currentEmailAddress = dbInterface.GetUserEmail(recipId).Trim();
                    else
                       currentEmailAddress = recipId;
                    if (currentEmailAddress.Length <= 0) continue;
                    string domain = ( currentEmailAddress.IndexOf("@") < 0 ? "@asg.com" : "" );
                    recipAddrs = recipAddrs + addrDelim + currentEmailAddress + domain;
                    addrDelim = ";";
                }

                // Construct body
                string body = "";
                if (bodyFileName.Length > 0) {
                    System.IO.StreamReader bodyFile = new System.IO.StreamReader(bodyFileName);
                    body = bodyFile.ReadToEnd();
                }

                SmtpClient client = new SmtpClient(smtpServer);
                string From = dbInterface.GetUserEmail(Environment.UserName).Trim() + "@asg.com";
                string To = recipAddrs;
                MailMessage msg = new MailMessage(new MailAddress(From), new MailAddress(To));                
                msg.Body = body;

                // support for attachments
                // string fileName = @"C:\temp\Hello.txt";
                // MailAttachment attachment = new MailAttachment(fileName, MailEncoding.Base64);
                // msg.Attachments.Add(attachment);

                Console.WriteLine("Sending email:");
                Console.WriteLine("Subject: " + msg.Subject);
                Console.WriteLine("To: " + msg.To);

                client.Send( msg );

                Console.WriteLine("Successfully sent email.");

                return 0;

            }
            catch (Exception e) {
                Console.Error.WriteLine("ERROR: " + e.ToString());
                return 3;            
            }
        }

    }
}
