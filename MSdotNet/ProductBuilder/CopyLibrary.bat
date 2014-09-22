@rem -----------------------------------------------------------------------------------
@rem CopyLibrary.bat - copy product library
@rem
@rem Usage: copylibrary sourcePath destPath exclusionFile
@rem
@rem -----------------------------------------------------------------------------------
@echo on
@setlocal

@set sourcePath=%~1
@set destPath=%~2
@set exclusionFile=%~3

@echo Clean out %destPath%
  del /f /s /q %destPath%\* > nul
  rd %destPath% /s /q

@echo Copying from %sourcePath% to %destPath%
  xcopy /EXCLUDE:%exclusionFile% /F /R /K /E /Y /C %sourcePath%\*.* %destPath%\
  @rem allow copy from bincopy to bin for post-DEVELOP engineering builds
  if not exist %sourcePath%\bin xcopy /EXCLUDE:%exclusionFile% /F /R /K /E /Y /C %sourcePath%\bincopy\*.* %destPath%\bin\

@endlocal
