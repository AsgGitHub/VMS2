@rem --------------------------------------------------------------------------
@rem SetUnixBuildEnvironment.bat
@rem
@rem Set DOS environment variables for UnixBuild that are common to 
@rem StartUnix.bat and FinishUnix.bat
@rem
@rem --------------------------------------------------------------------------
@echo on
@pushd .

  @for /F "usebackq" %%i in (`%PBToolsPath%\lowercase %ProductDirectory%`) do @set ProductDirectoryLC=%%i
  @for /F "usebackq" %%i in (`%PBToolsPath%\lowercase %BranchOrTrunkName%`) do @set ProductLevelLC=%%i
  @for /F "usebackq" %%i in (`%PBToolsPath%\lowercase %ProductPlatform%`) do @set ProductPlatformLC=%%i

  @rem set Build Server Name
  @set UnixBuildServer=%UnixServer%
  
  @set UnixBuildHome=/export/home1
  @set UnixProductBase=/export/home1
  @if /i "%ProductPlatformLC%"=="zos" @if "%ProductLevelLC%"=="develop" @set UnixBuildHome=/u/wmmdev
  @if /i "%ProductPlatformLC%"=="zos" @if "%ProductLevelLC%"=="preprod" @set UnixBuildHome=/u/wmmpreprod
  @if /i "%ProductPlatformLC%"=="zos" @if "%ProductLevelLC%"=="prod"    @set UnixBuildHome=/u/wmmprod
  @if /i not "%UnixTestBuild%"=="false" @set UnixBuildHome=%UnixBuildHome%/home/%UnixUser%
  
  @set UnixToolsHome=/export/home1/develop/utils/build
  @if /i "%ProductPlatformLC%"=="zos" @set UnixToolsHome=/u/wmmdev/utils/build

  @set UnixProductPath=%ProductDirectoryLC%/v%ProductVersion:.=_%.%ProductPlatformLC%/%ProductLevelLC%
  @set UnixBuildBase=%UnixBuildHome%/products/%UnixProductPath%

  @set UsePlink=true
  @if /i "%ProductPlatformLC%"=="zos" @set UsePlink=false
  @set TelnetPort=23
  @if /i "%ProductPlatformLC%"=="zos" @set TelnetPort=2023


@echo.
@popd
