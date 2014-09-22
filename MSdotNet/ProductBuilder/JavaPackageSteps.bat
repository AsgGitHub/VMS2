@rem --------------------------------------------------------------------------
@rem JavaPackageSteps.bat - Run package steps for java components
@rem
@rem Usage: javapackagesteps step inputDir outputDir targetName
@rem 
@rem         step	
@rem              the step name (jar, class, signcode, etc.)
@rem         inputDir
@rem              directory containing compiled classes, resources, etc.
@rem         outputDir
@rem              directory to create target in or copy target to (frequently \bin)
@rem         targetName
@rem              name of the target binary
@rem         jdk
@rem              the jdk used for jarring (deprecated)
@rem
@rem --------------------------------------------------------------------------
@setlocal
@pushd .

@rem set up variables
  @set JPS_Success=true

  @set JPS_Step=%~1
  @set JPS_InputDir=%~2
  @set JPS_OutputDir=%~3
  @set JPS_Target=%4
  @set JPS_JAVA_HOME=%5
  
  @set PATH=%JPS_JAVA_HOME%\bin;%PATH%

@echo run %JPS_Step% step - output %JPS_Target% to %JPS_OutputDir% using %JPS_InputDir% as input
  @if exist %JPS_InputDir%\java.err @set JPS_Success=false
  @if exist %JPS_InputDir%\java.err @echo java.err found -- step %JPS_Step% was not run
  @if exist %JPS_InputDir%\java.err @goto ENDTASK
  @for %%i in (%JPS_InputDir%) do @%%~di
  @cd %JPS_InputDir%

  @if /i "%JPS_Step%"=="jar"        @goto JAR
  @if /i "%JPS_Step%"=="class"      @goto CLASS
  @if /i "%JPS_Step%"=="cab"        @goto CAB
  @if /i "%JPS_Step%"=="eecab"      @goto EECAB
  @if /i "%JPS_Step%"=="jarmaster"  @goto JARMASTER
  @if /i "%JPS_Step%"=="signcode"   @goto SIGNCODE
  @if /i "%JPS_Step%"=="eesigncode" @goto EESIGNCODE
  @if /i "%JPS_Step%"=="signtool"   @goto SIGNTOOL
  @if /i "%JPS_Step%"=="eesigntool" @goto EESIGNTOOL
  @if /i "%JPS_Step%"=="jarsigner"  @goto JARSIGNER
  @goto STEPNOTFOUND
  
  
:JAR
  @jar cf %JPS_OutputDir%\%JPS_Target% -C %JPS_InputDir% . || @set JPS_Success=false
  @goto ENDTASK
  
:CLASS
  @xcopy /f /r /k /y /c  %JPS_InputDir%\*.class %JPS_OutputDir%\ || @set JPS_Success=false
  @goto ENDTASK

:CAB
  @p:\vendors\ms\mssdk\cabarc -s 6144 n %JPS_OutputDir%\%JPS_Target% %JPS_InputDir%\*.* || @set JPS_Success=false
  @goto ENDTASK

:EECAB
  @if not exist q:\txt\EEVersionInfo.txt @set JPS_Success=false
  @if /i not "%JPS_Success%"=="true" @goto ENDTASK
  @md default
  @copy EnterpriseExplorerApplet.class .\default\*.*
  @"p:\vendors\ms\Microsoft SDK for Java 4.0\bin\dubuild" %JPS_OutputDir%\%JPS_Target% . /N "Mobius" /D "Enterprise Explorer" /I *.class /I *.gif /I *.properties /V @q:\txt\EEVersionInfo.txt  || @set JPS_Success=false
  @goto ENDTASK

:JARMASTER
  @call %MJ_ToolsDir%\BuildJarMaster.bat
  @goto ENDTASK
  
:SIGNCODE
  @if exist %Temp%\signcode.log @del /f /q %Temp%\signcode.log
  @cscript %MJ_ToolsDir%\SigningTool.js signcode %JPS_OutputDir%\%JPS_Target% false %Temp%\signcode.log || @set JPS_Success=false
  @type %Temp%\signcode.log
  @if /i not "%JPS_Success%"=="true" @del /f /q %JPS_OutputDir%\%JPS_Target%
  @goto ENDTASK

:EESIGNCODE
  @if exist %Temp%\eesigncode.log @del /f /q %Temp%\eesigncode.log
  @cscript %MJ_ToolsDir%\SigningTool.js signcode %JPS_OutputDir%\%JPS_Target% false %Temp%\eesigncode.log low || @set JPS_Success=false
  @type %Temp%\eesigncode.log
  @if /i not "%JPS_Success%"=="true" @del /f /q %JPS_OutputDir%\%JPS_Target%
  @goto ENDTASK
  
:SIGNTOOL
  @if exist %Temp%\signtool.log @del /f /q %Temp%\signtool.log
  @for %%i in (%JPS_Target%) do set JPS_SigningScript=%%~ni.txt
  @cscript %MJ_ToolsDir%\SigningTool.js signtool %JPS_OutputDir%\%JPS_Target% false %Temp%\signtool.log %JPS_InputDir% %JPS_SigningScript%
  @if %errorlevel% NEQ 0 @set JPS_Success=false
  @if "%JPS_Success%"=="false" @if not "%JPS_Target%"=="" @if exist %JPS_OutputDir%\%JPS_Target% @del /f /q %JPS_OutputDir%\%JPS_Target%
  @type %Temp%\signtool.log
  @goto ENDTASK

:EESIGNTOOL
  @if exist %Temp%\eesigntool.log @del /f /q %Temp%\eesigntool.log
  @set JPS_SigningScript=
  @if exist %JPS_InputDir%\SmartUpdate.js set JPS_SigningScript=%JPS_InputDir%\SmartUpdate.js
  @cscript %MJ_ToolsDir%\SigningTool.js signtool %JPS_OutputDir%\%JPS_Target% false %Temp%\eesigntool.log %JPS_InputDir% %JPS_SigningScript%
  @if %errorlevel% NEQ 0 @set JPS_Success=false
  @if "%JPS_Success%"=="false" @if not "%JPS_Target%"=="" @if exist %JPS_OutputDir%\%JPS_Target% @del /f /q %JPS_OutputDir%\%JPS_Target%
  @type %Temp%\eesigntool.log
  @goto ENDTASK

:JARSIGNER
  @if exist %Temp%\jarsigner.log @del /f /q %Temp%\jarsigner.log
  @cscript %MJ_ToolsDir%\SigningTool.js jarsigner %JPS_OutputDir%\%JPS_Target% false %Temp%\eesigncode.log || @set JPS_Success=false
  @if exist %Temp%\jarsigner.log @type %Temp%\jarsigner.log
  @if /i not "%JPS_Success%"=="true" @del /f /q %JPS_OutputDir%\%JPS_Target%
  @goto ENDTASK
  
:STEPNOTFOUND
  @echo step "%JPS_Step%" not found
  @set JPS_Success=false
  @goto ENDTASK
  
  
:ENDTASK
  @if /i     "%JPS_Success%"=="true" @echo %JPS_Step% succeeded
  @if /i not "%JPS_Success%"=="true" @echo %JPS_Step% failed
  @if /i not "%JPS_Success%"=="true" @copy nul "%JPS_InputDir%\java.err"
    
@endlocal
@popd
