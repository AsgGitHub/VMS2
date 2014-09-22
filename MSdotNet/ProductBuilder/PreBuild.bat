@echo --------------------------------------------------------------------------
@echo PreBuild.bat
@echo.
@echo Usage: prebuild
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
  
@echo ensure q:\bin and q:\lib exist
  @md q:\bin
  @md q:\lib
  
@echo Copy managed DLLs from the DLL directory to the BIN directory.
  @xcopy /F /D /R /K /S /Y /C "q:\DLL\*.DLL" "q:\BIN\"

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

