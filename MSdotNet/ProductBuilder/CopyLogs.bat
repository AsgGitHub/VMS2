@rem -----------------------------------------------------------------------------------
@rem CopyLogs.bat - copy logfile to build report area
@rem
@rem Usage: copylogs sourcePath destPath [exclusionFile]
@rem
@rem -----------------------------------------------------------------------------------
@echo on
@setlocal

@set sourcePath=%~1
@set destPath=%~2
@set exclusionFile=%~3

@if "%exclusionFile%" NEQ "" @set exclusionFile=/EXCLUDE:%exclusionFile%

@echo Copying from %sourcePath%\build_cfg to %destPath%\build_cfg
  @xcopy %exclusionFile% /F /R /K /E /Y /C "%sourcePath%\build_cfg\*.*" "%destPath%\build_cfg\"
  
@echo Copying from %sourcePath%\build_temp to %destPath%\build_temp
  @xcopy %exclusionFile% /F /R /K /E /Y /C "%sourcePath%\build_temp\*.*" "%destPath%\build_temp\"

@endlocal
