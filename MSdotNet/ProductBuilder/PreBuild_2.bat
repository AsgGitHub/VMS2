@echo --------------------------------------------------------------------------
@echo PreBuild_2.bat
@echo.
@echo Usage: prebuild_2
@echo.
@echo --------------------------------------------------------------------------
@setlocal
@pushd .

@set StartPreBuildDateTime=%date% %time%

@rem echo environment variables
  @set PBToolsPath
  @set BuildPath
  @set SetReadWrite
  @set BuildTimesFileName
  @set BuildStepStatusFileName
  @set RemoveOldLogFiles
  
@echo.
@echo Check for the build directory
  @if not exist q:\ @goto ERRORNOBUILDPATH
 
@echo Erase the old message and log files.
  @if exist q:\msg @rd /q /s q:\msg
  @md q:\msg
  @if /i "%RemoveOldLogFiles%"=="true" @if exist q:\build_temp @rd /s /q q:\build_temp
  @if not exist q:\build_temp\log @md q:\build_temp\log
 
@echo Initialize logging
  @set LogFile=%BuildPath%\build_temp\log\PreBuild.log
  @if exist %LogFile% del /f /q %LogFile%
  @copy nul %LogFile% > nul

@echo Initialize the files that track build times and results.
  @if exist %BuildTimesFileName% @del /f /q %BuildTimesFileName%
  @if exist %BuildStepStatusFileName% @del /f /q %BuildStepStatusFileName%

@echo Set files to read write
  @call %PBToolsPath%\SetReadWrite.bat q:\

@echo removing dll, exe, and jar directories
  @rd /s /q q:\dll
  @rd /s /q q:\exe
  @rd /s /q q:\jar
  @if exist q:\dll        @echo Unable to remove q:\dll directory  >> %LogFile%
  @if exist q:\dll        @goto PREBUILDFAILED
  @if exist q:\exe        @echo Unable to remove q:\exe directory  >> %LogFile%
  @if exist q:\exe        @goto PREBUILDFAILED
  @if exist q:\jar        @echo Unable to remove q:\jar directory  >> %LogFile%
  @if exist q:\jar        @goto PREBUILDFAILED
 
@if /i not "%CleanBuild%"=="true" @goto NOCLEAN
@echo Cleaning intermediate and output directories.
  @rd /s /q q:\lib
  @if exist q:\lib        @echo Unable to remove q:\lib directory  >> %LogFile%
  @if exist q:\lib        @goto PREBUILDFAILED
  @md q:\lib

  @rd /s /q q:\obj
  @if exist q:\obj        @goto PREBUILDFAILED
  @if exist q:\obj        @echo Unable to remove q:\obj directory  >> %LogFile%
  @md q:\obj

  @rd /s /q q:\class_temp
  @if exist q:\class_temp @echo Unable to remove q:\class_temp directory  >> %LogFile%
  @if exist q:\class_temp @goto PREBUILDFAILED
  @md q:\class_temp

  @rd /s /q q:\class  
  @if exist q:\class      @echo Unable to remove q:\class directory  >> %LogFile%
  @if exist q:\class      @goto PREBUILDFAILED  
  @md q:\class

  @rd /s /q q:\dsp_temp
  @if exist q:\dsp_temp   @echo Unable to remove q:\dsp_temp directory  >> %LogFile%
  @if exist q:\dsp_temp   @goto PREBUILDFAILED
  @md q:\dsp_temp
  @xcopy q:\dsp\*.dsp q:\dsp_temp\ > nul
  @rd /s /q q:\dsp
  @if exist q:\dsp        @echo Unable to remove q:\dsp directory - *.dsp in q:\dsp_temp  >> %LogFile%
  @if exist q:\dsp        @goto PREBUILDFAILED
  @md q:\dsp
  @xcopy q:\dsp_temp\*.dsp q:\dsp\ > nul
  @rd /s /q q:\dsp_temp
  
  @rd /s /q q:\bin
  @if exist q:\bin        @echo Unable to remove q:\bin directory  >> %LogFile%
  @if exist q:\bin        @goto PREBUILDFAILED
  @md q:\bin

:NOCLEAN

@echo ensure q:\bin and q:\lib exist
  @if not exist q:\bin @md q:\bin
  @if not exist q:\lib @md q:\lib


@echo Building %ProductName% %ProductVersion% %ProductPlatform% %ProductLevel% in %BuildPath%
@echo Building %ProductName% %ProductVersion% %ProductPlatform% %ProductLevel% in %BuildPath% >> %LogFile%
  @goto PREBUILDSUCCEEDED


:ERRORNOBUILDPATH
  @echo Error: The build path, "%BuildPath%", was not found.
  @echo Error: The build path, "%BuildPath%", was not found. >> %LogFile%
  @goto PREBUILDFAILED

  
:PREBUILDFAILED
  @echo PreBuildSteps Failed
  @echo PreBuildSteps: Error: file://%LogFile% >> %BuildStepStatusFileName%  
  @rem ensure q:\bin and q:\lib exist
  @if not exist q:\bin @md q:\bin
  @if not exist q:\lib @md q:\lib
  @goto ENDPREBUILD

:PREBUILDSUCCEEDED
  @echo PreBuildSteps Succeeded
  @echo PreBuildSteps: Success: file://%LogFile% >> %BuildStepStatusFileName%
  @goto ENDPREBUILD
  

:ENDPREBUILD
@echo %StartPreBuildDateTime% - PreBuild started. >> %BuildTimesFileName%
@echo %date% %time% - PreBuild complete.          >> %BuildTimesFileName%


@echo.
@echo PreBuild.bat complete.
@echo --------------------------------------------------------------------------
@echo.

@popd
@endlocal
