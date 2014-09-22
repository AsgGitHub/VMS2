@echo off
REM ********************************************************************
REM *** Copyright (c) 1992-2013 Allen Systems Group, Inc.  All rights reserved.
REM ********************************************************************
REM ***$Workfile:   ImportComponents.bat  $
REM *** $Archive:   I:\Archives\BAT\1\ImportComponents.b~t  $
REM ***$Revision:   1.1  $
REM ***  $Author:   KOFORNAG  $
REM ***    $Date:   14 Feb 2007 09:54:54  $
REM *** $Modtime:   14 Feb 2007 09:55:04  $
REM ***     $Log:   I:\Archives\BAT\1\ImportComponents.b~t  $
REM ***
REM ***   Rev 1.1   14 Feb 2007 09:54:54   KOFORNAG
REM ***   Merge of Project EIBranchProjectsBI
REM ***
REM ***   Rev 1.0   04 Jan 2007 14:21:30   KOFORNAG
REM ***   Initialize Archive
REM ***  $Endlog$
REM ********************************************************************

@echo on
@echo --------------------------------------------------------------------------
@echo ImportComponents.bat - Import files for build
@echo --------------------------------------------------------------------------
@setlocal
@pushd .
@echo off

@if not defined BuildStepStatusFileName @set BuildStepStatusFileName=nul
@if not defined BuildTimesFileName @set BuildTimesFileName=nul

@echo %date% %time% - ImportComponents started.  >> %BuildTimesFileName%

@echo echo environment variables
@set PBToolsPath
@set BuildPath
@set BuildOrMake
@set ReleaseOrDebug
@set BuildTimesFileName
@set BuildStepStatusFileName


set BuildImportStepsError=false
set ImportManifestFilename=%1
set ImportUtil=%PBToolsPath%\ImportComponents.exe

@echo Setting up logging...cleaning old log, if any
set BuildImportStepsLogFIle=%BuildPath%\build_temp\log\BuildImportSteps.log
if exist %BuildImportStepsLogFIle% del /f /q %BuildImportStepsLogFIle%
copy nul %BuildImportStepsLogFIle% > nul

@echo Checking for import manifest
if not exist %ImportManifestFilename% goto NOIMPORTMANIFEST
@echo Import manifest found!

@echo Importing files....
@call %ImportUtil%  %ImportManifestFilename% >> %BuildImportStepsLogFIle%

@echo %importUtil% errorlevel = %errorlevel%
@if not %errorlevel%==0 @set BuildImportStepsError=true
goto END

:NOIMPORTMANIFEST
@echo.
@echo Error: Import manifest file - %ImportManifestFilename% - not found. 
@echo Error: Import manifest file - %ImportManifestFilename% - not found. >> %BuildImportStepsLogFIle%
set BuildImportStepsError=true
@echo.

:END
@echo generate status message
  @set BuildImportStepsError
  @if /i     "%BuildImportStepsError%"=="false" @echo ImportComponents: Success: file://%BuildImportStepsLogFIle% >> %BuildStepStatusFileName%
  @if /i not "%BuildImportStepsError%"=="false" @echo ImportComponents: Error: file://%BuildImportStepsLogFIle% >> %BuildStepStatusFileName%

@echo %date% %time% - ImportComponents complete.    >> %BuildTimesFileName%

@echo.
@echo ImportComponents complete.
@echo --------------------------------------------------------------------------
@echo.
@echo on

@popd
@endlocal
