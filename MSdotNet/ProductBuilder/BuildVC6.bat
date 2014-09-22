@echo --------------------------------------------------------------------------
@echo BuildVC6.bat - Build the MSVC++ 6 components
@echo --------------------------------------------------------------------------
@setlocal
@pushd .
@echo on

@echo %date% %time% - BuildVC6 started.    >> %BuildTimesFileName%

@set BuildVC6Error=false

@echo echo environment variables
  @set PBToolsPath
  @set BuildPath
  @set BuildOrMake
  @set ReleaseOrDebug
  @set BuildTimesFileName
  @set BuildStepStatusFileName

@echo Set up the environment.
  @call %PBToolsPath%\CallVcVars32.Bat
  @echo on
  
@echo set up output directories
  @set msvc_temp_dir=q:\build_temp\msvc
  @if exist %msvc_temp_dir% @rd %msvc_temp_dir% /s /q
  @md %msvc_temp_dir%
  @md %msvc_temp_dir%\msg
  
@echo set up logfiles
  @set BuildLogFile=%BuildPath%\build_temp\log\BuildVC6.log
  @if exist %BuildLogFile% del /f /q %BuildLogFile% 
  @set CurrentPassLogFile=q:\build_temp\msvc\BuildVC6_current_pass.log
  @if exist %CurrentPassLogFile% del /f /q %CurrentPassLogFile%
  @copy nul %CurrentPassLogFile% 
  @set LastPassLogFile=q:\build_temp\msvc\BuildVC6_last_pass.log

@rem Generate Build Compilation List
  @%PBToolsPath%\ResolveComplist q:\ /o:%msvc_temp_dir%\complist.txt /d:mak
  @if errorlevel 2 @set BuildVC6Error=true & @goto ENDCOMPILATION
  @for %%f in (%msvc_temp_dir%\complist.txt) do @if "%%~zf"=="0" @goto ENDCOMPILATION
  @copy /b %msvc_temp_dir%\complist.txt %msvc_temp_dir%\complist_relink.txt
  
@if /i "%RelinkVC6%"=="false" @goto NORELINK
@rem Generate Relink Compilation List
  @del /f /q %msvc_temp_dir%\complist_relink.txt
  @%PBToolsPath%\ResolveComplist q:\ /a /o:%msvc_temp_dir%\complist_relink.txt /d:mak > nul
  @if errorlevel 2 @set BuildVC6Error=true & @goto ENDCOMPILATION
:NORELINK
  
@rem hacks! for release xerces dll, vdrXcntrl.ocx and vdrapibrl.lib
  @set build_release_xerces=false
  @for /F %%i in (%msvc_temp_dir%\complist.txt) do @if /i "%%i"=="XercesLib.mak" @if /i "%ReleaseOrDebug%"=="Debug" @set build_release_xerces=true
  @set build_vdrxcntrl=false
  @for /F %%i in (%msvc_temp_dir%\complist.txt) do @if /i "%%i"=="vdrXcntrl.mak" @set build_vdrxcntrl=true
  @set build_vdrapibrl=false
  @if exist Q:\def\VDRAPI.DEF @set build_vdrapibrl=true
  
  
@Q:
@cd \

@echo Touch obj subdirectories
  @%PBToolsPath%\TouchDirectories.exe q:\obj "01-01-1980 00:01:00"

@rem build release dll 
  @if /i "%build_release_xerces%"=="true" @call :BUILDVC6 XercesLib.mak Release 0
  @if /i "%build_release_xerces%"=="true" @call :BUILDVC6 XercesLib.mak Release 1

@rem Make lib directory read only so that lib files don't get cleaned out.
  @attrib +r Q:\lib\*.*
  
@rem If we're doing a MAKE as opposed to a complete BUILD then we skip the clean out step.
  @if /i "%BuildOrMake%"=="MAKE" @goto NOCLEAN

@echo.
@echo Clean out all the intermediate and output files for the MSVC components (except for lib files)...
@echo.
  @for /F %%i in (%msvc_temp_dir%\complist.txt) do @call :BUILDVC6 %%i %ReleaseOrDebug% 0
  @if /i "%build_vdrxcntrl%"=="true" @(echo deleting vdrXcntrl.ocx & del /f /q q:\bin\vdrXcntrl.ocx)
  @if /i "%build_vdrapibrl%"=="true" @(echo deleting VDRAPIBRL.LIB & del /f /q Q:\LIB\VDRAPIBRL.LIB)

:NOCLEAN
@rem Make lib directory writable.
  @attrib -r Q:\lib\*.*
    

@set PassNumber=0
@set LastRelink=false

:STARTCOMPILATION
  @set /A PassNumber=PassNumber+1
  @set CurrentPassBuildError=false

@echo copy current log file to old log file and clear current logfile
  @if exist %LastPassLogFile% @del /f /q %LastPassLogFile%
  @copy %CurrentPassLogFile% %LastPassLogFile% 
  @if exist %CurrentPassLogFile% @del /f /q %CurrentPassLogFile%
  @copy nul %CurrentPassLogFile%

@echo.
@echo Compiling the MSVC and associated components -- pass number %PassNumber%
@echo.
  @for /F %%i in (%msvc_temp_dir%\complist_relink.txt) do @call :BUILDVC6 %%i %ReleaseOrDebug% %PassNumber%
  @if /i "%build_vdrxcntrl%"=="true" @(call Q:\bat\buildvdrdsp.bat %ReleaseOrDebug% %PassNumber% || set CurrentPassBuildError=true)
  @if /i "%build_vdrapibrl%"=="true" @(L:\WINAPPS\BC50\BIN\IMPLIB Q:\LIB\VDRAPIBRL.LIB Q:\DEF\VDRAPI.DEF > %msvc_temp_dir%\msg\VDRAPIBRL.ms%PassNumber% 2>&1 || set CurrentPassBuildError=true)

@rem logging
  @del /f /q %msvc_temp_dir%\msg\*.msg > nul 2>&1
  @for %%f in (%msvc_temp_dir%\msg\*.ms%PassNumber%) do @copy %%f %%~dpnf.msg > nul 2>&1
  @%PBToolsPath%\vendors\GNU\Grep.Exe -iwHsU "Invalid configuration" %msvc_temp_dir%\msg\*.msg   >> %CurrentPassLogFile%
  @%PBToolsPath%\vendors\GNU\Grep.Exe -iwHsU "error" %msvc_temp_dir%\msg\*.msg                   >> %CurrentPassLogFile%

@set CurrentPassBuildError
  @if "%CurrentPassBuildError%"=="false" @if "%LastRelink%"=="true" @goto ENDCOMPILATION
  @if "%CurrentPassBuildError%"=="false" @(set LastRelink=true& goto STARTCOMPILATION)
  @echo n|comp %CurrentPassLogFile% %LastPassLogFile% > nul 2>&1 && @(set BuildVC6Error=true& goto ENDCOMPILATION)
  @if "%PassNumber%"=="7" @(set BuildVC6Error=true& goto ENDCOMPILATION)
  @goto STARTCOMPILATION
:ENDCOMPILATION

@echo MSVC build messages
@type %CurrentPassLogFile% >> %BuildLogFile%
  
@echo generate status message
  @set BuildVC6Error
  @if /i     "%BuildVC6Error%"=="false" @echo BuildVC6: Success: file://%BuildLogFile% >> %BuildStepStatusFileName%
  @if /i not "%BuildVC6Error%"=="false" @echo BuildVC6: Error: file://%BuildLogFile% >> %BuildStepStatusFileName%

@echo %date% %time% - BuildVC6 complete.    >> %BuildTimesFileName%

@echo.
@echo BuildVC6.bat complete.
@echo --------------------------------------------------------------------------
@echo.

@popd
@endlocal


:BUILDVC6
@if "%1"=="" (set CurrentPassBuildError=true& goto :eof)
@if "%2"=="" (set CurrentPassBuildError=true& goto :eof)
@if "%3"=="" (set CurrentPassBuildError=true& goto :eof)

@set clean=
@if "%3"=="0" set clean=CLEAN
@set projectCfg=
@for /F "tokens=* usebackq" %%i in (`%PBToolsPath%\GetVC6ProjectConfiguration q:\mak\%1 %2`) do @set projectCfg=%%i

@echo   nmake -f q:\Mak\%1 -nologo -d CFG="%projectCfg%" %clean%
@set errorlevel=
@nmake -f q:\Mak\%1 -nologo -d CFG="%projectCfg%" %clean% > %msvc_temp_dir%\msg\%~n1.ms%3 2>&1

@set BuildVC6RC=%errorlevel%
@set BuildVC6RC
@if "%BuildVC6RC%" NEQ "0" set CurrentPassBuildError=true
@goto :eof



