@ECHO OFF
@REM -----------------------------------------------------------------------
@REM This batch files generates an error log from the Delphi message files.
@REM -----------------------------------------------------------------------

Q:
cd \
copy P:\ProductBuilder\Tools\Awk\MakeLog.AWK Q:\
G:\Develop\DevUtils\DOSAPPS\AWK\GAWK -f MakeLog.AWK %1
attrib -r Q:\MakeLog.AWK
Erase /Q /F Q:\MakeLog.AWK