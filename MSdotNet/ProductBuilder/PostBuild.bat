@echo --------------------------------------------------------------------------
@echo PostBuild.bat
@echo.
@echo Usage: postbuild
@echo.
@echo --------------------------------------------------------------------------
@setlocal
@pushd .

@echo %date% %time% - PostBuild started.     >> %BuildTimesFileName%

@set PostBuildError=false

@echo set up logfile
  @set LogFile=%BuildPath%\build_temp\log\PostBuild.log
  @if exist %LogFile% del /f /q %LogFile%
  @copy nul %LogFile% > nul

@echo echo environment variables
  @set PBToolsPath
  @set BuildPath
  @set PopulatePath
  @set StablePath
  @set Populate
  @set PopulateStable
  @set SetReadOnly
  @set ProductName
  @set ProductVersion
  @set ProductPlatform
  @set ProductLevel
  @set BuildTimesFileName
  @set BuildStepStatusFileName
  @set CheckBuildResultsFileName
  @set SymStoreRoot
  
@if "%SymStoreRoot%" == "" goto SkipSym1
  @rem Create the list before copying in vendor files...
  @if exist %BuildPath%\build_temp\SymStore.lst del %BuildPath%\build_temp\SymStore.lst
  @rem First list all the PDB files
  @for %%F in (q:\bin\*.pdb) do @echo %%F >> %BuildPath%\build_temp\SymStore.lst
  @rem Add the EXE files without corresponding PDB's, and which aren't Pascal or C#.
  @for %%F in (q:\bin\*.exe) do if not exist %%~dpnF.pdb if not exist q:\dpr\%%~nF.dpr if not exist q:\vdrnet\%%~nF\%%~nF.csproj echo %%F >> %BuildPath%\build_temp\SymStore.lst
  @rem Add the DLL files without corresponding PDB's, and which aren't C#.
  @for %%F in (q:\bin\*.dll) do if not exist %%~dpnF.pdb if not exist q:\vdrnet\%%~nF\%%~nF.csproj echo %%F >> %BuildPath%\build_temp\SymStore.lst
:SkipSym1

@if /i not "%ProductLevel%"=="develop" goto ENDPOSTBUILDCOPY
@echo start post-build copy step
  @set postbuildfile=q:\bat\post_build_%ProductDirectory%_%ProductPlatform%.bat
  @echo copying files with %postbuildfile% ...
  @if exist %postbuildfile% call %postbuildfile%
@echo complete post-build copy step
:ENDPOSTBUILDCOPY
  

@if /i not "%SetReadOnly%"=="True" goto SKIPSETRO
  @echo Set files to read only
  @call %PBToolsPath%\SetReadOnly.bat %BuildPath%
  @rem except for log files ...
  @flag %BuildPath%\build_cfg\*.* Rw /FO /S /C > nul 2>&1
  @attrib -r /s %BuildPath%\build_cfg\*.* > nul 2>&1
  @flag %BuildPath%\build_temp\*.* Rw /FO /S /C > nul 2>&1
  @attrib -r /s %BuildPath%\build_temp\*.* > nul 2>&1
:SKIPSETRO


@echo Check the build against the build path.
  @echo %date% %time% - Build Test Time.              > %CheckBuildResultsFileName%
  @set CheckBuildError=false
  @echo on  
  @%PBToolsPath%\CheckBuild.Exe %BuildPath% /f:%BuildDefinitionFileName% >> %CheckBuildResultsFileName%
  @if errorlevel 1 @set CheckBuildError=true
  @echo off
  @if /i "%CheckBuildError%"=="false" @echo CheckBuild result: Successful Build
  @if /i "%CheckBuildError%"=="false" @echo CheckBuild result: Successful Build >> %LogFile%
  @if /i not "%CheckBuildError%"=="false" @echo CheckBuild result: Build Failed
  @if /i not "%CheckBuildError%"=="false" @echo CheckBuild result: Build Failed >> %LogFile%
  
@rem Copy to Populate directory
  @set PopChkldLogFile=%BuildPath%\build_temp\populate_checkbuild_results.txt
  @if exist %PopChkldLogFile% del /f /q %PopChkldLogFile%
  @if /i not "%Populate%"=="True" @goto ENDREPOPULATE
  @if /i "%BuildPath%"=="%PopulatePath%" @goto ENDREPOPULATE
  @echo Copy to Populate directory

  @echo Check for, and break, any file connections in the BIN dir. >> %LogFile%
  call %PBToolsPath%\BreakConnections.Bat %PopulatePath%\bin >> %LogFile%

  @rem Erase the old build & message files.
  @rd /s /q %PopulatePath%\build_temp 2>nul
  @rd /s /q %PopulatePath%\msg 2>nul
  
  @if /i not "%CleanBuild%"=="true" @goto NOCLEAN
  @echo Cleaning intermediate and output directories.
    @if exist %PopulatePath%\bin        @call :TryClean %PopulatePath%\bin

    @if exist %PopulatePath%\dll        @call :TryClean %PopulatePath%\dll
    @if exist %PopulatePath%\exe        @call :TryClean %PopulatePath%\exe
    @if exist %PopulatePath%\jar        @call :TryClean %PopulatePath%\jar
    @if exist %PopulatePath%\lib        @call :TryClean %PopulatePath%\lib
    @if exist %PopulatePath%\obj        @call :TryClean %PopulatePath%\obj
    @if exist %PopulatePath%\class_temp @call :TryClean %PopulatePath%\class_temp
    @if exist %PopulatePath%\class      @call :TryClean %PopulatePath%\class
    @if exist "%PopulatePath%\Disk Images"  @call :TryClean "%PopulatePath%\Disk Images"
    @if exist %PopulatePath%\Installation   @call :TryClean %PopulatePath%\Installation
    @goto NOCLEAN

    :TryClean
      @if .%1 == . (echo TryClean:  Missing argument & goto :eof)
      @rem  If it doesn't exist, there's nothing to do...
      @if not exist %1 goto :eof
      @rem  Try removing it...
      @rd /s /q %1
      @rem  If it doesn't exist now, all's well...
      @if not exist %1 goto :eof
      @rem  Try removing it one more time (in case it was a transient LAN glitch)...
      %PBToolsPath%\vendors\wait.exe "Waiting to remove %~1" 10
      @rd /s /q %1
      @rem  If it doesn't exist now, all's well...
      @if not exist %1 goto :eof
      @rem  There was an error...
      @rem  If the directory is empty, it's not so bad.  (We'll log it, but not fail.)
      @set FileExists=
      @for %%f in (%1\*) do @set FileExists=true
      @if "%FileExists%" == "" @goto NoFiles
      @set FileExists=
      @set PostBuildError=true 
      @echo "Clean" error - failed to remove directory %1, which contains the following files: >> %LogFile%
      @dir /b %1 >> %LogFile%
      @echo ... end directory listing ... >> %LogFile%
      @goto :eof

    @rem We didn't find any files, but what about subdirectories?
    :NoFiles
      @set DirExists=
      @for /d %%f in (%1\*) do @if not "%%f" == "." @if not "%%f" == ".." @set DirExists=true
      @if "%DirExists%" == "" @echo "Clean" error - failed to remove empty directory %1 >> %LogFile%
      @if "%DirExists%" == "" goto :eof
      @set DirExists=
      @set PostBuildError=true 
      @echo "Clean" error - failed to remove directory %1, which contains the following subdirectories: >> %LogFile%
      @dir /b %1 >> %LogFile%
      @echo ... end directory listing ... >> %LogFile%
      @goto :eof

  :NOCLEAN

  @echo Copying the files back to the product directory %PopulatePath%
  @echo Copying the files back to the product directory %PopulatePath%: >> %LogFile%
  @echo %date% %time% - restore drive P: started.   >> %BuildTimesFileName%

  @xcopy /F /D /R /K /E /Y /C "%BuildPath%\*.*" "%PopulatePath%\" 1>nul 2>> %LogFile%
  @if errorlevel 1 @goto REPOPULATEFAILED

  @echo %date% %time% - restore drive P: complete.  >> %BuildTimesFileName%
  @echo "Product directory re-population succeeded." >> %LogFile%

  @rem Check the build in the product directory to verify the copy.
  @echo %date% %time% - Build Test Time.                                     > %PopChkldLogFile%
  @%PBToolsPath%\CheckBuild.Exe %PopulatePath% /f:%BuildDefinitionFileName% >> %PopChkldLogFile%
  @if errorlevel 1 @goto REPOPULATECHECKFAILED

  @echo "Product directory CheckBuild succeeded." >> %LogFile%
  @goto ENDREPOPULATE

:REPOPULATEFAILED
  @set PostBuildError=true
  @echo "Product directory re-population FAILED!" >> %LogFile%
  @goto ENDREPOPULATE

:REPOPULATECHECKFAILED
  @set PostBuildError=true
  @echo "Product directory CheckBuild FAILED!  (See %PopChkldLogFile%)" >> %LogFile%
  @goto ENDREPOPULATE

:ENDREPOPULATE

@rem Save debug symbols
  @if "%SymStoreRoot%" == "" @goto SkipSym2
  @echo Save debug symbols
  @if /i not "%CheckBuildError%"=="false" @echo CheckBuild error flag set; skipping the saving of symbols. >> %LogFile%
  @if /i not "%CheckBuildError%"=="false" @goto SkipSym2
  @if /i %PostBuildError% == true @echo PostBuild error flag set; skipping the saving of symbols. >> %LogFile%
  @if /i %PostBuildError% == true @goto SkipSym2
  @if exist %BuildPath%\build_temp\log\SymStore.log del %BuildPath%\build_temp\log\SymStore.log
  @pushd %PBToolsPath%\vendors\MS_Debugging_Tools
  @symstore add /f @%BuildPath%\build_temp\SymStore.lst /s %SymStoreRoot% /t %ProductDirectory% /v %ProductVersion% /o /c "%BranchOrTrunkName% %LibraryDate%" /d %BuildPath%\build_temp\log\SymStore.log
  @find "Number of errors = 0" %BuildPath%\build_temp\log\SymStore.log > NUL
  @if errorlevel 1 @echo SymStore failed!  (See log %BuildPath%\build_temp\log\SymStore.log) >> %LogFile%
  @if not errorlevel 1 @echo SymStore succeeded.  >> %LogFile%
  @popd
:SkipSym2

@rem Copy to Stable directory
  @if /i not "%CheckBuildError%"=="false" @goto NOCOPY
  @if /i not "%PopulateStable%"=="True" @goto ENDSTABLEPOPULATE
  @echo Copy to Stable directory
  
  @echo Check for, and break, any file connections in the BIN dir. >> %LogFile%
  call %PBToolsPath%\BreakConnections.Bat %StablePath%\bin >> %LogFile%

  @echo Cleaning out the STABLE directory: %StablePath%
  @rd %StablePath% /s /q

  @echo Copying the files back to the product directory %StablePath%
  @echo Copying the files back to the product directory %StablePath%: >> %LogFile%
  @echo %date% %time% - copy stable started.        >> %BuildTimesFileName%
  
  @xcopy /F /R /K /S /Y /C /E /H "%BuildPath%\*.*" "%StablePath%\" 1>nul 2>> %LogFile%
  @if errorlevel 1 @goto STABLEPOPULATEFAILED

  @echo %date% %time% - copy stable complete.       >> %BuildTimesFileName%

  @rem Check the build in the product directory to verify the copy.
  @echo Calling CheckBuild against "stable" copy.       >> %LogFile%
  @%PBToolsPath%\CheckBuild.Exe %StablePath% /f:%BuildDefinitionFileName% >> %LogFile%
  @if errorlevel 1 @echo CheckBuild result: populate of 'STABLE' failed
  @if errorlevel 1 @echo CheckBuild result: populate of 'STABLE' failed >> %LogFile%
  @if errorlevel 1 @goto STABLEPOPULATEFAILED

  @echo "CopyStable Success!"
  @echo "CopyStable Success!" >> %LogFile%
  @goto ENDSTABLEPOPULATE


:STABLEPOPULATEFAILED
  @set PostBuildError=true
  @echo "CopyStable FAILURE!"
  @echo "CopyStable FAILURE!" >> %LogFile%
  goto ENDSTABLEPOPULATE


:ENDSTABLEPOPULATE
  @goto ENDPOSTBUILD


:NOCOPY
  @set PostBuildError=true
  @echo Stable directory was NOT populated. (CheckBuild Failed)
  @echo Stable directory was NOT populated. (CheckBuild Failed) >> %LogFile%
  @goto ENDPOSTBUILD


:ENDPOSTBUILD
  @set PostBuildError
  @if /i     "%PostBuildError%"=="false" @echo PostBuild: Success: file://%LogFile% >> %BuildStepStatusFileName%
  @if /i not "%PostBuildError%"=="false" @echo PostBuild: Error: file://%LogFile% >> %BuildStepStatusFileName%


@echo Build completed.
  @echo %date% %time% - PostBuild complete.    >> %BuildTimesFileName%
  @echo Overall Build Status: Complete: file://%BuildLogFileName% >> %BuildStepStatusFileName%

@echo Copy build_temp and build_cfg to source or stable if Populate and/or PopulateStable is True
  @if /i "%Populate%"=="True" @xcopy /f /r /y /k /c /e %BuildPath%\build_temp\*.* %PopulatePath%\build_temp\
  @if /i "%PopulateStable%"=="True" @xcopy /f /r /y /k /c /e %BuildPath%\build_temp\*.* %StablePath%\build_temp\
  @if /i "%Populate%"=="True" @xcopy /f /r /y /k /c /e %BuildPath%\build_cfg\*.* %PopulatePath%\build_cfg\
  @if /i "%PopulateStable%"=="True" @xcopy /f /r /y /k /c /e %BuildPath%\build_cfg\*.* %StablePath%\build_cfg\
  
@call %PBToolsPath%\MakeSound.bat


@echo.
@echo PostBuild.bat complete.
@echo --------------------------------------------------------------------------
@echo.

@popd
@endlocal
