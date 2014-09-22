@rem --------------------------------------------------------------------------
@rem JavaMakeTasks.bat - Run compilation tasks for java components
@rem
@rem Usage: javamaketasks task complistFile jdk classpath outputDir [args] 
@rem 
@rem         task	
@rem              the task name (javac, rmic, etc.)
@rem         complistFile
@rem              file containing list of source files to compile
@rem         outputDir
@rem              output directory for compiled targets 
@rem         jdk
@rem              the jdk used for compilation (deprecated -- use dummy value)
@rem         classpath
@rem              the classpath used for compilation
@rem         args
@rem              optional args passed through to compilation tools (not implemented)
@rem
@rem --------------------------------------------------------------------------
@setlocal
@pushd .

@rem set up variables
  @set JMT_Task=%~1
  @set JMT_Complist=%~2
  @set JMT_OutputDir=%~3
  @set JMT_JAVA_HOME=%4
  @set CLASSPATH=%~5
  @set JMT_Args=
:STARTPROCESSARGS
  @if "%6"=="" @goto ENDPROCESSARGS
  @set JMT_Args=%JMT_Args% %6
  @shift
  @goto STARTPROCESSARGS
:ENDPROCESSARGS
  @set PATH=%JMT_JAVA_HOME%\bin;%PATH%

@echo run %JMT_Task% task - output to %3 using %2 as input

@rem run tasks
  @if /i "%JMT_Task%"=="oldjavac" @goto OLDJAVAC
  @if /i "%JMT_Task%"=="javac"    @goto JAVAC
  @if /i "%JMT_Task%"=="rmic"     @goto RMIC
  @goto TASKNOTFOUND
  
  
:JAVAC
  @javac -J-Djavac.pipe.output=true -d "%JMT_OutputDir%" "@%JMT_Complist%" || @copy nul "%JMT_OutputDir%\java.err"
  @goto ENDTASK
  
:RMIC
  @cd \
  @for /F %%i in (%JMT_Complist%) do @call :RUNRMIC %%~pni "%JMT_OutputDir%"
  @goto ENDTASK

:OLDJAVAC
  @for /F %%i in (%JMT_Complist%) do @javac -J-Djavac.pipe.output=true -d "%JMT_OutputDir%" %%i || @copy nul "%JMT_OutputDir%\java.err"
  @goto ENDTASK
  
:TASKNOTFOUND
@echo task "%JMT_Task%" not found
  @copy nul "%JMT_OutputDir%\java.err"
  @goto ENDTASK
  
  
:ENDTASK

@endlocal
@popd
@goto :eof



:RUNRMIC
  @set tmprmic=%1
  @set tmprmic=%tmprmic:~1%
  @set tmprmic=%tmprmic:\=.%
  @rmic -sourcepath "%~2" -d "%~2" %tmprmic% || @copy nul "%~2\java.err"
@goto :eof