@echo --------------------------------------------------------------------------
@echo BuildHelp.bat - Build the on-line help system.
@echo --------------------------------------------------------------------------
@setlocal
@pushd .

@if not defined BuildStepStatusFileName @set BuildStepStatusFileName=nul
@if not defined BuildTimesFileName @set BuildTimesFileName=nul

@echo %date% %time% - BuildHelp started.    >> %BuildTimesFileName%

@set BuildHelpError=false

@echo echo environment variables
  @set PBToolsPath
  @set BuildPath
  @set BuildTimesFileName
  @set BuildStepStatusFileName
   
@echo set up output directories and logfile
  @set help_temp_dir=%BuildPath%\build_temp\help
  @if exist %help_temp_dir% @rd %help_temp_dir% /s /q
  @md %help_temp_dir%
  @set HelpLogFile=%BuildPath%\build_temp\log\Help.log
  @if exist %HelpLogFile% del /f /q %HelpLogFile%
  @copy nul %HelpLogFile% > nul 

@echo Generate Compilation List
  %PBToolsPath%\ResolveComplist %BuildPath% /o:%help_temp_dir%\complist.txt /d:hpj >> %HelpLogFile% 2>&1
  @if errorlevel 2 @set BuildHelpError=true

@echo make all file writable except the *.hpj files.
  @attrib -r q:\hpj\*.*
  @attrib +r q:\hpj\*.hpj

@echo building help
  @rem find the Microsoft help compiler.
  @if exist "C:\Program Files\VStudio\Common\Tools\Hcw.exe" @set Help_Compiler="C:\Program Files\VStudio\Common\Tools\Hcw.exe"
  @if exist "C:\Program Files\Microsoft Visual Studio\Common\Tools\Hcw.exe" @set Help_Compiler="C:\Program Files\Microsoft Visual Studio\Common\Tools\Hcw.exe"
  @rem switch to the proper directory.
  @q:
  @cd \hpj
  @for /F %%i in (%help_temp_dir%\complist.txt) do (@%Help_Compiler% /c /e %%i || @set BuildHelpError=true) >> %HelpLogFile% 2>&1


@echo Moving help files to the BIN directory...
  @rem Since the HLP files are built in the "wrong" place, we must move them.
  @cd \

  @attrib -r \bin\*.cnt
  @attrib +r \cnt\*.cnt
  @copy \cnt\*.cnt \bin

  @attrib -r \bin\*.hlp
  @move \hpj\*.hlp \bin
  @attrib +r \bin\*.hlp


@echo generate status message
  @set BuildHelpError
  @if /i     "%BuildHelpError%"=="false" @echo BuildHelp: Success: file://%HelpLogFile% >> %BuildStepStatusFileName%
  @if /i not "%BuildHelpError%"=="false" @echo BuildHelp: Error: file://%HelpLogFile% >> %BuildStepStatusFileName%

@echo %date% %time% - BuildHelp complete.    >> %BuildTimesFileName%

@echo.
@echo BuildHelp.bat complete.
@echo --------------------------------------------------------------------------
@echo.

@popd
@endlocal
