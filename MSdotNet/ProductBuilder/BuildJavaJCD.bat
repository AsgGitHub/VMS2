@echo --------------------------------------------------------------------------
@echo BuildJavaJCD.bat - Build Java components based on JCD files
@echo --------------------------------------------------------------------------
@setlocal
@pushd .

@if not defined BuildStepStatusFileName @set BuildStepStatusFileName=nul
@if not defined BuildTimesFileName @set BuildTimesFileName=nul

@echo %date% %time% - BuildJavaJCD started.    >> %BuildTimesFileName%

@set BuildJavaJCDError=false

@echo echo environment variables
  @set PBToolsPath
  @set BuildPath
  @set BuildTimesFileName
  @set BuildStepStatusFileName
  
@if /i "%QuickJavaBuild%"=="true" @goto ENDCLEANUP
@echo erase the old output and message directories.
  @if exist q:\class\nul      @rd /s /q q:\class
  @if exist q:\class_temp\nul @rd /s /q q:\class_temp
  @if exist q:\msg\java\nul   @rd /s /q q:\msg\java
:ENDCLEANUP
  
@echo set up output directories and logfile
  @set java_temp_dir=q:\build_temp\java
  @if exist %java_temp_dir% @rd %java_temp_dir% /s /q
  @md %java_temp_dir%
  @md %java_temp_dir%\msg
  @set JavaLogFile=%BuildPath%\build_temp\log\Java.log
  @if exist %JavaLogFile% del /f /q %JavaLogFile%
  @copy nul %JavaLogFile% > nul

@rem Generate 1st pass Compilation List
  @%PBToolsPath%\ResolveComplist %BuildPath% /a /o:%java_temp_dir%\complist_first_pass.txt /d:jcd >> %JavaLogFile% 2>&1
  @if errorlevel 2 @set BuildJavaJCDError=true

@rem Generate 2nd pass Compilation List
  @%PBToolsPath%\ResolveComplist %BuildPath% /o:%java_temp_dir%\complist.txt /d:jcd >> %JavaLogFile% 2>&1
  @if errorlevel 2 @set BuildJavaJCDError=true
  @for %%f in (%java_temp_dir%\complist.txt) do @if "%%~zf"=="0" @goto ENDCOMPILATION
  
@if /i "%QuickJavaBuild%"=="true" @(del /q /f %java_temp_dir%\complist_first_pass.txt & copy /b %java_temp_dir%\complist.txt %java_temp_dir%\complist_first_pass.txt)
  
@Q:
@cd \

@echo hacks ...
  @echo.
  @for /F %%i in (%java_temp_dir%\complist_first_pass.txt) do @if /i "%%i"=="vdrapi_jar.jcd" @call %PBToolsPath%\BuildJava.bat %%~ni -1 -f Q:\class -c Q:\class_temp -m %java_temp_dir%\msg -p Q:\
  @echo.

@echo running java first pass ...
  @echo.
  @for /F %%i in (%java_temp_dir%\complist_first_pass.txt) do @if not "%%~ni"=="" @call %PBToolsPath%\BuildJava.bat %%~ni -1 -f Q:\class -c Q:\class_temp -m %java_temp_dir%\msg -p Q:\
  @echo.

@echo running java second pass ...
  @echo.
  @for /F %%i in (%java_temp_dir%\complist.txt) do @if not "%%~ni"=="" @call %PBToolsPath%\BuildJava.bat %%~ni -2 -s Q:\class_temp\%%~ni -c Q:\class_temp -m %java_temp_dir%\msg -b Q:\bin -p Q:\ || @set BuildJavaJCDError=true
  @echo.

:ENDCOMPILATION

@echo Java build messages
  @echo ---------------------------------------------------- Java Log (includes errors)  > %JavaLogFile%
  @for %%F in (%java_temp_dir%\msg\*.msg) do @type %%F                                  >> %JavaLogFile%
  @echo.                                                                                >> %JavaLogFile%
  @echo ---------------------------------------------------- The End                    >> %JavaLogFile%

@echo generate status message
  @set BuildJavaJCDError
  @if /i     "%BuildJavaJCDError%"=="false" @echo BuildJavaJCD: Success: file://%JavaLogFile% >> %BuildStepStatusFileName%
  @if /i not "%BuildJavaJCDError%"=="false" @echo BuildJavaJCD: Error: file://%JavaLogFile% >> %BuildStepStatusFileName%

@echo %date% %time% - BuildJavaJCD complete.    >> %BuildTimesFileName%

@echo.
@echo BuildJavaJCD.bat complete.
@echo --------------------------------------------------------------------------
@echo.

@popd
@endlocal

