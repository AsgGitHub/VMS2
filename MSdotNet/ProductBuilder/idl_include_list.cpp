//**********************************************************************
//***** Copyright (c) 1992-2013 Allen Systems Group, Inc.  All rights reserved.
//**********************************************************************
//*****$Workfile:   idl_include_list.cpp  $
//***** $Archive:   I:\Archives\CPP\2\idl_include_list.c~p  $
//*****$Revision:   2.1  $
//*****  $Author:   KOFORNAG  $
//*****    $Date:   12 Oct 2006 16:57:22  $
//***** $Modtime:   12 Oct 2006 16:57:22  $
//*****     $Log:   I:\Archives\CPP\2\idl_include_list.c~p  $
//*****
//*****   Rev 2.1   12 Oct 2006 16:57:22   KOFORNAG
//*****   Merge of Project EISplitProductBuilder
//*****
//*****   Rev 2.0   11 Oct 2006 11:51:58   KOFORNAG
//*****   Create Branch Revision.
//*****
//*****   Rev 1.1   Sep 03 2003 15:01:30   RRUSSELL
//*****Merge of Project EXBuildHPX33
//*****
//*****   Rev 1.0   Aug 22 2003 11:36:24   RRUSSELL
//*****Initialize Archive
//*****
//***** branched from include_list.cpp v1.1
//*****
//*****  $Endlog$
//**********************************************************************

#include <iostream.h>
#include <fstream.h>
#include <string.h>

#define MAX_PATH 256
#define NUM_PATH 8
#define MAX_HIST 10240

#ifdef WIN32
#define DIR_CHAR '\\'
#else
#define DIR_CHAR '/'
#endif

bool GetIncludeFile(const char* line, char includeFile[MAX_PATH])
{
    const char* pptr = strstr(line, "#include");


    if (pptr == NULL)
        return false;

    if (strpbrk(line, "<\"") == NULL)
        return false;

    while (*pptr != '\"' && *pptr != '<')
        pptr++;

    pptr++;
    char* p1 = includeFile;
    while (*pptr != '\"' && *pptr != '>')
        *p1++ = *pptr++;

    *p1 = '\0';
    return true;
}


bool GetImportFile(const char* line, char includeFile[MAX_PATH])
{
    const char* pptr = strstr(line, "import");


    if (pptr == NULL)
        return false;

    if (strpbrk(line, "\"") == NULL)
        return false;

    while (*pptr != '\"')
        pptr++;

    pptr++;
    char* p1 = includeFile;
    while (*pptr != '\"')
        *p1++ = *pptr++;

    *p1 = '\0';
    return true;
}



bool ProcessFile(const char fileName[MAX_PATH], const char path[NUM_PATH][MAX_PATH], int numPath, int indent, char history[MAX_HIST], bool isIDL)
{
    char fullPath[MAX_PATH];
    ifstream inf;

    // open file
    bool ok = false;
    inf.open(fileName, ios::nocreate);
    if (inf.good())
        ok = true;
    else
        inf.clear();

    int iw1 = 0;
    if (! ok)
    {
        for (iw1 = 0; iw1 < numPath; iw1++)
        {
            strcpy(fullPath, path[iw1]);
            if (strlen(fullPath) > 0)
            {
                if (fullPath[strlen(fullPath) - 1] != DIR_CHAR)
                {
                    int lw1 = strlen(fullPath);
                    fullPath[lw1] = DIR_CHAR;
                    fullPath[lw1 + 1] = '\x0';
                }
            }

            strcat(fullPath, fileName);
            inf.open(fullPath, ios::nocreate);
            if (inf.good())
                break;
            else
                inf.clear();
        }
    }

    // cannot open file
    if (iw1 == numPath && !ok)
        return false;

    // get all #includes
    char line[1024];
    char includeFile[MAX_PATH];
    while (! inf.eof())
    {
        inf.getline(line, 1024);
        if (GetIncludeFile(line, includeFile))
        {
            // see if file was processed already
            if (strstr(history, includeFile) == NULL)
            {
                strcat(history, " ");
                strcat(history, includeFile);
                for (int jw1 = 0; jw1 < indent; jw1++)
                    cout << " ";

                cout << includeFile << endl;
                indent += 2;
                int histPos = strlen(history);
                ProcessFile(includeFile, path, numPath, indent, history, false);
                history[histPos] = '\x0';
                indent -= 2;
            }
        }
        else if ( isIDL && GetImportFile(line, includeFile) ) {
            // see if file was processed already
            if (strstr(history, includeFile) == NULL)
            {
                strcat(history, " ");
                strcat(history, includeFile);
                for (int jw1 = 0; jw1 < indent; jw1++)
                    cout << " ";

                cout << includeFile << endl;
                indent += 2;
                int histPos = strlen(history);
                ProcessFile(includeFile, path, numPath, indent, history, true);
                history[histPos] = '\x0';
                indent -= 2;
            }
      }
    }

    inf.close();
    return true;
}



int main(int argc, char* argv[])
{
   int retval = -1;
   try {
        char path[NUM_PATH][MAX_PATH];
        char fileName[MAX_PATH];
        char history[MAX_HIST];
        history[0] = '\x0';
        int numPath;
        int indent = 0;

        if (argc < 2)
        {
            cout << "Please specify file name!\n" << endl;
            return -2;
        }

        strcpy(fileName, argv[1]);
        for (int i = 2; i < argc; i++)
            strcpy(path[i - 2], argv[i]);

        numPath = argc - 2;
        if (numPath == 0)
        {
            strcpy(path[0], "");
            numPath = 1;
        }

        if (ProcessFile(fileName, path, numPath, indent, history, true) )
            retval = 0;
        else
            retval = -3;

    }
    catch (...) {
      retval = -1;
   }

   return retval;
}
