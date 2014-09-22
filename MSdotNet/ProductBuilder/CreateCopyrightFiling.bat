@echo off
REM ********************************************************************
REM *** Copyright (c) 1992-2013 Allen Systems Group, Inc.  All rights reserved.
REM ********************************************************************
REM ***$Workfile:   CreateCopyrightFiling.bat  $
REM *** $Archive:   I:\Archives\BAT\1\CreateCopyrightFiling.b~t  $
REM ***$Revision:   1.1  $
REM ***  $Author:   shermank  $
REM ***    $Date:   25 Feb 2010 09:37:06  $
REM *** $Modtime:   25 Feb 2010 09:37:06  $
REM ***     $Log:   I:\Archives\BAT\1\CreateCopyrightFiling.b~t  $
REM ***
REM ***   Rev 1.1   25 Feb 2010 09:37:06   shermank
REM ***   Merge of Project EIVMSGenerateCopyrightNotice
REM ***
REM ***   Rev 1.0   24 Feb 2010 15:15:36   shermank
REM ***   Initialize Archive
REM ***  $Endlog$
REM ********************************************************************
@echo on
@rem 
@rem Invoke Copyright Filing utility
@rem 
@rem %1 = product name
@rem %2 = fully-qualified path of file containing list of product source files.
@rem 
@if "%2"=="" @goto missing
@echo %0 invoked for %1
@java -jar P:\ProductBuilder\tools2\CreateCopyrightFiling.jar %1 %2 Q:\CopyrightFiling.txt Q:\build_temp\log\CopyrightFiling.log VMS
@exit /b %ERRORLEVEL%
:missing
@echo Missing parameter                            > Q:\build_temp\log\CopyrightFiling.log
@echo.										      >> Q:\build_temp\log\CopyrightFiling.log
@echo Syntax %0 ^<product^> ^<source-list-path^>  >> Q:\build_temp\log\CopyrightFiling.log
@echo.											  >> Q:\build_temp\log\CopyrightFiling.log
@exit /b -2										  >> Q:\build_temp\log\CopyrightFiling.log
