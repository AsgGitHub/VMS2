@rem --------------------------------------------------------------------------
@rem FtpBuildFile.bat - FTP a file to/from the build server
@rem
@rem Usage: ftpbuildfile /svr:build_server [/port:port#] /usr:username
@rem            /pwd:password [ /put | /get ] localfile remotefile
@rem
@rem --------------------------------------------------------------------------
@echo on
@setlocal
@pushd .

@echo ftpbuildfile.bat %*

@rem set initial values
  @set FTPServer=
  @set FTPPort=
  @set FTPUsername=
  @set FTPPassword=
  @set FTPPut=true
  @set FTPLocalfile=
  @set FTPLocalDirectory=
  @set FTPRemoteFile=
  @set FTPRemoteDirectory=
  @set ProcessedLocalfile=false
  @set ProcessedRemoteFile=false

@rem get command line args
:STARTPROCESSARGS
  @set currentArg=%~1
  @if "%currentArg%"=="" @goto ENDPROCESSARGS
  @if /i "%currentArg:~0,5%"=="/svr:"  @(set FTPServer=%currentArg:*:=%& shift & goto STARTPROCESSARGS)
  @if /i "%currentArg:~0,6%"=="/port:" @(set FTPPort=%currentArg:*:=%& shift & goto STARTPROCESSARGS)
  @if /i "%currentArg:~0,5%"=="/usr:" @(set FTPUsername=%currentArg:*:=%& shift & goto STARTPROCESSARGS)
  @if /i "%currentArg:~0,5%"=="/pwd:" @(set FTPPassword=%currentArg:*:=%& shift & goto STARTPROCESSARGS)
  @if /i "%currentArg%"=="/put" @(set FTPPut=true& shift & goto STARTPROCESSARGS)
  @if /i "%currentArg%"=="/get" @(set FTPPut=false& shift & goto STARTPROCESSARGS)
  @if /i "%ProcessedLocalfile%"=="false" @(set FTPLocalfile=%currentArg%& shift & set ProcessedLocalFile=true& goto STARTPROCESSARGS)
  @if /i "%ProcessedRemotefile%"=="false" @(set FTPRemotefile=%currentArg%& shift & set ProcessedRemoteFile=true& goto STARTPROCESSARGS)
  @goto ARGERROR
:ENDPROCESSARGS

@rem local and remote directories
  @for %%a in (%FTPLocalFile%) do @set FTPLocalDirectory=%%~dpa
  @for %%a in (%FTPRemoteFile%) do @set FTPRemoteDirectory=%%~pa
  @set FTPRemoteDirectory=%FTPRemoteDirectory:\=/%

@rem check for missing arguments
  @if "%FTPServer%"=="" @goto :ARGERROR
  @if "%FTPUsername%"=="" @goto :ARGERROR
  @if "%FTPPassword%"=="" @goto :ARGERROR
  @if "%FTPLocalfile%"=="" @goto :ARGERROR
  @if "%FTPLocalDirectory%"=="" @goto :ARGERROR
  @if "%FTPRemotefile%"=="" @goto :ARGERROR
  @if "%FTPRemoteDirectory%"=="" @goto :ARGERROR

@if "%FTPPut%"=="false" @md %FTPLocalDirectory% > nul 2>&1
@if "%FTPPut%"=="false" @if exist %FTPLocalfile% @del /f /q %FTPLocalfile%

@rem prepare ftp script and log files
  @set FtpScriptFile=%temp%\ftpscriptfile.txt
  @if exist %FtpScriptFile% del /f /q %FtpScriptFile%
  @set FtpLogFile=%temp%\ftplogfile.txt
  @if exist %FtpLogFile% del /f /q %FtpLogFile%
  
  @echo open %FTPServer% %FTPPort%                                 >> %FtpScriptFile%
  @echo user %FTPUsername% %FTPPassword%                           >> %FtpScriptFile%
  @echo binary                                                     >> %FtpScriptFile%
  @echo literal pasv                                               >> %FtpScriptFile%
  @if "%FTPPut%"=="true" @echo mkdir %FTPRemoteDirectory%          >> %FtpScriptFile%
  @if "%FTPPut%"=="true" @echo delete %FTPRemotefile%              >> %FtpScriptFile%
  @if "%FTPPut%"=="true" @echo put %FTPLocalfile% %FTPRemotefile%  >> %FtpScriptFile%
  @if "%FTPPut%"=="false" @echo get %FTPRemotefile% %FTPLocalfile% >> %FtpScriptFile%
  @echo bye                                                        >> %FtpScriptFile%
  
@echo %date% %time% - FTP transfer started.
  
@rem run ftp script
  @ftp -n -w:16384 -s:%FtpScriptFile% > %FTPLogFile% 2>&1  

@echo %date% %time% - FTP transfer completed.   
  
@rem process Logfile for success/failure
  @set RCProcessing=false
  @set FTPRC=1
  @for /F "tokens=1,2 delims=> " %%l in (%FTPLogFile%) do @call :PROCESSFTPLINE %%l %%m

@echo RC=%FTPRC%
@if not "%FTPRC%"=="0" @type %FTPLogFile%

@exit /b %FTPRC%


:PROCESSFTPLINE
  @rem look for FTP responses 226 or 250 between get/put and transfer completion
  @if "%FTPPut%"=="true"  if /i "%1 %2"=="ftp put" set RCProcessing=true
  @if "%FTPPut%"=="false" if /i "%1 %2"=="ftp get" set RCProcessing=true
  @if /i "%1"=="ftp:" set RCProcessing=false
  @if "%RCProcessing%"=="true" if /i "%1"=="226" set FTPRC=0
  @if "%RCProcessing%"=="true" if /i "%1"=="250" set FTPRC=0
  @goto :eof


:ARGERROR
  @echo bad argument: %*

  @echo test argument parsing
  @echo FTPServer=%FTPServer%
  @echo FTPPort=%FTPPort%
  @echo FTPUsername=%FTPUsername%
  @echo FTPPassword=%FTPPassword%
  @echo FTPPut=%FTPPut%
  @echo FTPLocalfile=%FTPLocalfile%
  @echo FTPLocalDirectory=%FTPLocalDirectory%
  @echo FTPRemoteFile=%FTPRemotefile%
  @echo FTPRemoteDirectory=%FTPRemoteDirectory%

@echo RC=2
@exit /b 2



