@if /i not "%CopyVBTest%"=="true" @goto :eof
@echo --------------------------------------------------------------------------
@echo CopyVBTest.bat - Copies VBTEST.Exe to the product BIN directory.
@echo --------------------------------------------------------------------------
@echo on
@setlocal
@pushd .

@echo %date% %time% - CopyVBTest started.     >> %BuildTimesFileName%

@set CopyVBTestError=false

@echo echo environment variables
  @set CopyVBTest
  @set BuildPath
  @set BuildTimesFileName
  @set BuildStepStatusFileName

@echo setup logging
  @set LogFile=%BuildPath%\build_temp\log\CopyVBTest.log
  @if exist %LogFile% @del /f /q %LogFile%
  @copy nul %LogFile% > nul

@echo Copying VBTEST.Exe to the product BIN directory... 
  @set UtilVersion=4
  @if "%ProductDirectory%"=="DDRAPP" @if "%ProductVersion%"=="4.1" @set UtilVersion=4_1
  @if "%ProductDirectory%"=="DDRAPP" @if "%ProductVersion%"=="4.2" @set UtilVersion=4_2
  (@xcopy /F /R /K /Y /C P:\PRODUCTS\UTIL\V%UtilVersion%.W32\%ProductLevel%\BIN\vbtest.exe q:\BIN\ || @set CopyVBTestError=true) >> %LogFile% 2>&1

  
  @set CopyVBTestError
  @if /i     "%CopyVBTestError%"=="false" @echo CopyVBTest: Success: file://%LogFile% >> %BuildStepStatusFileName%
  @if /i not "%CopyVBTestError%"=="false" @echo CopyVBTest: Error: file://%LogFile% >> %BuildStepStatusFileName%
  
@echo %date% %time% - CopyVBTest complete.    >> %BuildTimesFileName%
  
@echo.
@popd
@endlocal
