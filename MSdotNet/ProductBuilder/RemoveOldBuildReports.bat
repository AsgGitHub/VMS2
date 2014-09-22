@echo off
REM ********************************************************************
REM *** Copyright (c) 1992-2013 Allen Systems Group, Inc.  All rights reserved.
REM ********************************************************************
REM ***$Workfile:   RemoveOldBuildReports.bat  $
REM *** $Archive:   I:\Archives\BAT\2\RemoveOldBuildReports.b~t  $
REM ***$Revision:   2.1  $
REM ***  $Author:   KOFORNAG  $
REM ***    $Date:   12 Oct 2006 16:57:20  $
REM *** $Modtime:   12 Oct 2006 16:57:22  $
REM ***     $Log:   I:\Archives\BAT\2\RemoveOldBuildReports.b~t  $
REM ***
REM ***   Rev 2.1   12 Oct 2006 16:57:20   KOFORNAG
REM ***   Merge of Project EISplitProductBuilder
REM ***
REM ***   Rev 2.0   11 Oct 2006 11:51:52   KOFORNAG
REM ***   Create Branch Revision.
REM ***
REM ***   Rev 1.1   Oct 17 2003 16:19:02   RRUSSELL
REM ***Merge of Project EIPRDBLD-Misc38
REM ***
REM ***   Rev 1.0   Sep 19 2003 15:34:40   RRUSSELL
REM ***Initialize Archive
REM ***  $Endlog$
REM ********************************************************************


set DeleteDate=%1

if "%DeleteDate%"=="" goto USAGE
if "%DeleteDate%" LEQ "20" goto USAGE
if "%DeleteDate%" GEQ "21" goto USAGE

for /D %%g in (P:\ProductBuilder\BuildReports\*) do @for /D %%h in ("%%g\*") do @if "%%~nh" GEQ "20" @if "%%~nh" LEQ "%DeleteDate%" @echo rd /s /q "%%h" & rd /s /q "%%h"

goto :eof

:USAGE
  echo Usage:
  echo.
  echo     RemoveOldBuildReports.bat deleteDate
  echo.
  echo     deleteDate = yyyymmdd (ex. 20030831)
  echo.
