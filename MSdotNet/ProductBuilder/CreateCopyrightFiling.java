//**********************************************************************
//***** Copyright (c) 1992-2013 Allen Systems Group, Inc.  All rights reserved.
//**********************************************************************
//*****$Workfile:   CreateCopyrightFiling.java  $
//***** $Archive:   I:\Archives\java\1\CreateCopyrightFiling.j~va  $
//*****$Revision:   1.2  $
//*****  $Author:   shermank  $
//*****    $Date:   24 Mar 2010 11:09:04  $
//***** $Modtime:   24 Mar 2010 11:09:06  $
//*****     $Log:   I:\Archives\java\1\CreateCopyrightFiling.j~va  $
//*****
//*****   Rev 1.2   24 Mar 2010 11:09:04   shermank
//*****   Merge of Project EIVMSCopyrightFilingFix
//*****
//*****   Rev 1.1   25 Feb 2010 09:37:06   shermank
//*****   Merge of Project EIVMSGenerateCopyrightNotice
//*****
//*****   Rev 1.0   27 May 2009 12:42:18   shermank
//*****   Initialize Archive
//*****  $Endlog$
//**********************************************************************
import java.io.*;
import java.util.*;

/**
 *
 * @author shermank
 */
public class CreateCopyrightFiling {


    private static int minCopyrightFilingLineCount = 3250;

	String[] parms;
   	private static String productName = "";
	private static String fileListPath = "";
	private static String copyrightFilingPath = "";
	private static String logFilePath = "";
	private static boolean VMSManaged = false;
	private static int copyrightFilingLineTotal = 0;
	private static int strippedLineTotal = 0;

    private static BufferedReader fileListReader;
    private static BufferedWriter copyrightFilingWriter;
    private static BufferedWriter logFileWriter;

    public static void main(String[] args) {
        CreateCopyrightFiling app = new CreateCopyrightFiling(args);
		System.exit(app.run());
    }

    public CreateCopyrightFiling(String[] args) {
		parms = args;
    }

    public int run() {
		int returnCode = 0;

		// Process Command-line Arguments
		if (parms.length < 4) {
			Usage();
            return(-2);
		}

   		productName = parms[0];
		fileListPath = parms[1];
		copyrightFilingPath = parms[2];
		logFilePath = parms[3];

        // Open log file
        try {
            logFileWriter = new BufferedWriter(new FileWriter(logFilePath));
		}
		catch ( Exception e ) {
			System.err.println("Could not create log file " + logFilePath + ".");
			System.err.println(e.toString());
    		e.printStackTrace();
            return(-1);
		}

        // Fifth parameter must be 'VMS'  
		if (parms.length >= 5) {
            if (parms[4].equalsIgnoreCase("VMS")) {
                VMSManaged = true;
   			}
            else
   			    writeToLog("Fifth parameter is unrecognized: " + parms[4] + ".  Will be ignored.");

            if (parms.length > 5)
		        writeToLog("Extra parameters after " + parms[4] + " will be ignored.");
		}

        // Echo parameters
        writeToLog("Creating Copyright Filing for: "+productName);
        if (VMSManaged) {
            writeToLog(productName + " is a VMS managed product.");
            setUpCommentInfo();  // setup to strip VMS headers
        }
        writeToLog("Source file list: "+fileListPath);
        writeToLog("Copyright Filing will be written to: "+copyrightFilingPath);
        writeToLog("Log will be written to: "+logFilePath);

        // Process source file list
		int sourceFilesDropped = 0;
		try {
            fileListReader = new BufferedReader(new FileReader(fileListPath));
            File fileListFile = new File(fileListPath);
			if ( false == fileListFile.exists() )
				throw new Exception("Input file, " + fileListPath + ", not found.");

            copyrightFilingWriter = new BufferedWriter(new FileWriter(copyrightFilingPath));
            File copyrightFilingFile = new File(copyrightFilingPath);

            // Write standard Copyright Filing header
            initCopyrightFiling();

            // Process one source file
			String sourceFile;
			while ( (sourceFile=fileListReader.readLine()) != null )
			{
				sourceFile = sourceFile.trim();
				if ( sourceFile.length()==0 )
					continue;

                File sourceFileFile = new File(sourceFile);
				if ( false == sourceFileFile.exists() ) {
                    writeToLog("Source file " + sourceFile + " not found.  Will be ignored.");
                    sourceFilesDropped++;
					continue;
				}

    			writeToLog("Processing source file: "+sourceFile);
                BufferedReader sourceFileReader = new BufferedReader(new FileReader(sourceFile));

                FileTypeCharacteristics f = null;
                if (VMSManaged) {
                    String targetExtension = sourceFile.substring(1+sourceFile.lastIndexOf("."));
                    targetExtension = targetExtension.toLowerCase();
                    f = (FileTypeCharacteristics) commentInfo.get(targetExtension);
   				    if ( f == null ) {
                        writeToLog("VMS comment info not found for extension " + sourceFile + ". Will be ignored.");
                        sourceFilesDropped++;
	    				continue;
		    		}
                }

                String sourceLine;
    			int sourceLinesStripped = 0;
                int sourceLinesWritten = 0;
                boolean VMSHeaderStripped = false;

				while ( (sourceLine=sourceFileReader.readLine()) != null ) {

                    // Strip VMS headers
					boolean stripit = false;
                    if (VMSManaged && VMSHeaderStripped==false) {
                        if (f.isVMSHeaderLine(sourceLine)) {
                            sourceLinesStripped++;
                            continue;
                        }
                        else {
                            VMSHeaderStripped=true;
                        }
                    }

                    //    and source lines written
                    else {
                        writeCopyrightFiling(sourceLine);
                        sourceLinesWritten++;
					}
			    }

                sourceFileReader.close();
    			writeToLog("     Lines used: "+sourceLinesWritten);
                if (VMSManaged)
    			    writeToLog("     VMS header lines stripped: " + sourceLinesStripped);
                strippedLineTotal += sourceLinesStripped;
			}
		}
		catch ( Exception e )
		{
			writeToLog("Unrecoverable error in CreateCopyrightFiling");
			writeToLog(e.toString());
    		e.printStackTrace();
			returnCode = -1;
		}
        if (copyrightFilingLineTotal > 0) {
            try {
                fileListReader.close();
                copyrightFilingWriter.close();
            }
            catch (Exception e) {}

            if (copyrightFilingLineTotal < minCopyrightFilingLineCount) {
                writeToLog("The generated Copyright Filing is too short (" + copyrightFilingLineTotal + " lines).");
                writeToLog("It must contain at least " + minCopyrightFilingLineCount + " lines.");
                returnCode=1;
            }
            else {
                writeToLog("Copyright Filing containing " + copyrightFilingLineTotal + " lines created in " + copyrightFilingPath);
            }

			if (sourceFilesDropped > 0) {
                writeToLog(sourceFilesDropped+" source files were skipped");
                returnCode=1;
			}

            if (strippedLineTotal > 0) {
                writeToLog(strippedLineTotal + " VMS header lines were stripped.");
            }
        }

        try {
            logFileWriter.close();
        }
        catch (Exception e) {}

        return(returnCode);
	}

	static void writeToLog(String msg)
	{
        try {
            logFileWriter.write(msg, 0, msg.length());
            logFileWriter.newLine();
            System.out.println(msg);
        }
		catch ( Exception e ) {
			System.err.println("Could not write to log file " + logFilePath + ".");
			System.err.println(e.toString());
    		e.printStackTrace();
    	}
	}

    static boolean initCopyrightFiling()
	{
        String copyrightStatement =
            "//    Copyright (c) " + Calendar.getInstance().get(Calendar.YEAR) + " Allen Systems Group, Inc.  All rights reserved.";
        String copyrightHeader1[] = {
            "",
            "//$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$",
            "//",
            "//    Description:"
        };
        String copyrightHeaderProduct =
            "//                " + productName;
        String copyrightHeader2[] = {
            "//",
            "//",
            "//    Copyright:"
        };
             //    Copyright (c) <yyyy> Allen Systems Group, Inc. All rights reserved.
        String copyrightHeader3[] = {
           "//",
           "//",
           "//$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$"
        };

        try {
   		    writeCopyrightFiling(copyrightStatement);
            for (int i=0; i<copyrightHeader1.length; i++) {
                writeCopyrightFiling(copyrightHeader1[i]);
            }
   		    writeCopyrightFiling(copyrightHeaderProduct);
            for (int i=0; i<copyrightHeader2.length; i++) {
                writeCopyrightFiling(copyrightHeader2[i]);
            }
   		    writeCopyrightFiling(copyrightStatement);
            for (int i=0; i<copyrightHeader3.length; i++) {
                writeCopyrightFiling(copyrightHeader3[i]);
            }
        }
        catch (Exception e) {
            return(false);
        }

		return true;
    }

	static void writeCopyrightFiling(String line) throws Exception
	{
        try {
            copyrightFilingWriter.write(line, 0, line.length());
            copyrightFilingWriter.newLine();
            copyrightFilingLineTotal++;
        }
		catch ( Exception e ) {
            writeToLog(e.toString());
            e.printStackTrace();
			throw e;
    	}
	}

	static void Usage()
	{
		System.out.println();
		System.out.println("Parameters: <productName> <fileListPath> <copyrightFilingPath> <logFilePath> [VMS]" );
		System.out.println();
		System.out.println("     Generates a Copyright Filing for <productName> ");
        System.out.println("          from the source files listed in <fileListPath>.");
        System.out.println("     The Copyright Filing is stored in <copyrightFilingPath>.");
        System.out.println("          If <copyrightFilingPath> exists, it is replaced.");
        System.out.println("     A log file will be written to <logFilePath>.");
        System.out.println("     VMS (the only optional parameter) requests that VMS headers be stripped.");
		System.out.println();
		System.out.println("     All filenames are specified as fully qualified paths.");
        System.out.println("     Parameters with embedded spaces must be enclosed in \"double quotes\".");
		System.out.println();
        System.out.println("     Example:");
		System.out.println("        ... java ... CreateCopyrightFiling \"ViewDirect for Networks\" Q:\\txt\\CopyrightSourceList.txt Q:\\txt\\CopyrightFiling.txt Q:\\CopyrightFiling.log VMS");
		System.out.println();
	}

    // The remaining code was lifted from com.clickndone.build.util.CodeForUser.java
    // to support parsing VMS headers
    private static Map commentInfo = new TreeMap();

    private static class FileTypeCharacteristics {
        String commentStart = null;
        String commentEnd = null;
        String VMSHeaderLineStart = null;
        String alternateVMSHeaderLineStart = null;

        FileTypeCharacteristics(
            String commentStart,
            String commentEnd,
            String VMSHeaderLineStart) {
            if (commentStart != null)
                this.commentStart = commentStart;
            else this.commentStart = "";
            if (commentEnd != null)
                this.commentEnd = commentEnd;
            else this.commentEnd = "";
            if (VMSHeaderLineStart != null)
                this.VMSHeaderLineStart = VMSHeaderLineStart;
            else this.VMSHeaderLineStart = "";
        }

        FileTypeCharacteristics(
            String commentStart,
            String commentEnd,
            String VMSHeaderLineStart,
            String alternateVMSHeaderLineStart) {
            this(commentStart, commentEnd, VMSHeaderLineStart);
            this.alternateVMSHeaderLineStart = alternateVMSHeaderLineStart;
        }

        String getCommentStart () { return commentStart; }
        String getCommentEnd() { return commentEnd; }
        String getVMSHeaderLineStart () { return VMSHeaderLineStart; }
        boolean isVMSHeaderLine (String line) {
            if (alternateVMSHeaderLineStart != null) {
                if (line.toLowerCase().startsWith(alternateVMSHeaderLineStart))
                    return true;
            }
            return line.toLowerCase().startsWith(VMSHeaderLineStart);
        }
    }

    /*
     * A <code>FileTypeCharacteristics</code> object contains a start comment
     * string, an end comment string, a VMS header line initiator, and an optional
     * VMS header line non-initial initiator.  The last is only used for Pascal
     * where we seem to have made the bizarre choice to have the comment lines
     * terminate at the end of the block rather than on each line.  This was
     * probably due to the fact that we did not, at that time, support end comment
     * characters.
     */
    private static void setUpCommentInfo() {

        commentInfo.put("abap",       new FileTypeCharacteristics("* ",   "",    "* *****"));
        commentInfo.put("bas",        new FileTypeCharacteristics("'",    "",    "'*****"));
        commentInfo.put("bat",        new FileTypeCharacteristics("rem ", "",    "rem ***"));
        commentInfo.put("c",          new FileTypeCharacteristics("//",   "",    "//*****"));
        commentInfo.put("cbl",        new FileTypeCharacteristics("HEADER*", "",  "header*"));
        commentInfo.put("cmp",        new FileTypeCharacteristics("//",   "",    "//*****"));
        commentInfo.put("cpp",        new FileTypeCharacteristics("//",   "",    "//*****"));
        commentInfo.put("def",        new FileTypeCharacteristics(";",    "",    ";*****"));
        commentInfo.put("h",          new FileTypeCharacteristics("//",   "",    "//*****"));
        commentInfo.put("htm",        new FileTypeCharacteristics("<!--", "-->", "<!--*****"));
        commentInfo.put("html",       new FileTypeCharacteristics("<!--", "-->", "<!--*****"));
        commentInfo.put("java",       new FileTypeCharacteristics("//",   "",    "//*****"));
        commentInfo.put("js",         new FileTypeCharacteristics("//",   "",    "//*****"));
        commentInfo.put("jsp",        new FileTypeCharacteristics("<%--", "--%>", "<%-- *****", " *****"));
        commentInfo.put("pas",        new FileTypeCharacteristics("(*",   "*)",  "(*****", " *****"));
        commentInfo.put("policy",     new FileTypeCharacteristics("//",   "",    "//*****"));
        commentInfo.put("properties", new FileTypeCharacteristics("#",    "",    "#*****"));
        commentInfo.put("properts",   new FileTypeCharacteristics("#",    "",    "#*****"));
        commentInfo.put("rc",         new FileTypeCharacteristics("//",   "",    "//*****"));
        commentInfo.put("sh",         new FileTypeCharacteristics("#",    "",    "#*****"));
        commentInfo.put("sql",        new FileTypeCharacteristics("--",   "",    "--*****"));
        commentInfo.put("tpl",        new FileTypeCharacteristics("//",   "",    "//*****"));
        commentInfo.put("xml",        new FileTypeCharacteristics("<!--", "-->", "<!-- *****", " *****"));
    }
}
