@echo --------------------------------------------------------------------------
@echo SetBuildEnvironment.bat - set environment variables for build
@echo.
@echo Usage: setbuildenvironment.bat buildconfigurationfile
@echo --------------------------------------------------------------------------

@set JAVA_HOME=

@%PBPath%\tools2\SetBuildEnvironment.exe %1 %temp%\build_env.txt
@for /F "tokens=1,2 delims==" %%i in (%temp%\build_env.txt) do @set %%i=%%j
@for /F "tokens=1,2 delims==" %%i in (%temp%\build_env.txt) do @set %%i
@xcopy /r /y %temp%\build_env.txt %BuildPath%\build_temp\

@rem set dummy passwords
@%PBToolsPath%\SetDummyPasswords.exe %1

@rem map Q: to %BuildPath%
@call %PBToolsPath%\MapQ.bat %BuildPath%
