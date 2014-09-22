@echo --------------------------------------------------------------------------
@echo FinishUnix.bat
@echo --------------------------------------------------------------------------
@echo on
@setlocal
@pushd .

@rem skip if not building unix components
@%PBToolsPath%\ResolveComplist q:\ /o:q:\build_temp\unix_build\dummy.txt /d:make > nul 2>&1 || @goto :eof

@echo %date% %time% - FinishUnix started.     >> %BuildTimesFileName%

@set FinishUnixError=false

@echo echo environment variables
  @set BuildPath
  @set BuildTimesFileName
  @set BuildStepStatusFileName

@echo setup logging
  @set LogFile=%BuildPath%\build_temp\log\FinishUnix.log
  @if exist %LogFile% @del /f /q %LogFile%
  @copy nul %LogFile% > nul
  @set UnixBuildFlag=q:\build_temp\unix_build_flag.txt
  
@rem set Environment variables common to StartUnix.bat and FinishUnix.bat
@call %PBToolsPath%\SetUnixBuildEnvironment.bat

@echo %date% %time% - FinishUnix: Call WAITFORUNIX.    >> %BuildTimesFileName%
@call :WAITFORUNIX || set FinishUnixError=true
@echo %date% %time% - FinishUnix: End WAITFORUNIX. FinishUnixError=%FinishUnixError%.    >> %BuildTimesFileName%
@if /i not "%FinishUnixError%"=="false" @goto ENDFINISHUNIX

@(call :COPYJAVA || set FinishUnixError=true) >> %LogFile% 2>&1

@rem Check Build Status
@for /F "tokens=1" %%i in (%UnixBuildFlag%) do @if not "%%i"=="success" @goto ENDFINISHUNIX

@(call :UNIXPOSTBUILD || set FinishUnixError=true) >> %LogFile% 2>&1
  
:ENDFINISHUNIX
 
  @set FinishUnixError
  @if /i     "%FinishUnixError%"=="false" @echo FinishUnix: Success: file://%LogFile% >> %BuildStepStatusFileName%
  @if /i not "%FinishUnixError%"=="false" @echo FinishUnix: Error: file://%LogFile% >> %BuildStepStatusFileName%
  
@echo %date% %time% - FinishUnix complete.    >> %BuildTimesFileName%
  
@echo.
@popd
@endlocal
@goto :eof



:WAITFORUNIX
@rem Set up environment to wait for Unix build to complete
  @set WaitCount=0
  @set MaxWait=360
  @if not "%1"=="" @set MaxWait=%1

@echo Will wait %MaxWait% minutes for Unix Build to Complete
@echo Will wait %MaxWait% minutes for Unix Build to Complete >> %LogFile% 2>&1
:WAITSTARTUNIX
  @if exist %UnixBuildFlag% @goto WAITENDUNIX
  @if "%WaitCount%"=="%MaxWait%" @goto WAITTIMEOUTUNIX
  @rem Sleep for 60 seconds
  @cscript %PBToolsPath%\Sleep.js 60 > nul
  @set /A WaitCount=WaitCount+1
  @echo Waiting for %WaitCount% minute(s)
  @echo Waiting for %WaitCount% minute(s) >> %LogFile% 2>&1
  @goto WAITSTARTUNIX
:WAITENDUNIX
  @echo Unix Product Build Completed
  @echo Unix Product Build Completed >> %LogFile% 2>&1
  @exit /b 0
:WAITTIMEOUTUNIX
  @taskkill /fi "windowtitle eq C:\WINDOWS\system32\cmd.exe - P:\ProductBuilder\BuildSteps\StartUnix.bat"
  @taskkill /fi "windowtitle eq C:\WINDOWS\system32\cmd.exe - q:\bat\StartUnix.bat"
  @rem NOTE: Remove the following line when ABSNET script is standardized
  @taskkill /fi "windowtitle eq C:\WINDOWS\system32\cmd.exe - q:\bat\StartUnixABSNET.bat"
  @echo Timed out waiting for Unix Build to Complete - StartUnix process killed
  @echo Timed out waiting for Unix Build to Complete - StartUnix process killed >> %LogFile% 2>&1
  @exit /b 1




:COPYJAVA
@echo copy Java components and XML files to Unix

@echo.  
@echo Create zipfiles for targets built on windows
  @set WinTargetsBinZipfile=targets_win_bin.zip
  @set WinTargetsTxtZipfile=targets_win_txt.zip
  @if exist %WinTargetsBinZipfile% @del /f %WinTargetsBinZipfile%  
  @if exist %WinTargetsTxtZipfile% @del /f %WinTargetsTxtZipfile%

@rem don't copy java components for QA builds
  @if /i "%NoJavaCopy%"=="true" @exit /b 0
  
  @q:
  @cd \bin
  
  @set errorlevel=
  @"%PBToolsPath%\vendors\info-zip\zip" q:\%WinTargetsBinZipfile% *.class *.jar & @set InfoZipRC=%errorlevel%
  @if %InfoZipRC% NEQ 0 @if %InfoZipRC% NEQ 1 @if %InfoZipRC% NEQ 12 @goto COPYJAVAFAILED
  @"%PBToolsPath%\vendors\info-zip\zip" q:\%WinTargetsTxtZipfile% *.jsp *.xml *.policy & @set InfoZipRC=%errorlevel%
  @if %InfoZipRC% NEQ 0 @if %InfoZipRC% NEQ 1 @if %InfoZipRC% NEQ 12 @goto COPYJAVAFAILED
    
@echo.  
@echo Copy .jar, .class, and .jsp files up to Unix
  @if exist q:\%WinTargetsBinZipfile% @call %PBToolsPath%\FtpBuildFile.bat /svr:%UnixBuildServer% /usr:%UnixUser% /pwd:%UnixPassword% /put q:\%WinTargetsBinZipfile% %UnixBuildBase%/%WinTargetsBinZipfile% || @goto COPYJAVAFAILED
  @if exist q:\%WinTargetsTxtZipfile% @call %PBToolsPath%\FtpBuildFile.bat /svr:%UnixBuildServer% /usr:%UnixUser% /pwd:%UnixPassword% /put q:\%WinTargetsTxtZipfile% %UnixBuildBase%/%WinTargetsTxtZipfile% || @goto COPYJAVAFAILED
  
@exit /b 0

:COPYJAVAFAILED
  @echo Copy Java Reported as Failed
  @exit /b 1



:UNIXPOSTBUILD

@echo.  
@echo Post Build Unix Steps
@echo   log to: q:\build_temp\unix_build\post_build.log

  rem set up PostBuildCmdFile
  set PostBuildCmdFile=q:\build_temp\unix_build\post_build_cmd_file.txt
  if exist %PostBuildCmdFile% @del /f /q %PostBuildCmdFile% 
  echo %UnixToolsHome%/post_build_steps.sh %ProductDirectoryLC% %ProductVersion% %ProductPlatformLC% %ProductLevelLC% %UnixBuildBase% > %PostBuildCmdFile%

  set PostBuildFailed=false  
  if /i     "%UsePlink%"=="true" @set UnixPostBuildCmd=%PBToolsPath%\vendors\putty\plink -batch -ssh -l %UnixUser% -pw %UnixPassword% %UnixBuildServer% -m %PostBuildCmdFile%
  if /i not "%UsePlink%"=="true" @set UnixPostBuildCmd=%PBToolsPath%\RunRemoteScript /svr:%UnixBuildServer% /port:%TelnetPort% /usr:%UnixUser% /pwd:%UnixPassword% /cmd:@%PostBuildCmdFile%

@echo %date% %time% - FinishUnix: Call PostBuild command via %UnixPostBuildCmd% >> %BuildTimesFileName%

  @%UnixPostBuildCmd% > q:\build_temp\unix_build\post_build.log 2>&1
  @if errorlevel 1 @set PostBuildFailed=true
  @if not exist %PostBuildCmdFile% @set PostBuildFailed=true

@echo %date% %time% - FinishUnix: End PostBuild command. PostBuildFailed=%PostBuildFailed% >> %BuildTimesFileName%

  @if /i "%PostBuildFailed%"=="true" @goto POSTBUILDFAILED
  @echo Post Unix Build Steps Reported as Succeeded
  @exit /b 0

:POSTBUILDFAILED
  @echo Post Unix Build Steps Reported as Failed
  @exit /b 1

