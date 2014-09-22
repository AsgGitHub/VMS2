@if /i not "%CopyUnitTest%"=="true" @goto :eof
@echo --------------------------------------------------------------------------
@echo CopyUnitTestFiles.bat - Wrapper for product-specific unit-test file copy step.
@echo --------------------------------------------------------------------------
@echo on
@setlocal
@pushd .

@echo %date% %time% - CopyUnitTest started.     >> %BuildTimesFileName%

@set CopyUnitTestError=false

@echo echo environment variables
  @set CopyUnitTest
  @set BuildPath
  @set BuildTimesFileName
  @set BuildStepStatusFileName

@echo setup logging
  @set LogFile=%BuildPath%\build_temp\log\CopyUnitTest.log
  @if exist %LogFile% @del /f /q %LogFile%
  @copy nul %LogFile% > nul

@set CopyUnitTestBatchfileName=q:\bat\copy_unit_test_%ProductDirectory%_%ProductPlatform%.bat
@echo calling copy unit test batch file: %CopyUnitTestBatchfileName%
  @if not exist %CopyUnitTestBatchfileName% @goto COPYUNITTESTMISSING
  (@call %CopyUnitTestBatchfileName% || @set CopyUnitTestError=true) >> %LogFile% 2>&1
  @goto ENDCOPYUNITTEST

:COPYUNITTESTMISSING
  @echo %CopyUnitTestBatchfileName% does not exist.
  @echo %CopyUnitTestBatchfileName% does not exist. >> %LogFile%
  @set CopyUnitTestError=true
  @goto ENDCOPYUNITTEST
  
:ENDCOPYUNITTEST
  @set CopyUnitTestError
  @if /i     "%CopyUnitTestError%"=="false" @echo CopyUnitTest: Success: file://%LogFile% >> %BuildStepStatusFileName%
  @if /i not "%CopyUnitTestError%"=="false" @echo CopyUnitTest: Error: file://%LogFile% >> %BuildStepStatusFileName%
  
@echo %date% %time% - CopyUnitTest complete.    >> %BuildTimesFileName%
  
@echo.
@popd
@endlocal
