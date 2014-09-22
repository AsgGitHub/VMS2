//**********************************************************************
//***** Copyright (c) 1992-2013 Allen Systems Group, Inc.  All rights reserved.
//**********************************************************************
//*****$Workfile:   LogMessage.cs  $
//***** $Archive:   I:\Archives\CS\2\LogMessage.c~  $
//*****$Revision:   2.1  $
//*****  $Author:   KOFORNAG  $
//*****    $Date:   12 Oct 2006 16:57:24  $
//***** $Modtime:   12 Oct 2006 16:57:22  $
//*****     $Log:   I:\Archives\CS\2\LogMessage.c~  $
//*****
//*****   Rev 2.1   12 Oct 2006 16:57:24   KOFORNAG
//*****   Merge of Project EISplitProductBuilder
//*****
//*****   Rev 2.0   11 Oct 2006 11:52:02   KOFORNAG
//*****   Create Branch Revision.
//*****
//*****   Rev 1.2   07 Jul 2005 18:00:24   KOFORNAG
//*****   Merge of Project FILbPreventDuplicateQueue
//*****
//*****   Rev 1.1   03 Sep 2004 17:30:04   KOFORNAG
//*****Merge of Project EILbCommandLine
//*****
//*****   Rev 1.0   17 Aug 2004 13:08:56   KOFORNAG
//*****Initialize Archive
//*****  $Endlog$
//**********************************************************************
using System;
using System.Messaging;
using LibraryBuilder;
using System.Reflection;

namespace ProductBuilder {
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class LogMsg {
		[STAThread]
		static int Main(string[] args) {
			bool useMsmq = args.Length > 0;
			NewMessage userMsg = null;
			if (useMsmq) {
                string queueName = LBEnvironment.LBMessageQueuePath + "_" + args[0];
                NewMessage.CreateQueue(queueName);
				userMsg = new NewMessage(args[0]);
			}
			string currLine;
			while ((currLine = Console.ReadLine()) != null) {
				Console.WriteLine(currLine);
				if (useMsmq) 
					userMsg.SendConsoleCopyMessage(currLine + "\r\n", 0, 0);	
			}
			return 0;
		}
	}
}
