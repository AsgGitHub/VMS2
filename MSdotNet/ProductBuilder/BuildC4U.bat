@echo --------------------------------------------------------------------------
@echo BuildC4U.bat - Run the CodeForUser utility.
@echo --------------------------------------------------------------------------
@setlocal
@pushd .

@if not defined BuildStepStatusFileName @set BuildStepStatusFileName=nul
@if not defined BuildTimesFileName @set BuildTimesFileName=nul

@echo %date% %time% - BuildC4U started.    >> %BuildTimesFileName%

@set BuildC4UError=false

@rem set up C4U environment
  @set path=p:\vendors\sun\jdk130\bin;%path%
  @set ProdBld_Code4UserCommandLine=java -classpath %PBToolsPath%\cndutil.jar com.clickndone.build.util.CodeForUser

@echo echo environment variables
  @set PBToolsPath
  @set BuildPath
  @set BuildTimesFileName
  @set BuildStepStatusFileName
   
@echo set up output directories and logfile
  @set c4u_temp_dir=%BuildPath%\build_temp\c4u
  @if exist %c4u_temp_dir% @rd %c4u_temp_dir% /s /q
  @md %c4u_temp_dir%
  @set C4ULogFile=%BuildPath%\build_temp\log\C4U.log
  @if exist %C4ULogFile% del /f /q %C4ULogFile%
  @copy nul %C4ULogFile% > nul 

@echo Make the targets read / write.
  @attrib -r %BuildPath%\Bin\*.*

@echo Generate Compilation List
  %PBToolsPath%\ResolveComplist %BuildPath% /o:%c4u_temp_dir%\complist.txt /d:c4u >> %C4ULogFile% 2>&1
  @if errorlevel 2 @set BuildC4UError=true
  @for /F %%i in (%c4u_temp_dir%\complist.txt) do @echo Adding file to Code4User processing list: %BuildPath%\C4U\%%i
  @echo.

@echo Set up the c4u directory
  @copy /Y P:\products\vms\v3_0.w32\develop\txt\copyrite.txt %BuildPath%\C4U\
  @for %%f in (%BuildPath%) do @%%~df
  @cd %BuildPath%\c4u

@echo delete old targets 
  @for /F %%i in (%c4u_temp_dir%\complist.txt) do @del /f /q %BuildPath%\bin\%%i >> %C4ULogFile% 2>&1

@echo building c4u
  @rem dummy target copied to bin if c4u is successful
  @for /F %%i in (%c4u_temp_dir%\complist.txt) do (@%ProdBld_Code4UserCommandLine% %BuildPath%\C4U\%%i && @copy /Y nul %BuildPath%\bin\%%i ) >> %C4ULogFile% 2>&1

@rem check for success
  @for /F %%i in (%c4u_temp_dir%\complist.txt) do @if not exist %BuildPath%\bin\%%i @set BuildC4UError=true

@echo generate status message
  @set BuildC4UError
  @if /i     "%BuildC4UError%"=="false" @echo BuildC4U: Success: file://%C4ULogFile% >> %BuildStepStatusFileName%
  @if /i not "%BuildC4UError%"=="false" @echo BuildC4U: Error: file://%C4ULogFile% >> %BuildStepStatusFileName%

@echo %date% %time% - BuildC4U complete.    >> %BuildTimesFileName%

@echo.
@echo BuildC4U.bat complete.
@echo --------------------------------------------------------------------------
@echo.

@popd
@endlocal
