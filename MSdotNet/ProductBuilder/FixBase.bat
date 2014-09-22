@if /i not "%RunFixBase%"=="true" @goto :eof
@echo --------------------------------------------------------------------------
@echo FixBase.bat - rebase dll's
@echo --------------------------------------------------------------------------
@echo on
@setlocal
@pushd .
@echo.

@echo %date% %time% - FixBase started.     >> %BuildTimesFileName%

@set FixBaseError=false

@echo echo environment variables
  @set PBToolsPath
  @set RunFixBase
  @set BuildPath
  @set BuildTimesFileName
  @set BuildStepStatusFileName

@q:
@cd \bin

@echo Rebasing dll's ...
  @set LogFile=%BuildPath%\build_temp\log\FixBase.log
  @set BakFile=%BuildPath%\build_temp\log\FixBase.bak
  @if exist %BakFile% @del /f /q %BakFile%
  @if exist %LogFile% @rename %LogFile% %BakFile%
  @if exist %LogFile% @del /f /q %LogFile%
  @copy nul %LogFile% > nul
  
  @set RebaseFiles=afp*.dll agm.dll auditcntclients.dll auth_*.dll cooltype.dll cvrtwrap.dll dafutil.dll dataexporter.dll dd*.dll docservercom.dll ebppauth.dll epfaxapi.dll imageutilities.dll lcom*.dll lf*.dll lincjet.dll localio.dll lpfd*.dll lt*.dll lu*.dll mpdibdll.dll pagescomposermanager.dll pc*.dll pd*.dll pg*.dll process.dll rc*.dll rds*.dll reg*.dll resw*.dll rpcdoc*.dll srv*.dll ssiowrap.dll systemal.dll threadsal.dll tsmio.dll vdr*.dll xceedzip.dll xerces-c_1_3.dll xeroxbitmapdll.dll zlib.dll

  @%PBToolsPath%\rebase.exe -b 28000000 -d -c %LogFile% %RebaseFiles% > q:\build_temp\log\FixBase.con 2>&1 || @set FixBaseError=true
  
  @type q:\build_temp\log\FixBase.con >> %LogFile%


:ENDPOSTBUILD
  @set FixBaseError
  @if /i     "%FixBaseError%"=="false" @echo FixBase: Success: file://%LogFile% >> %BuildStepStatusFileName%
  @if /i not "%FixBaseError%"=="false" @echo FixBase: Error: file://%LogFile% >> %BuildStepStatusFileName%

@echo %date% %time% - FixBase complete.    >> %BuildTimesFileName%

@echo.
@popd
@endlocal
