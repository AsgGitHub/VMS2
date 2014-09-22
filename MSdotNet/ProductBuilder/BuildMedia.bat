@if /i not "%BuildMedia%"=="true" @goto :eof
@echo --------------------------------------------------------------------------
@echo BuildMedia.bat - Build file for creating distribution media.
@echo --------------------------------------------------------------------------
setlocal
pushd .

@if not defined BuildStepStatusFileName @set BuildStepStatusFileName=nul
@if not defined BuildTimesFileName @set BuildTimesFileName=nul

@echo %date% %time% - BuildMedia started.    >> %BuildTimesFileName%

@if "%BuildPath%"=="" @set BuildPath=%ProdBld_BuildPath%
@if "%PBToolsPath%"=="" @set PBToolsPath=P:\ProductBuilder\tools2
@if "%ProductName%"=="" @set ProductName=%ProdBld_Name%
@if "%ProductVersion%"=="" @set ProductVersion=%ProdBld_Version%
@if "%ProductPlatform%"=="" @set ProductPlatform=%ProdBld_Platform%

@echo echo environment variables
  @set PBToolsPath
  @set BuildPath
  @set ProductName
  @set ProductVersion
  @set ProductPlatform
  @set BuildTimesFileName
  @set BuildStepStatusFileName

@echo.
@echo set up logfiles
  @set BuildMediaLogFile=%BuildPath%\build_temp\log\BuildMedia.log
  @if exist %BuildMediaLogFile% del /f /q %BuildMediaLogFile%
  @copy nul %BuildMediaLogFile% > nul
  @if exist C:\temp\pcopy.log     @del /f /q C:\temp\pcopy.log

@rem Prevent notepad popup in IPD files
  if "%ProdBld_NoPauses%"=="" set ProdBld_NoPauses=TRUE
  
@rem Set Default PcopyPath  
  @if "%PcopyPath%"=="" @set PcopyPath=p:\products\util\v4.w32\preprod\bin
  
@rem Set Default OutputPath  
  @if "%OutputPath%"=="" @set OutputPath=%BuildPath%
  
@rem Set Major & Minor ProductVersion.
  @set LOCALVERSION=%ProductVersion%
  @set VMAJOR=%LOCALVERSION:~0,1%
  @set VMINOR=%LOCALVERSION:~-1%
  
@rem Make the targets read/write.
  @attrib -r %BuildPath%\bin\*.*

@echo.
@echo Building installation media for: %ProductName% %VMAJOR%.%VMINOR% %ProductPlatform%

@echo.
@echo Call generic batch file to build the media.
  echo on
  call %PBToolsPath%\RunMediaBuild "%ProductName%" %VMAJOR% %VMINOR% %ProductPlatform% %BuildPath% %OutputPath% /p %PcopyPath%
  @echo off

@echo.
@echo Copy media building log to buildpath
  @copy /y C:\temp\pcopy.log %BuildMediaLogFile%

@echo.
@echo Determine success:
  @set MediaBuildSuccess=0
  @for /F "tokens=1-6" %%i in (%BuildMediaLogFile%) do ( @if "%%i %%j %%k %%l %%m %%n"=="Default - 0 error(s), 0 warning(s)" set MediaBuildSuccess=1 )
  @if     %MediaBuildSuccess%==1 @echo Media Build Succeeded.
  @if     %MediaBuildSuccess%==1 @echo BuildMedia: Success: file://%BuildMediaLogFile% >> %BuildStepStatusFileName%
  @if not %MediaBuildSuccess%==1 @echo Media Build Failed!
  @if not %MediaBuildSuccess%==1 @echo BuildMedia: Error: file://%BuildMediaLogFile% >> %BuildStepStatusFileName%

@echo %date% %time% - BuildMedia complete.    >> %BuildTimesFileName%

@echo.
@echo BuildMedia.bat complete.
@echo --------------------------------------------------------------------------
@echo.

@popd
@endlocal
