@rem --------------------------------------------------------------------------
@rem RunMediaBuild.bat - Build file for running IPD file/InstallShield.
@rem --------------------------------------------------------------------------
@echo off
setlocal
pushd .
echo.


rem -----------------------------------------------------------
rem Begin Environment Setup

rem Protect against missing command-line arguments
  if /i "%5"=="/p" goto BADPCOPYDIR
  if /i "%6"=="/p" goto BADPCOPYDIR

rem Set JDK path
  set PATH=%PATH%;P:\VENDORS\Sun\JDK130\bin

echo Command line:          %0 %1 %2 %3 %4 %5 %6 %7 %8 %9

rem Set Default Paths
  if not defined ProdBld_Path set ProdBld_Path=P:\ProductBuilder
  if not defined BuildMediaDataFile set BuildMediaDataFile=%ProdBld_Path%\data\MediaBuildDefs.txt

rem Store the arguments.
  set prodName=%~1
  set vmajor=%2
  set vminor=%3
  set platform=%4
  set SourcePath=%~5
  set OutputPath=%~6
  if /i "%7"=="/p" set PcopyPath=%~8

rem Verify the product
  set FullProdName=%prodName% V%vmajor%_%vminor%.%platform%
  set prodValid=0
  for /F "tokens=1 delims==;" %%i in (%BuildMediaDataFile%) do if /i "%%i"=="%FullProdName%" set prodValid=1 
  if %prodValid%==0 goto BADARGUMENTS
  echo FullProdName:          %FullProdName%

rem Get Product Abbreviation
  set ProdAbr=
  for /F "tokens=1,2 delims==;" %%i in (%BuildMediaDataFile%) do if /i "%%i"=="%FullProdName%" set ProdAbr=%%j
  if "%ProdAbr%"==""  goto MISSINGPRODABR
  if "%ProdAbr%"==" " goto MISSINGPRODABR
  echo ProdAbr:               %ProdAbr%
  
rem Get Outpath Abbreviation
  set ProdOutAbr=
  for /F "tokens=1,3 delims==;" %%i in (%BuildMediaDataFile%) do if /i "%%i"=="%FullProdName%" set ProdOutAbr=%%j
  if "%ProdOutAbr%"==""  goto MISSINGPRODOUTABR
  if "%ProdOutAbr%"==" " goto MISSINGPRODOUTABR
  echo ProdOutAbr:            %ProdOutAbr%
  
rem Defaults for optional arguments.
  rem if "%SourcePath%"=="" set SourcePath=P:\PRODUCTS\%ProdAbr%\V%vmajor%_%vminor%.%platform%\prod
  rem if "%OutputPath%"=="" set OutputPath=L:\DISTRIBU\%ProdOutAbr%\DISTR%vmajor%.%vminor%
  if "%PcopyPath%"=="" set PcopyPath=p:\products\util\v4.w32\preprod\bin
  echo Pcopy path:            %PcopyPath%
  echo Source path:           %SourcePath%
  echo Output path:           %OutputPath%

rem Make sure we can find Isprep.Exe and Pcopy.Exe.
  if not exist %PcopyPath%\Isprep.Exe goto MISSINGISPREP
  if not exist %PcopyPath%\Pcopy.Exe  goto MISSINGPCOPY

rem Get Install Shield Version
  set ISVer=
  for /F "tokens=1,4 delims==;" %%i in (%BuildMediaDataFile%) do if /i "%%i"=="%FullProdName%" set ISVer=%%j
  if "%ISVer%"==""  goto NOINSTALLSHIELD
  if "%ISVer%"==" " goto NOINSTALLSHIELD
  echo ISVer:                 %ISVer%

rem Get ReadMe File
  set ReadMeFile=
  for /F "tokens=1,5 delims==;" %%i in (%BuildMediaDataFile%) do if /i "%%i"=="%FullProdName%" set ReadMeFile=%%j
  if "%ReadMeFile%"==""  goto BADREADMEFILE
  if "%ReadMeFile%"==" " goto BADREADMEFILE
  if not exist %SourcePath%\TXT\%ReadMeFile% goto NOREADMEFILE
  echo ReadMeFile:            %ReadMeFile%

rem Get IPD File
  set IPDFile=
  for /F "tokens=1,6 delims==;" %%i in (%BuildMediaDataFile%) do if /i "%%i"=="%FullProdName%" set IPDFile=%%j
  if "%IPDFile%"==""  goto BADIPDFILE
  if "%IPDFile%"==" " goto BADIPDFILE
  if not exist %SourcePath%\IPD\%IPDFILE% goto NOIPDFILE
  echo IPDFile:               %IPDFile%

rem Get ISP File
  set ISPFile=
  for /F "tokens=1,7 delims==;" %%i in (%BuildMediaDataFile%) do if /i "%%i"=="%FullProdName%" set ISPFile=%%j
  echo ISPFile:               %ISPFile%

rem Select a temp directory that's unique to the product, version and platform.
  set TempDirectory=%TEMP%\%ProdAbr%\V%vmajor%_%vminor%.%platform%\%ISPFile:.ISP=%
  echo TempDirectory:         %TempDirectory%

rem End Environment Setup
rem -----------------------------------------------------------


rem -----------------------------------------------------------
rem Begin Media Build

rem delete temp directory
  if exist %TempDirectory% rd %TempDirectory% /s /q

echo.
echo Make and use local copies of Isprep and Pcopy
  copy %PcopyPath%\Isprep.Exe %TEMP%
  copy %PcopyPath%\Pcopy.Exe %TEMP%
  set PcopyPath=%TEMP%

echo.
echo copy IPD file
  copy "%SourcePath%\IPD\%IPDFile%" "%TEMP%\TEMP$.BAT"

rem Execute InstallShield
  if "%ISVer%"=="0" goto NOISHIELD
  if "%ISVer%"=="3" goto ISHIELD3
  if "%ISVer%"=="5" goto ISHIELD5
  goto NOINSTALLSHIELD

:NOISHIELD
  echo.
  echo Run The IPD file without Install Shield
  echo %TEMP%\TEMP$.BAT %SourcePath% %OutputPath% %TempDirectory% /p %PcopyPath%
  call %TEMP%\TEMP$.BAT %SourcePath% %OutputPath% %TempDirectory% /p %PcopyPath%
  goto MEDIABUILT

:ISHIELD3
  echo.
  echo Run InstallShield 3
  if not "%PcopyPath%"=="" echo %TEMP%\TEMP$.BAT %SourcePath% %OutputPath% /p %PcopyPath% /s 1390
  if     "%PcopyPath%"=="" echo %TEMP%\TEMP$.BAT %SourcePath% %OutputPath% /s 1390
  if not "%PcopyPath%"=="" call %TEMP%\TEMP$.BAT %SourcePath% %OutputPath% /p %PcopyPath% /s 1390
  if     "%PcopyPath%"=="" call %TEMP%\TEMP$.BAT %SourcePath% %OutputPath% /s 1390
  goto MEDIABUILT

:ISHIELD5
  echo.
  echo Expand the InstallShield project (ISP file)
  if "%ISPFile%"==""  goto BADISPFILE
  if "%ISPFile%"==" " goto BADISPFILE
  if not exist %SourcePath%\ISP\%ISPFile% goto NOISPFILE
  echo start /w %pcopypath%\isprep.exe /e /i %SourcePath%\ISP\%ISPFile% /p %TempDirectory% /r %SourcePath%\rul\allprod.rul
  start /w %pcopypath%\isprep.exe /e /i %SourcePath%\ISP\%ISPFile% /p %TempDirectory% /r %SourcePath%\rul\allprod.rul
  
  echo.
  echo Run InstallShield 5
  echo %TEMP%\TEMP$.BAT %SourcePath% %OutputPath% %TempDirectory% /p %PcopyPath%
  call %TEMP%\TEMP$.BAT %SourcePath% %OutputPath% %TempDirectory% /p %PcopyPath%
  goto MEDIABUILT

:MEDIABUILT
  del /f /q %TEMP%\TEMP$.BAT
  goto END

rem End Media Build
rem -----------------------------------------------------------


rem -----------------------------------------------------------
rem Begin Error Handlers

:BADARGUMENTS
  echo One or more bad arguments:
  echo.
  echo   productName: %prodName%
  echo   vmajor:      %vmajor%
  echo   vminor:      %vminor%
  echo   platform:    %platform%
  echo   SourcePath:  %SourcePath%
  echo   OutputPath:  %OutputPath%
  goto END

:MISSINGPRODABR
  echo ERROR: Can't find the product abbreviation.
  goto END
  
:MISSINGPRODOUTABR
  echo ERROR: Can't find the product output path abbreviation.
  goto END

:BADREADMEFILE
  echo ERROR: The name of the ReadMe.Txt file (e.g. DDRJSPW32.TXT) could not be determined.
  goto END
  
:NOREADMEFILE
  echo ERROR: Can't find the file %SourcePath%\TXT\%ReadMeFile% to verify the product.
  goto END
  
:BADIPDFILE
  echo ERROR: The name of the product IPD file (e.g. DDRJSPW32.IPD) could not be determined.
  goto END
  
:NOIPDFILE
  echo ERROR: Can't find the file %SourcePath%\IPD\%IPDFile%.
  goto END

:BADISPFILE
  echo ERROR: The name of the product ISP file could not be determined.
  goto END
  
:NOISPFILE
  echo ERROR: Can't find %SourcePath%\ISP\%ISPFile%.
  goto END

:BADPCOPYDIR
  echo ERROR: You MUST specify both the input and output directories if you 
  echo        want to specify the Pcopy/Isprep directory using the /p switch.
  goto END

:MISSINGISPREP
  echo ERROR: Can't find %PcopyPath%\Isprep.Exe.
  goto END

:MISSINGPCOPY
  echo ERROR: Can't find %PcopyPath%\Pcopy.exe.
  goto END

:NOINSTALLSHIELD
  echo ERROR: Can't determine what version of InstallShield to use.
  goto END

rem End Error Handlers
rem -----------------------------------------------------------

:END

echo.
popd
endlocal
echo on