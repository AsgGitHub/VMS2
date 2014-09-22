@rem -----------------------------------------------------------------------------------
@rem CopyMainLog.bat - copy a single logfile - ~/build_cfg/build.log to build report area
@rem
@rem Usage: CopyMainLog sourcePath destPath
@rem
@rem -----------------------------------------------------------------------------------
@echo on
@setlocal

@set sourcePath=%~1
@set destPath=%~2

@echo Copying build.log only from %sourcePath%\build_cfg\ to %destPath%\build_cfg\
  @xcopy /F /R /K /E /Y /C "%sourcePath%\build_cfg\build.log" "%destPath%\build_cfg\"
  
@endlocal
