@echo --------------------------------------------------------------------------
@echo StartUnix.bat
@echo --------------------------------------------------------------------------
@echo on
@setlocal
@pushd .

@echo %date% %time% - UnixBuild started.     >> %BuildTimesFileName%

@set UnixBuildError=false

@echo echo environment variables
  @set BuildUnix
  @set BuildPath
  @set BuildTimesFileName
  @set BuildStepStatusFileName

@echo setup logging
  @set LogFile=%BuildPath%\build_temp\log\UnixBuild.log
  @if not exist q:\build_temp\log @md q\build_temp\log
  @if exist %LogFile% @del /f /q %LogFile%
  @copy nul %LogFile% > nul
  @md q:\build_temp\unix_build 2>nul
  @del /f /s /q q:\build_temp\unix_build > nul
  @if exist q:\build_temp\unix_build_flag.txt del /f q:\build_temp\unix_build_flag.txt

@echo Running the Unix Build... 
  @(call :UNIXBUILDCORE || set UnixBuildError=true) >> %LogFile% 2>&1
  
  @set UnixBuildError
  @if /i     "%UnixBuildError%"=="false" @echo StartUnix: Success: file://%LogFile% >> %BuildStepStatusFileName%
  @if /i not "%UnixBuildError%"=="false" @echo StartUnix: Error: file://%LogFile% >> %BuildStepStatusFileName%
  
  @if /i     "%UnixBuildError%"=="false" @echo success> q:\build_temp\unix_build_flag.txt
  @if /i not "%UnixBuildError%"=="false" @echo failure> q:\build_temp\unix_build_flag.txt
  
@echo %date% %time% - UnixBuild complete.    >> %BuildTimesFileName%
  
@echo.
@popd
@endlocal
@exit



:UNIXBUILDCORE
@echo.
@echo Start UNIX Build

@set UnixBuildRC=0

@rem set Environment variables common to StartUnix.bat and FinishUnix.bat
@call %PBToolsPath%\SetUnixBuildEnvironment.bat

@set errorlevel=  
@call :PARTIALUNIXCOMPLIST
@set PartialUnixComplistRC=%errorlevel%
@if "%PartialUnixComplistRC%"=="2" @goto PARTIALCOMPLISTFAILED
@if "%PartialUnixComplistRC%"=="1" @goto EMPTYCOMPLIST

@call :ZIPSOURCE || @goto ZIPSOURCEFAILED
  
  @set UnixSourcePath=%UnixBuildHome%/tmp
  
  @set BuildMode=
  @if /i "%ReleaseOrDebug%"=="debug"   @set BuildMode=-d
  
  @set UnixRelinkMode=
  @if /i "%RelinkUnix%"=="true"   @set UnixRelinkMode=-l

  @set UnixBuildScriptName=build_product.sh
  @if /i "%UseManagedUnixScripts%"=="true" @set UnixBuildScriptName=build_product_M.sh
  
  @rem set up XRefBuildCmdFile
  @set XRefBuildCmdFile=q:\build_temp\unix_build\build_cmd_file_xref.txt
  @if exist %XRefBuildCmdFile% @del /f /q %XRefBuildCmdFile% 
  @if /i "%RunXrefBuild%"=="true" @echo %UnixToolsHome%/%UnixBuildScriptName% %ProductDirectoryLC% %ProductVersion% %ProductPlatformLC% %ProductLevelLC% %UnixBuildHome% %BuildMode% %CleanFlag% %UnixRelinkMode% -x > %XRefBuildCmdFile%  

  @rem set up BuildCmdFile
  @set BuildCmdFile=q:\build_temp\unix_build\build_cmd_file.txt
  @if exist %BuildCmdFile% @del /f /q %BuildCmdFile% 
  @echo %UnixToolsHome%/%UnixBuildScriptName% %ProductDirectoryLC% %ProductVersion% %ProductPlatformLC% %ProductLevelLC% %UnixBuildHome% %BuildMode% %CleanFlag% %UnixRelinkMode% > %BuildCmdFile%


@echo echo environment variables
  @set UnixBuildServer
  @set UnixUser
  @set UnixTestBuild
  @set Logfile
  @set ProductDirectoryLC
  @set ProductLevelLC
  @set ProductPlatformLC
  @set BuildMode
  @set UnixBuildBase
  @set UnixSourcePath
  
 
@rem Setup Plink (SSH client)
  @if /i "%UsePlink%"=="true" @echo y|@%PBToolsPath%\vendors\putty\plink -ssh -l %UnixUser% -pw %UnixPassword% %UnixBuildServer% echo ... > q:\build_temp\unix_build\setup_plink.msg
 
@if /i not "%RunXrefBuild%"=="true" @goto ENDXREFBUILDPASS
@if /i "%SinglePassUnixBuild%"=="true" @goto ENDXREFBUILDPASS

@set XRefBuildFailed=false
 
@echo.  
@echo FTP source up to Unix
  @set DestSourceTxtZipfile=%ProductDirectoryLC%_v%ProductVersion:.=_%_%ProductPlatformLC%_%ProductLevelLC%_txt.zip
  @set DestSourceBinZipfile=%ProductDirectoryLC%_v%ProductVersion:.=_%_%ProductPlatformLC%_%ProductLevelLC%_bin.zip
  @call %PBToolsPath%\FtpBuildFile.bat /svr:%UnixBuildServer% /usr:%UnixUser% /pwd:%UnixPassword% /put q:\source_txt.zip %UnixSourcePath%/%DestSourceTxtZipfile% || @set XRefBuildFailed=true
  @call %PBToolsPath%\FtpBuildFile.bat /svr:%UnixBuildServer% /usr:%UnixUser% /pwd:%UnixPassword% /put q:\source_bin.zip %UnixSourcePath%/%DestSourceBinZipfile% || @set XRefBuildFailed=true
 
@echo.  
@echo Unix ProductBuild - XREF Pass
@echo   log to: q:\build_temp\unix_build\xrefbuild.log
  @if /i     "%UsePlink%"=="true" @set UnixBuildCmd=%PBToolsPath%\vendors\putty\plink -batch -ssh -l %UnixUser% -pw %UnixPassword% %UnixBuildServer% -m %XRefBuildCmdFile%
  @if /i not "%UsePlink%"=="true" @set UnixBuildCmd=%PBToolsPath%\RunRemoteScript /svr:%UnixBuildServer% /port:%TelnetPort% /usr:%UnixUser% /pwd:%UnixPassword% /cmd:@%XRefBuildCmdFile%
  @%UnixBuildCmd% > q:\build_temp\unix_build\xref_build.log 2>&1
  @if "%errorlevel%" NEQ "0" @echo Build Return Code = %errorlevel% & @set XRefBuildFailed=true
  @if not exist %XRefBuildCmdFile% @set XRefBuildFailed=true
  @rem copy log files to Windows
  @set XRefLogDirectory=%BuildPath%\build_temp\unix_build\xref_log
  @rd /s /q %XRefLogDirectory% > nul 2>nul
  @md %XRefLogDirectory%
  @echo Copying XRef build logs to %XRefLogDirectory%
  @call %PBToolsPath%\FtpBuildFile.bat /svr:%UnixBuildServer% /usr:%UnixUser% /pwd:%UnixPassword% /get q:\build_temp\xref_log.zip %UnixBuildBase%/xref_log.zip
  @"%PBToolsPath%\vendors\info-zip\unzip" -aa -o q:\build_temp\xref_log.zip -d %XRefLogDirectory% > q:\build_temp\unix_build\unzip_xref_log.txt

  @set XRefBuildFailed
  @if /i not "%XRefBuildFailed%"=="false" @echo   XRefUnixBuild: Error: file://%XRefLogDirectory%\ >> %BuildStepStatusFileName%
  @if /i not "%XRefBuildFailed%"=="false" @set UnixBuildRC=1
  @if /i not "%XRefBuildFailed%"=="false" @goto ENDXREFBUILDPASS
  
  @if exist %XRefLogDirectory%\build_*_ref_warning.log @echo Reference errors occurred -- see build_*_ref_warning.log in the xref log directory. >> %LogFile%
  @if exist %XRefLogDirectory%\build_*_ref_warning.log @echo   XRefUnixBuild: Warning: file://%XRefLogDirectory%\ >> %BuildStepStatusFileName%
  @if exist %XRefLogDirectory%\build_*_ref_warning.log @goto ENDXREFBUILDPASS

  @if /i     "%XRefBuildFailed%"=="false" @echo   XRefUnixBuild: Success: file://%XRefLogDirectory%\ >> %BuildStepStatusFileName%
  @if /i     "%XRefBuildFailed%"=="false" @goto ENDXREFBUILDPASS
  
:ENDXREFBUILDPASS

@set UnixBuildFailed=false

@echo.  
@echo FTP source up to Unix
  @set DestSourceTxtZipfile=%ProductDirectoryLC%_v%ProductVersion:.=_%_%ProductPlatformLC%_%ProductLevelLC%_txt.zip
  @set DestSourceBinZipfile=%ProductDirectoryLC%_v%ProductVersion:.=_%_%ProductPlatformLC%_%ProductLevelLC%_bin.zip
  @call %PBToolsPath%\FtpBuildFile.bat /svr:%UnixBuildServer% /usr:%UnixUser% /pwd:%UnixPassword% /put q:\source_txt.zip %UnixSourcePath%/%DestSourceTxtZipfile% || @set UnixBuildFailed=true
  @call %PBToolsPath%\FtpBuildFile.bat /svr:%UnixBuildServer% /usr:%UnixUser% /pwd:%UnixPassword% /put q:\source_bin.zip %UnixSourcePath%/%DestSourceBinZipfile% || @set UnixBuildFailed=true

@echo.  
@echo Unix ProductBuild - 2nd Pass
@echo   log to: q:\build_temp\unix_build\unix_build.log
  @if /i     "%UsePlink%"=="true" @set UnixBuildCmd=%PBToolsPath%\vendors\putty\plink -batch -ssh -l %UnixUser% -pw %UnixPassword% %UnixBuildServer% -m %BuildCmdFile%
  @if /i not "%UsePlink%"=="true" @set UnixBuildCmd=%PBToolsPath%\RunRemoteScript /svr:%UnixBuildServer% /port:%TelnetPort% /usr:%UnixUser% /pwd:%UnixPassword% /cmd:@%BuildCmdFile%
  @%UnixBuildCmd% > q:\build_temp\unix_build\unix_build.log 2>&1
  @if "%errorlevel%" NEQ "0" @echo Build Return Code = %errorlevel% & @set UnixBuildFailed=true
  @if not exist %BuildCmdFile% @set UnixBuildFailed=true
  @rem copy log files to Windows
  @set UnixLogDirectory=%BuildPath%\build_temp\unix_build\unix_log
  @rd /s /q %UnixLogDirectory% > nul 2>nul
  @md %UnixLogDirectory%
  @echo Copying Unix build logs to %UnixLogDirectory%
  @call %PBToolsPath%\FtpBuildFile.bat /svr:%UnixBuildServer% /usr:%UnixUser% /pwd:%UnixPassword% /get q:\build_temp\unix_log.zip %UnixBuildBase%/unix_log.zip
  @"%PBToolsPath%\vendors\info-zip\unzip" -aa -uo q:\build_temp\unix_log.zip -d %UnixLogDirectory% > q:\build_temp\unix_build\unzip_unix_log.txt
  
  @set UnixBuildFailed
  @if /i not "%UnixBuildFailed%"=="false" @echo   UnixBuild: Error: file://%UnixLogDirectory%\ >> %BuildStepStatusFileName%
  @if /i not "%UnixBuildFailed%"=="false" @set UnixBuildRC=1
  @if /i not "%UnixBuildFailed%"=="false" @goto ENDUNIXBUILDPASS

  @if /i     "%UnixBuildFailed%"=="false" @echo   UnixBuild: Success: file://%UnixLogDirectory%\ >> %BuildStepStatusFileName%
  @if /i     "%UnixBuildFailed%"=="false" @goto ENDUNIXBUILDPASS

:ENDUNIXBUILDPASS

    
@echo.  
@echo Copy Targets from Unix
  @set UnixBinaryZipFile=bin_archives.zip
  @call %PBToolsPath%\FtpBuildFile.bat /svr:%UnixBuildServer% /usr:%UnixUser% /pwd:%UnixPassword% /get q:\%UnixBinaryZipFile% %UnixBuildBase%/%UnixBinaryZipFile% || @set UnixBuildRC=1

@echo.
@echo Extract Targets on Windows
@echo   log to: q:\build_temp\unix_build\unzip_targets.txt
  @"%PBToolsPath%\vendors\info-zip\unzip" -b -uo q:\%UnixBinaryZipFile% -d q:\bin > q:\build_temp\unix_build\unzip_targets.txt || @goto UNZIPFAILED
 
@goto ENDUNIXBUILD
 
   
:UNZIPFAILED
  @echo.
  @echo Unzip of Unix Targets Failed
  @set UnixBuildRC=1
  @goto ENDUNIXBUILD
  
:ZIPSOURCEFAILED
  @echo.
  @echo Zip of Unix source files failed
  @set UnixBuildRC=1
  @goto ENDUNIXBUILD
  
:PARTIALCOMPLISTFAILED
  @echo.
  @echo Generation of partial Unix complist failed
  @set UnixBuildRC=1
  @goto ENDUNIXBUILD
  
:EMPTYCOMPLIST
  @echo.
  @echo Unix complist is empty
  @set UnixBuildRC=0
  @goto ENDUNIXBUILD
  
 
:ENDUNIXBUILD

@exit /b %UnixBuildRC%



:ZIPSOURCE

@echo.
@echo Create source zipfiles for Unix Build

@set ZipSourceLog=q:\build_temp\unix_build\zip_source.log

@rem clean up old files
  @if exist %ZipSourceLog% del %ZipSourceLog% /f /q
  @if exist q:\source_txt.zip del q:\source_txt.zip /f /q
  @if exist q:\source_bin.zip del q:\source_bin.zip /f /q
  @if exist q:\bin_archives.zip del q:\bin_archives.zip /f /q  

@echo.
@echo Copy Source to Temp Directory -- log to: %ZipSourceLog%
@echo Copy Source to Temp Directory >> %ZipSourceLog%
  @set TEMPZIP=c:\temp\temp_zip
  @if exist %TEMPZIP% rd %TEMPZIP% /s /q
  @md %TEMPZIP%
  @xcopy "q:\*" %TEMPZIP% /f /r /k /s /y /c /e /h /exclude:%PBDataPath%\zip_exclude.lst >> %ZipSourceLog% 2>&1 || @goto ZIPSOURCEERROR
  
@echo.
@echo Change case of sub-directories for Solaris
  @"p:\vendors\sun\jdk130\bin\java" -cp %PBToolsPath% ChangeCase %TEMPZIP% 1 1 0  || @goto ZIPSOURCEERROR

@echo.  
@echo Copy VENDORS, preserving case of files and sub-directories -- log to: %ZipSourceLog%
@echo Copy VENDORS, preserving case of files and sub-directories >> %ZipSourceLog%
  @md %TEMPZIP%\vendors
  @xcopy "q:\vendors\*" %TEMPZIP%\vendors /f /r /k /s /y /c /e /h >> %ZipSourceLog% 2>&1 || @goto ZIPSOURCEERROR

@echo.
@echo Set \bin attributes to RW to allow Unix build to overwrite targets
  @attrib -r -a -s -h %TEMPZIP%\bin\* /s

@rem change to temp directory
  @c:
  @cd %TEMPZIP%

@echo.
@echo Add ascii files to source_txt.zip -- log to: %ZipSourceLog%
@echo Add ascii files to source_txt.zip >> %ZipSourceLog%
  @"%PBToolsPath%\vendors\info-zip\zip" -r q:\source_txt.zip * -i@%PBDataPath%\asciizip.lst >> %ZipSourceLog% 2>&1 || @goto ZIPSOURCEERROR

@echo.  
@echo Add binary files to source_bin.zip -- log to: %ZipSourceLog%
@echo Add binary files to source_bin.zip >> %ZipSourceLog%
  @"%PBToolsPath%\vendors\info-zip\zip" -r q:\source_bin.zip * -x@%PBDataPath%\asciizip.lst >> %ZipSourceLog% 2>&1 || @goto ZIPSOURCEERROR
  
@rem clean up source copy
  @cd \
  @rd %TEMPZIP% /s /q
  
@echo.
@echo ZipSource Succeeded
@exit /b 0

:ZIPSOURCEERROR
@echo.
@echo ZipSource Failed
@exit /b 1



:PARTIALUNIXCOMPLIST

@set PartialUnixComplistLog=q:\build_temp\unix_build\partial_unix_complist.log
@set TempComplistAll=q:\build_temp\unix_build\temp_complist_all.txt
@set TempComplist=q:\build_temp\unix_build\temp_complist.txt
@set PartialComplist=q:\txt\partial_complist.txt

@del /f /q %PartialUnixComplistLog% 2>nul
@del /f /q %TempComplistAll% 2>nul
@del /f /q %TempComplist% 2>nul
@del /f /q %PartialComplist% 2>nul

@echo Generate Unordered Full Complist -- log to: %PartialUnixComplistLog%
@echo Generate Unordered Full Complist >> %PartialUnixComplistLog%
  @%PBToolsPath%\ResolveComplist q:\ /o:%TempComplistAll% /d:make /a >> %PartialUnixComplistLog%
  @if errorlevel 2 @exit /b 2

@echo Generate Unordered Partial Complist -- log to: %PartialUnixComplistLog%
@echo Generate Unordered Partial Complist >> %PartialUnixComplistLog%
  @set errorlevel=
  @%PBToolsPath%\ResolveComplist q:\ /o:%TempComplist% /d:make >> %PartialUnixComplistLog%
  @set PartialComplistRC=%errorlevel%
  @if "%PartialComplistRC%" NEQ "0" @exit /b %PartialComplistRC%

@set CleanFlag=
@echo n|comp %TempComplistAll% %TempComplist% > nul 2>&1 && @set CleanFlag=-c
@if /i "%CleanBuild%"=="true" @set CleanFlag=-c
  
@echo Generate Ordered Partial Complist -- log to: %PartialUnixComplistLog%
  @for /F %%o in (q:\txt\complist_%ProductDirectoryLC%_%ProductPlatformLC%.txt) do @for /F %%u in (%TempComplist%) do @if /i "%%o"=="%%u" @echo %%o>>%PartialComplist%

@echo Ordered Complist:     >> %PartialUnixComplistLog%
@echo.                      >> %PartialUnixComplistLog%
  @type %PartialComplist%   >> %PartialUnixComplistLog%
  
@exit /b 0
