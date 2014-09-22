@if /i not "%Install%"=="true" @goto :eof
@echo --------------------------------------------------------------------------
@echo InstallProduct.bat - Wrapper for product-specific unit-test file copy step.
@echo --------------------------------------------------------------------------
@echo on
@setlocal
@pushd .

@echo %date% %time% - Install started.     >> %BuildTimesFileName%

@set InstallError=false

@echo echo environment variables
  @set Install
  @set BuildPath
  @set BuildTimesFileName
  @set BuildStepStatusFileName

@echo setup logging
  @set LogFile=%BuildPath%\build_temp\log\Install.log
  @if exist %LogFile% @del /f /q %LogFile%
  @copy nul %LogFile% > nul

@set InstallBatchfileName=q:\bat\install_%ProductDirectory%_%ProductPlatform%.bat
@echo calling copy unit test batch file: %InstallBatchfileName%
  @if not exist %InstallBatchfileName% @goto INSTALLMISSING
  (@call %InstallBatchfileName% || @set InstallError=true) >> %LogFile% 2>&1
  @goto ENDINSTALL

:INSTALLMISSING
  @echo %InstallBatchfileName% does not exist.
  @echo %InstallBatchfileName% does not exist. >> %LogFile%
  @set InstallError=true
  @goto ENDINSTALL
  
:ENDINSTALL
  @set InstallError
  @if /i     "%InstallError%"=="false" @echo Install: Success: file://%LogFile% >> %BuildStepStatusFileName%
  @if /i not "%InstallError%"=="false" @echo Install: Error: file://%LogFile% >> %BuildStepStatusFileName%
  
@echo %date% %time% - Install complete.    >> %BuildTimesFileName%
  
@echo.
@popd
@endlocal
