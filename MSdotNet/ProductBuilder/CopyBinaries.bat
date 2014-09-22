REM ********************************************************************
REM *** Copyright (c) 1992-2013 Allen Systems Group, Inc.  All rights reserved.
REM ********************************************************************
REM ***$Workfile:   CopyBinaries.bat  $
REM *** $Archive:   I:\Archives\BAT\2\CopyBinaries.b~t  $
REM ***$Revision:   2.1  $
REM ***  $Author:   KOFORNAG  $
REM ***    $Date:   12 Oct 2006 16:57:16  $
REM *** $Modtime:   12 Oct 2006 16:57:22  $
REM ***     $Log:   I:\Archives\BAT\2\CopyBinaries.b~t  $
REM ***
REM ***   Rev 2.1   12 Oct 2006 16:57:16   KOFORNAG
REM ***   Merge of Project EISplitProductBuilder
REM ***
REM ***   Rev 2.0   11 Oct 2006 11:48:52   KOFORNAG
REM ***   Create Branch Revision.
REM ***
REM ***   Rev 1.1   23 Sep 2004 18:07:50   KOFORNAG
REM ***Merge of Project FIPreprodPartialBuilds
REM ***
REM ***   Rev 1.0   23 Sep 2004 10:01:02   KOFORNAG
REM ***Initialize Archive
REM ***  $Endlog$
REM ********************************************************************
@rem -----------------------------------------------------------------------------------
@rem CopyBinaries.bat - copy product binaries library to used in partial builds
@rem
@rem Usage: CopyBinaries.bat sourcePath buildPath 
@rem
@rem -----------------------------------------------------------------------------------
@echo on
@setlocal

@set sourcePath=%~1
@set buildPath=%~2


@echo Copy to the build directory

  @rem Erase the old build & message files.
  @if exist %buildPath%\build_temp     @rd /s /q %buildPath%\build_temp 2>nul
  @if exist %buildPath%\msg            @rd /s /q %buildPath%\msg 2>nul
  
  @echo Cleaning intermediate and output directories.
    @if exist %buildPath%\bin        @rd /s /q %buildPath%\bin || @echo Error cleaning bin directory
    @if exist %buildPath%\dll        @rd /s /q %buildPath%\dll || @echo Error cleaning dll directory
    @if exist %buildPath%\exe        @rd /s /q %buildPath%\exe || @echo Error cleaning exe directory
    @if exist %buildPath%\jar        @rd /s /q %buildPath%\jar || @echo Error cleaning jar directory
    @if exist %buildPath%\lib        @rd /s /q %buildPath%\lib || @echo Error cleaning lib directory
    @if exist %buildPath%\obj        @rd /s /q %buildPath%\obj || @echo Error cleaning obj directory
    @if exist %buildPath%\class_temp @rd /s /q %buildPath%\class_temp || @echo Error cleaning calss_temp directory
    @if exist %buildPath%\class      @rd /s /q %buildPath%\class || @echo Error cleaning class directory
    @if exist "%buildPath%\Disk Images"  @rd /s /q "%buildPath%\Disk Images" || @echo Error cleaning "Disk Images" directory 
    @if exist %buildPath%\Installation   @rd /s /q %buildPath%\Installation || @echo Error cleaning installation directory 

  @echo Copying network built binaries to the build path

  @xcopy /F /D /R /K /E /Y /C "%sourcePath%\*.*" "%buildPath%\" 

@endlocal
