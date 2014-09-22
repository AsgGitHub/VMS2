@rem --------------------------------------------------------------------------
@rem BuildJava.bat - Build Java components
@rem
@rem Usage: buildjava componentName [-1] [-2] [-f firstPassDirectory] 
@rem            [-s secondPassDirectory] [-b binDirectory] [-c configDirectory]
@rem            [-m msgDirectory] [-p productSourceBaseDirectory]
@rem 
@rem         -1	  run 1st pass only (both 1st and 2nd pass are run by default)
@rem         -2   run 2nd pass only
@rem         firstPassDirectory
@rem              the base directory for the 1st pass
@rem         secondPassDirectory
@rem              the base directory for the 2nd pass
@rem         binDirectory
@rem              directory to which targets are copied 
@rem         configDirectory
@rem              directory where temporary java build files are located
@rem         msgDirectory
@rem              directory to which log files are written
@rem         productSourceBaseDirectory
@rem              base directory for source files
@rem
@rem         all directories default to the current directory if not specified
@rem
@rem --------------------------------------------------------------------------
@echo on
@set BuildJavaRC=1
@setlocal
@pushd .

@echo   component %~1

@rem set up default variables
  @if not defined ProdBld_Path @set ProdBld_Path=P:\ProductBuilder
  @rem for compatibility with INI file ProductBuilder
  @set MJ_ToolsDir=%PBToolsPath%
  @if "%MJ_ToolsDir%"=="" @set MJ_ToolsDir=%ProdBld_Path%\tools
  @set MJ_RunFirstPass=true
  @set MJ_RunSecondPass=true
  @set MJ_FirstPassDir=%CD%
  @set MJ_SecondPassDir=%CD%
  @set MJ_BinDir=%CD%
  @set MJ_ConfigDir=%CD%
  @set MJ_MsgDir=%CD%
  @set MJ_ProdDir=%CD%
  @rem JDK is kept for backwards compatibility but is now deprecated in favor of JAVA_HOME
  @rem and has no effect if JAVA_HOME is defined.
  @set JDK=1.3.0
  @rem set CLASSPATH:
  @call %MJ_ToolsDir%\SetClassPath.bat  

@rem get command line args
  @set MJ_ComponentName=%~1
  @shift
:STARTPROCESSARGS
  @if "%1"=="" @goto ENDPROCESSARGS
  @if "%1"=="-1"    @set MJ_RunSecondPass=false
  @if "%1"=="-2"    @set MJ_RunFirstPass=false
  @if /i "%1"=="-f" @set MJ_FirstPassDir=%~f2
  @if /i "%1"=="-s" @set MJ_SecondPassDir=%~f2
  @if /i "%1"=="-b" @set MJ_BinDir=%~f2
  @if /i "%1"=="-c" @set MJ_ConfigDir=%~f2
  @if /i "%1"=="-m" @set MJ_MsgDir=%~f2
  @if /i "%1"=="-p" @set MJ_ProdDir=%~f2
  @shift
  @goto STARTPROCESSARGS
:ENDPROCESSARGS

@rem hack to correct extra trailing seperator if directory is root
  @if "%MJ_FirstPassDir%"=="%MJ_FirstPassDir:~0,-1%\"   @set MJ_FirstPassDir=%MJ_FirstPassDir:~0,-1%
  @if "%MJ_SecondPassDir%"=="%MJ_SecondPassDir:~0,-1%\" @set MJ_SecondPassDir=%MJ_SecondPassDir:~0,-1%
  @if "%MJ_BinDir%"=="%MJ_BinDir:~0,-1%\"               @set MJ_BinDir=%MJ_BinDir:~0,-1%
  @if "%MJ_ConfigDir%"=="%MJ_ConfigDir:~0,-1%\"         @set MJ_ConfigDir=%MJ_ConfigDir:~0,-1%
  @if "%MJ_MsgDir%"=="%MJ_MsgDir:~0,-1%\"               @set MJ_MsgDir=%MJ_MsgDir:~0,-1%
  @if "%MJ_ProdDir%"=="%MJ_ProdDir:~0,-1%\"             @set MJ_ProdDir=%MJ_ProdDir:~0,-1%

@rem set up directories, logging
  @if "%MJ_RunFirstPass%"=="true"  @if not exist %MJ_FirstPassDir%  @md %MJ_FirstPassDir%
  @if "%MJ_RunSecondPass%"=="true" @if exist %MJ_SecondPassDir% @rd /s /q %MJ_SecondPassDir%
  @if "%MJ_RunSecondPass%"=="true" @md %MJ_SecondPassDir%
  @if "%MJ_RunSecondPass%"=="true" @if not exist %MJ_BinDir%    @md %MJ_BinDir%
  @if not exist %MJ_ConfigDir% @md %MJ_ConfigDir%
  @if not exist %MJ_MsgDir%    @md %MJ_MsgDir%
  @set MJ_LogfileBase=%MJ_MsgDir%\%MJ_ComponentName%
  @if "%MJ_RunFirstPass%"=="true"  @if exist %MJ_LogfileBase%.ms0 @del /f /q %MJ_LogfileBase%.ms0
  @if "%MJ_RunFirstPass%"=="true"  @if exist %MJ_LogfileBase%.ms1 @del /f /q %MJ_LogfileBase%.ms1
  @if "%MJ_RunSecondPass%"=="true" @if exist %MJ_LogfileBase%.ms2 @del /f /q %MJ_LogfileBase%.ms2
  @if exist %MJ_LogfileBase%.msg @del /f /q %MJ_LogfileBase%.msg

@echo parse JCD file: %MJ_ProdDir%\jcd\%MJ_ComponentName%.jcd > %MJ_LogfileBase%.ms0 2>&1
  @if not exist %MJ_ProdDir%\jcd\%MJ_ComponentName%.jcd @echo %MJ_ProdDir%\jcd\%MJ_ComponentName%.jcd is missing!
  @if not exist %MJ_ProdDir%\jcd\%MJ_ComponentName%.jcd @goto ENDBUILD  
  @%MJ_ToolsDir%\JCDParser.exe "%MJ_ProdDir%\jcd\%MJ_ComponentName%.jcd" "%MJ_ConfigDir%" >> %MJ_LogfileBase%.ms0 2>&1 || @goto ENDBUILD

@echo scan java files for resource package locations >> %MJ_LogfileBase%.ms0 2>&1
  @set BuildPath=%MJ_ProdDir%
  @%MJ_ToolsDir%\LocateResourcePackages.exe %MJ_ConfigDir% %MJ_ComponentName% %MJ_ProdDir% >> %MJ_LogfileBase%.ms0 2>&1

@echo change current directory to %MJ_ProdDir%\java >> %MJ_LogfileBase%.ms0 2>&1
  @for %%f in (%MJ_ProdDir%) do @%%~df
  @cd %MJ_ProdDir%\java
  @echo current directory is %CD% >> %MJ_LogfileBase%.ms0 2>&1


@if "%MJ_RunFirstPass%"=="false" @goto SKIPFIRSTPASS
@echo Running first build pass for %MJ_ComponentName% > %MJ_LogfileBase%.ms1 2>&1

@echo copy resource files into output directory >> %MJ_LogfileBase%.ms1 2>&1
  @for /F "tokens=1,2" %%i in (%MJ_ConfigDir%\%MJ_ComponentName%_copy2package.txt) do @call %MJ_ToolsDir%\CopyJavaResources.bat %%i %MJ_FirstPassDir% %%j >> %MJ_LogfileBase%.ms1 2>&1

@echo run java compilation tasks >> %MJ_LogfileBase%.ms1 2>&1
  @rem format of *_buildcmd.txt is: task complistFile jdk classpath args
  @for /F "tokens=1-4*" %%i in (%MJ_ConfigDir%\%MJ_ComponentName%_buildcmd.txt) do @call %MJ_ToolsDir%\JavaMakeTasks.bat %%i %%j %MJ_FirstPassDir% %%k %%l %%m >> %MJ_LogfileBase%.ms1 2>&1

:SKIPFIRSTPASS




@if "%MJ_RunSecondPass%"=="false" @goto SKIPSECONDPASS
@echo Running second build pass for %MJ_ComponentName%  > %MJ_LogfileBase%.ms2 2>&1

@echo copy resource files into output directory >> %MJ_LogfileBase%.ms2 2>&1
  @for /F "tokens=1,2" %%i in (%MJ_ConfigDir%\%MJ_ComponentName%_copy2package.txt) do @call %MJ_ToolsDir%\CopyJavaResources.bat %%i %MJ_SecondPassDir% %%j >> %MJ_LogfileBase%.ms2 2>&1

@echo run java compilation tasks >> %MJ_LogfileBase%.ms2 2>&1
  @rem format of *_buildcmd.txt is: task complistFile jdk classpath args
  @for /F "tokens=1-4*" %%i in (%MJ_ConfigDir%\%MJ_ComponentName%_buildcmd.txt) do @call %MJ_ToolsDir%\JavaMakeTasks.bat %%i %%j %MJ_SecondPassDir% %%k %%l %%m >> %MJ_LogfileBase%.ms2 2>&1

@echo delete existing target >> %MJ_LogfileBase%.ms2 2>&1
  @echo set target >> %MJ_LogfileBase%.ms2 2>&1
  @set MJ_TargetName=
  @for /F "tokens=1" %%i in (%MJ_ConfigDir%\%MJ_ComponentName%_target.txt) do @set MJ_TargetName=%%i>> %MJ_LogfileBase%.ms2 2>&1
  @if not "%MJ_TargetName%"=="" @if exist %MJ_BinDir%\%MJ_TargetName% @del /f %MJ_BinDir%\%MJ_TargetName%  >> %MJ_LogfileBase%.ms2 2>&1

@echo check for package steps >> %MJ_LogfileBase%.ms2 2>&1
  @set MJ_JAVA_HOME=
  @for /F %%i in (%MJ_ConfigDir%\%MJ_ComponentName%_JAVA_HOME.txt) do @set MJ_JAVA_HOME=%%i>> %MJ_LogfileBase%.ms2 2>&1
  @if exist %MJ_ConfigDir%\%MJ_ComponentName%_package.txt @goto SKIPDEFAULTPKG
  @for %%i in (%MJ_TargetName%) do @if /i "%%~xi"==".jar" @set DefaultPkgStep=jar
  @for %%i in (%MJ_TargetName%) do @if /i "%%~xi"==".class" @set DefaultPkgStep=class
  @echo %DefaultPkgStep%> %MJ_ConfigDir%\%MJ_ComponentName%_package.txt
:SKIPDEFAULTPKG

@echo run package steps >> %MJ_LogfileBase%.ms2 2>&1
  @for /F %%i in (%MJ_ConfigDir%\%MJ_ComponentName%_package.txt) do @call %MJ_ToolsDir%\JavaPackageSteps.bat %%i %MJ_SecondPassDir% %MJ_BinDir% %MJ_TargetName% %MJ_JAVA_HOME% >> %MJ_LogfileBase%.ms2 2>&1

@if not exist %MJ_BinDir%\%MJ_TargetName% @echo     %MJ_BinDir%\%MJ_TargetName% does not exist
@if exist %MJ_BinDir%\%MJ_TargetName% @set BuildJavaRC=0

:SKIPSECONDPASS

:ENDBUILD



@rem logging
  @echo ------------------------------------------           > %MJ_LogfileBase%.msg
  @echo %MJ_ComponentName% - Setup:                         >> %MJ_LogfileBase%.msg
  @echo ------------------------------------------          >> %MJ_LogfileBase%.msg
  @if exist %MJ_LogfileBase%.ms0 @type %MJ_LogfileBase%.ms0 >> %MJ_LogfileBase%.msg
  @echo.                                                    >> %MJ_LogfileBase%.msg
  @echo ------------------------------------------          >> %MJ_LogfileBase%.msg
  @echo %MJ_ComponentName% - 1st Pass:                      >> %MJ_LogfileBase%.msg
  @echo ------------------------------------------          >> %MJ_LogfileBase%.msg
  @if exist %MJ_LogfileBase%.ms1 @type %MJ_LogfileBase%.ms1 >> %MJ_LogfileBase%.msg
  @echo.                                                    >> %MJ_LogfileBase%.msg
  @echo ------------------------------------------          >> %MJ_LogfileBase%.msg
  @echo %MJ_ComponentName% - 2nd Pass:                      >> %MJ_LogfileBase%.msg
  @echo ------------------------------------------          >> %MJ_LogfileBase%.msg
  @if exist %MJ_LogfileBase%.ms2 @type %MJ_LogfileBase%.ms2 >> %MJ_LogfileBase%.msg
  @echo.                                                    >> %MJ_LogfileBase%.msg

@popd

@if "%MJ_RunSecondPass%"=="true" @if not "%BuildJavaRC%"=="0" @echo     %MJ_ComponentName% build failed

@exit /b %BuildJavaRC%









