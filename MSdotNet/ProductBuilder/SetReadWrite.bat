@rem --------------------------------------------------------------------------
@rem SetReadWrite.bat
@rem
@rem Usage: setreadwrite path
@rem
@rem --------------------------------------------------------------------------
@setlocal

@echo.
@echo Making Directories READ WRITE...

@set ProductPath=%~f1

@rem Since the DOS attrib command can't properly find subdirectories on a
@rem NOVELL drive and since the NOVELL flag command doesn't work on local
@rem drives, we use the following:
@flag %ProductPath%\bin\*.* Rw /FO /S /C > nul 2>&1
@if errorlevel 1 @goto DOSDIR

@echo setting files to RW with flag
@flag %ProductPath%\*.* Rw /FO /S /C > nul 2>&1
@goto DONE

:DOSDIR
@echo setting files to RW with attrib
@attrib -r /s %ProductPath%\*.* > nul
@goto DONE

:DONE
@echo Complete.
@echo.

@endlocal
