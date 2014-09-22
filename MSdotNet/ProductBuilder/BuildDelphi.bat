@echo --------------------------------------------------------------------------
@echo BuildDelphi.bat - Build the Delphi 2.0 components.
@echo --------------------------------------------------------------------------
@setlocal
@pushd .

@if not defined BuildStepStatusFileName @set BuildStepStatusFileName=nul
@if not defined BuildTimesFileName @set BuildTimesFileName=nul

@echo %date% %time% - BuildDelphi started.    >> %BuildTimesFileName%

@set BuildDelphiError=false

@echo echo environment variables
  @set PBToolsPath
  @set BuildPath
  @set BuildTimesFileName
  @set BuildStepStatusFileName
   
@echo set up output directories and logfile
  @set delphi_temp_dir=%BuildPath%\build_temp\delphi
  @if exist %delphi_temp_dir% @rd %delphi_temp_dir% /s /q
  @md %delphi_temp_dir%
  @md %delphi_temp_dir%\msg
  @set DelphiLogFile=%BuildPath%\build_temp\log\Delphi.log
  @if exist %DelphiLogFile% del /f /q %DelphiLogFile%
  @copy nul %DelphiLogFile% > nul 

@echo Generate Compilation List
  %PBToolsPath%\ResolveComplist %BuildPath% /o:%delphi_temp_dir%\complist.txt /d:dpr >> %DelphiLogFile% 2>&1
  @if errorlevel 2 @set BuildDelphiError=true
  
@echo make files writable
  @rem create a dir for the DCU files, if it's not already there.
  @md q:\DCU 2>nul
  @attrib -r Q:\DCU\*.*
  @attrib -r Q:\DPR\*.EXE
  
@echo get a copy of the configuration file.
  @copy %PBToolsPath%\DCC32.CFG Q:\

@echo building Delphi
  @rem find the Delphi command line compiler.
  @if exist "K:\Delphi20\Bin\DCC32.EXE" @set Delphi_Compiler=K:\Delphi20\Bin\DCC32.EXE
  @if exist "L:\WinApps\Delphi20\Bin\DCC32.EXE" @set Delphi_Compiler=L:\WinApps\Delphi20\Bin\DCC32.EXE
  @if exist "C:\Program Files\Borland\Delphi 2.0\Bin\DCC32.EXE" @set Delphi_Compiler=C:\Program Files\Borland\Delphi 2.0\Bin\DCC32.EXE
  @if "%Delphi_Compiler%" == "" @echo Error:  Couldn't find Delphi compiler.
  @if "%Delphi_Compiler%" == "" @echo Couldn't find Delphi compiler. >> %DelphiLogFile%
  @if "%Delphi_Compiler%" == "" @set BuildDelphiError=true
  @if "%Delphi_Compiler%" == "" @goto CLEANUP

  @q:
  @cd \
  @rem hack for VDRNET - get rid of advgrid.pas otherwise the build will fail.
  @del /f /q Q:\pas\advgrid.pas
  @for /F %%i in (%delphi_temp_dir%\complist.txt) do (@"%Delphi_Compiler%" q:\dpr\%%i || @set BuildDelphiError=true) >> %delphi_temp_dir%\msg\%%~ni.msg 2>&1
  
:CLEANUP
@rem Clean up.
  @del /f /q q:\DCC32.CFG

@echo setup log file
  @for %%F in (%delphi_temp_dir%\msg\*.msg) do @type %%F >> %DelphiLogFile%

:STATUSMSG
@echo generate status message
  @set BuildDelphiError
  @if /i     "%BuildDelphiError%"=="false" @echo BuildDelphi: Success: file://%DelphiLogFile% >> %BuildStepStatusFileName%
  @if /i not "%BuildDelphiError%"=="false" @echo BuildDelphi: Error: file://%DelphiLogFile% >> %BuildStepStatusFileName%

@echo %date% %time% - BuildDelphi complete.    >> %BuildTimesFileName%

@echo.
@echo BuildDelphi.bat complete.
@echo --------------------------------------------------------------------------
@echo.

:END
@popd
@endlocal
