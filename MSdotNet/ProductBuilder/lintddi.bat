@echo off
setlocal
rem
rem Mobius has a one-workstation license for PC-Lint, and a one-user lan license for FlexeLint.
rem Gimpel software has confirmed that since we have the FlexeLint lan license, we can install 
rem PC-Lint on the network as long as no more than one user accesses it at a time, and we don't
rem also use FlexeLint on Solaris.
rem

rem
rem Set some environment variables to indicate what product and version we're working with.
rem Maybe in the future we can modify this batch file to accept arguments, and make
rem this batch file more generic.
rem

rem
rem These environment variables are used for sending email
rem

set EmailDistributionDataFile=
set EmailRecipients=
set ProdBld_Path=P:\ProductBuilder
set ProdBld_BuildPath=P:\ProductBuilder
set ProdBld_EmailRecipients=
set ProdBld_Level=DEVELOP
set ProdBld_Name=DDR for the Internet
set ProdBld_Ver=2.1
set ProdBld_Platform=W32

if "%1"=="" goto synopsis

rem
rem See if the quiet-mode argument was specified.  If so, skip preamble.
rem

if "%1"=="q" goto quietmode
if "%1"=="Q" goto quietmode
goto noisymode

:quietmode
set qmode=enabled
shift
goto qmodeset
:noisymode
set qmode=disabled
goto qmodeset

:qmodeset
goto skipmsg

:synopsis
echo This batch file executes PC-Lint against the following source files:
echo.
echo q:\c\*.c
echo q:\cpp\*.cpp
echo u:\root\export\home1\products\%lintprod%\%prdver%.sun\develop\c\*_unix.c
echo u:\root\export\home1\products\%lintprod%\%prdver%.sun\develop\cpp\*_unix.cpp
echo u:\root\export\home1\products\%lintprod%\%prdver%.sun\develop\cpp\*_sol.cpp
echo.
echo The batch file maps the q: drive to c:\products\%lintprod%\%prdver%.w32\develop.
echo It also maps the u: drive to ryesnap2 for blduser, which must have
echo a symbolic link to the the root directory called 'root'.
echo.
echo The batch file cleans up any leftover files from the previous run.
echo Then, it creates two indirection input files with the names of the source
echo files (one for the files on drive q:, and one for the files on drive u:).
echo It then executes PC-Lint against each indirection input file. PC-Lint is
echo configured to report only on errors related to memory leaks.
echo.
echo If you specify q or Q as a parameter, then the batch file will run in
echo "quiet" mode, meaning all prompts and some messages will be suppressed.  
echo If this parameter is specified, it must be the first parameter.
echo .
echo The version, computername, password and user name must be provided as arguments 
echo (in that order) so that the u: drive can be mapped to ryesnap2.  These 
echo parameters must immediately follow the q (or Q) parameter if it is 
echo specified. 
echo.
echo For example:
echo.
echo lintddi q 2.1 \\ryesnap2\blduser bldpswd bldusername
echo.
echo PC-Lint is installed in p:\vendors\lint.  
echo.
echo This batch file makes p:\ProductBuilder\lint the current directory when
echo it runs.
echo.
goto done


:skipmsg
rem
rem Establish the start time of the process, which we'll log later.
rem

set startlinttime=%date% %time%

rem
rem Make p:\ProductBuilder\lint the current drive and directory.
rem

p:
cd \ProductBuilder\lint

echo.
echo Verifying arguments, mapping drives, and cleaning up leftover files...

rem 
rem Need parameters for net use command:
rem %1 is productversion
rem %2 is computername
rem %3 is password
rem %4 is userid
rem
if "%1"=="" goto missingargs
if "%2"=="" goto missingargs
if "%3"=="" goto missingargs
if "%4"=="" goto missingargs

rem
rem set product version
rem
set ProdBld_Ver=%~1

rem
rem These environment variables are used by options_xxx.lnt and later on in
rem this batch file.
rem
set lintprod=ddrint
set prdver=v%ProdBld_Ver:.=_%

echo.
echo Mapping q: drive...
subst q: /D >nul
subst q: c:\products\%lintprod%\%prdver%.w32\network_develop_build >nul
if errorlevel 1 goto mapqerror

set mloop=0
echo.
echo Mapping u: drive: %2 %3 /user:%4 ...
rem 
rem Try 2 times, just in case we hit some kind of timeout condition the first time...
rem
net use u: /DELETE >nul
:maploop
rem net use u: %2 %3 /user:%4 >nul
net use u: %2 %3 /user:%4
if errorlevel 1 goto looptest
goto mapok
:looptest
if %mloop%==1 goto mapuerror
set mloop=1
goto maploop

:mapok
rem
rem If the last run of the batch file detected differences between its output and the output from
rem the run that preceeded it, then both oldleaks%prdver%.txt and leaks%prdver%.txt will exist.  In this case, 
rem we delete oldleaks%prdver%.txt and rename leaks%prdver%.txt as oldleaks%prdver%.txt before proceeding.
rem 

if not exist leaks%prdver%.txt goto makeifile
if exist oldleaks%prdver%.txt del oldleaks%prdver%.txt
ren leaks%prdver%.txt oldleaks%prdver%.txt

:makeifile
echo.
echo Creating indirection file containing names of source files...

if exist sourcewin%prdver%.lnt del sourcewin%prdver%.lnt
dir /b /on p:\products\%lintprod%\%prdver%.w32\develop\cpp\*.cpp >sourcewin%prdver%.lnt
dir /b /on p:\products\%lintprod%\%prdver%.w32\develop\c\*.c >>sourcewin%prdver%.lnt

rem
rem For Solaris, just lint the unix-only files.
rem
if exist sourceunix%prdver%.lnt del sourceunix%prdver%.lnt
dir /b /on u:\root\export\home1\products\%lintprod%\%prdver%.sun\develop\cpp\*_unix.cpp >sourceunix%prdver%.lnt
dir /b /on u:\root\export\home1\products\%lintprod%\%prdver%.sun\develop\cpp\*_sol.cpp >>sourceunix%prdver%.lnt
dir /b /on u:\root\export\home1\products\%lintprod%\%prdver%.sun\develop\c\*_unix.c >>sourceunix%prdver%.lnt

echo.
echo Invoking PC-Lint...
echo.
if %qmode%==enabled goto skippause
pause

:skippause
rem
rem All the .lnt files are located in p:\vendors\lint.  
rem
rem std.win.lnt is the standard lint indirection file for Windows.
rem std.sun.lnt is the standard lint indirection file for Solaris.
rem These files include compiler-specific and library-specific .lnt files, 
rem and also include our custom options.lnt file.  options.lnt provides PC-Lint
rem with the location of our source and headers, and some symbol definitions.
rem
rem options_sun.lnt is the options file for Solaris.
rem options_win.lnt is the options file for Windows.
rem One of these files must be copied to options.lnt before running lint.  
rem
rem memoryleaks.lnt specifies the lint options that result in having only 
rem memory leak errors returned.  Note that PC-Lint sometimes returns non-
rem memoryleak related errors in spite of us telling it not to do so...
rem
rem options_sun.lnt, options_win.lnt, memoryleaks.lnt, std_sun.lnt and
rem std_win.lnt are managed by VMS.
rem

copy p:\vendors\lint\memoryleaks.lnt memoryleaks%prdver%.lnt >nul

rem 
rem Run lint using the std.lnt file and options.lnt file for Windows.
rem

copy p:\vendors\lint\std_win.lnt std%prdver%.lnt >nul
copy p:\vendors\lint\options_win.lnt >nul
"P:\VENDORS\Lint\Lint-nt" +v -width(0,4) -i"P:\VENDORS\Lint" std%prdver%.lnt -os(leaks1%prdver%.txt) memoryleaks%prdver%.lnt sourcewin%prdver%.lnt

rem 
rem Run lint using the std.lnt file and options.lnt file for Solaris.
rem

copy p:\vendors\lint\std_sun.lnt std%prdver%.lnt >nul
copy p:\vendors\lint\options_sun.lnt >nul
"P:\VENDORS\Lint\Lint-nt" +v -width(0,4) -i"P:\VENDORS\Lint" std%prdver%.lnt -os(leaks2%prdver%.txt) memoryleaks%prdver%.lnt sourceunix%prdver%.lnt

rem
rem Concatenate the output files from the lint runs from Windows and Unix
rem

copy leaks1%prdver%.txt + leaks2%prdver%.txt leaks%prdver%.txt >nul
del leaks1%prdver%.txt
del leaks2%prdver%.txt

rem
rem Copy leaks%prdver%.txt to a file with today's date and a .txt extension.  Append
rem the start and end times for the run.
rem

set tempdate=%date:~-10%
set rptfilename=%tempdate:/=%%prdver%.txt
if exist results\%rptfilename% del results\%rptfilename%
copy leaks%prdver%.txt results\%rptfilename%
echo Lint start time: %startlinttime%>>results\%rptfilename%
echo Lint end time:   %date% %time%>>results\%rptfilename%
set startlinttime=

rem
rem We now have to determine if there are any new leaks reported.  Compare against the leaks
rem that were found in the last run, which were saved in oldleaks%prdver%.txt.
rem

if exist oldleaks%prdver%.txt goto compare

rem
rem If oldleaks%prdver%.txt isn't found, then there's nothing to compare.  Rename leaks%prdver%.txt as oldleaks%prdver%.txt,
rem and exit.
rem

ren leaks%prdver%.txt oldleaks%prdver%.txt
goto nodiff

:compare
rem
rem We now have to compare the messages in the old file with those in the new file.  Use grep
rem to create two new output files: one containing the errors and warnings from oldleaks%prdver%.txt, and
rem one containing the errors and warnings from leaks%prdver%.txt.  Then use diff to compare them.  Note
rem that we can't simply compare oldleaks%prdver%.txt with leaks%prdver%.txt, because new files may have been added
rem to the product that will result in differences in those files, but those differences may not
rem be the result of any new errors.
rem
rem The messages that may be found in the lint output file are (see memoryleaks%prdver%.lnt):
rem
rem Error 7: Unable to open include file
rem Error 265: Internal error
rem Warning 423: creation of memory leak in assignment to variable
rem Warning 424: inappropriate deallocation
rem Warning 429: custodial pointer has not been freed or returned
rem Warning 672: possible memory leak in assignment to pointer
rem Warning 673: possible inappropriate deallocation for data
rem
rem First, get errors and warnings from the pre-existing report file from the last run.
rem

g:\develop\devutils\dosapps\gnuutils\grep "Error 7:" oldleaks%prdver%.txt >oldleaks%prdver%.ew
g:\develop\devutils\dosapps\gnuutils\grep "Error 256:" oldleaks%prdver%.txt >>oldleaks%prdver%.ew
g:\develop\devutils\dosapps\gnuutils\grep "Warning 423:" oldleaks%prdver%.txt >>oldleaks%prdver%.ew
g:\develop\devutils\dosapps\gnuutils\grep "Warning 424:" oldleaks%prdver%.txt >>oldleaks%prdver%.ew
g:\develop\devutils\dosapps\gnuutils\grep "Warning 429:" oldleaks%prdver%.txt >>oldleaks%prdver%.ew
g:\develop\devutils\dosapps\gnuutils\grep "Warning 672:" oldleaks%prdver%.txt >>oldleaks%prdver%.ew
g:\develop\devutils\dosapps\gnuutils\grep "Warning 673:" oldleaks%prdver%.txt >>oldleaks%prdver%.ew

rem
rem Now, get errors and warnings from the newly-generated report file from this run.
rem

g:\develop\devutils\dosapps\gnuutils\grep "Error 7:" leaks%prdver%.txt >leaks%prdver%.ew
g:\develop\devutils\dosapps\gnuutils\grep "Error 256:" leaks%prdver%.txt >>leaks%prdver%.ew
g:\develop\devutils\dosapps\gnuutils\grep "Warning 423:" leaks%prdver%.txt >>leaks%prdver%.ew
g:\develop\devutils\dosapps\gnuutils\grep "Warning 424:" leaks%prdver%.txt >>leaks%prdver%.ew
g:\develop\devutils\dosapps\gnuutils\grep "Warning 429:" leaks%prdver%.txt >>leaks%prdver%.ew
g:\develop\devutils\dosapps\gnuutils\grep "Warning 672:" leaks%prdver%.txt >>leaks%prdver%.ew
g:\develop\devutils\dosapps\gnuutils\grep "Warning 673:" leaks%prdver%.txt >>leaks%prdver%.ew

rem
rem Determine if any new errors are showing up in the new leaks%prdver%.ew file.  Do
rem this by checking to see if every line in leaks%prdver%.ew is also in oldleaks%prdver%.ew.
rem Each line that is not found is output to compleaks%prdver%.ew.  When we're done, if
rem compleaks%prdver%.ew does not exist, then there are no new lines and hence no 
rem new errors.
rem

if exist compleaks%prdver%.ew del compleaks%prdver%.ew
for /F "tokens=*" %%i in (leaks%prdver%.ew) do find "%%i" oldleaks%prdver%.ew>nul || echo %%i>>compleaks%prdver%.ew

rem
rem Now, see if compleaks%prdver%.ew exists.  If not, there are no
rem new memory leaks.  Delete oldleaks%prdver%.txt. Then, rename leaks%prdver%.txt as 
rem oldleaks%prdver%.txt.
rem

if exist compleaks%prdver%.ew goto diff
del oldleaks%prdver%.txt
ren leaks%prdver%.txt oldleaks%prdver%.txt
goto nodiff

:nodiff
rem
rem Files are identical...
rem

if exist EmailBody%prdver%.Txt del EmailBody%prdver%.Txt
set EmailBody=Linting of the source code for %ProdBld_Name% %ProdBld_Ver% has detected no new memory leaks.
echo.
echo %EmailBody%
echo %EmailBody%>>EmailBody%prdver%.Txt
goto sendemail

:diff
rem
rem Files are different...
rem

if exist EmailBody.Txt del EmailBody.Txt
set EmailBody1=Linting of the source code for %ProdBld_Name% %ProdBld_Ver% has detected new memory leaks.
set EmailBody2=Compare file://p:\ProductBuilder\lint\oldleaks%prdver%.txt and file://p:\ProductBuilder\lint\leaks%prdver%.txt
set EmailBody3=to determine what new memory leaks have been introduced.
echo.
echo %EmailBody1%
echo %EmailBody2%
echo %EmailBody3%
echo %EmailBody1%>>EmailBody%prdver%.Txt
echo %EmailBody2%>>EmailBody%prdver%.Txt
echo %EmailBody3%>>EmailBody%prdver%.Txt
goto sendemail

:sendemail
rem
rem Set up a lint notification list.  Use the same distribution list used by
rem the product builder.  SetBuildNotificationList will set the EmailRecipients 
rem environment variable, using the environment variables set at the top of this
rem batch file.
rem

rem hack -- use the 2.1 distibution list until we can replace MAPI with SMTP
set tempver=%ProdBld_Ver%
set ProdBld_Ver=2.1
call p:\ProductBuilder\Tools\SetBuildNotificationList.bat
set ProdBld_Ver=%tempver%

rem
rem Send the email.
rem

set EmailSubject=Lint results for %ProdBld_Name% %ProdBld_Ver%
p:\ProductBuilder\Tools\SmtpSendEmail.Exe /S "%EmailSubject%" /B "%ProdBld_BuildPath%\lint\EmailBody%prdver%.Txt" /R "%EmailRecipients%"
goto done

:missingargs
echo.
echo The productversion computername password and username must be specified (in that order)
echo.
goto synopsis

:mapqerror
echo.
echo Unable to map the q: drive with the subst command...
goto done

:mapuerror
echo.
echo Unable to map the u: drive with the net use command...
goto done

:done
rem
rem Before leaving, delete temporary files and unmap u: drive.
rem

net use u: /delete
if exist oldleaks%prdver%.ew del oldleaks%prdver%.ew
if exist leaks%prdver%.ew del leaks%prdver%.ew
if exist compleaks%prdver%.ew del compleaks%prdver%.ew
if exist EmailBody%prdver%.Txt del EmailBody%prdver%.Txt
if exist std%prdver%.lnt del std%prdver%.lnt
if exist options%prdver%.lnt del options%prdver%.lnt
if exist memoryleaks%prdver%.lnt del memoryleaks%prdver%.lnt
if exist sourcewin%prdver%.lnt del sourcewin%prdver%.lnt
if exist sourceunix%prdver%.lnt del sourceunix%prdver%.lnt
