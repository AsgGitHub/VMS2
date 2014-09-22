@rem --------------------------------------------------------------------------
@rem CallVcVars32.bat - Set up the MSVC environment. --------------------------
@rem   Since Mobius has installed MSVC in different directories we try them 
@rem   all.  On some Win2K machines the "MSDevDir" variable is already set so
@rem   we use MSVCDir.
@rem --------------------------------------------------------------------------
@rem retain echo state from calling batch file
@rem no localization of environment variables
@pushd .
@echo.

@rem Setting up the MSVC environment...
  @if NOT "%MSVCDir%" == "" goto VCVARSARESET
  @if exist "C:\program files\vstudio\vc98\bin\vcvars32.bat" call "C:\program files\vstudio\vc98\bin\vcvars32.bat"
  @if exist "C:\Program Files\Microsoft Visual Studio\Common\MSDev98\vcvars32.bat" call "C:\Program Files\Microsoft Visual Studio\Common\MSDev98\vcvars32.bat"
  @if exist "C:\Program Files\Microsoft Visual Studio\VC98\Bin\vcvars32.bat" call "C:\Program Files\Microsoft Visual Studio\VC98\Bin\vcvars32.bat"

:VCVARSARESET

@echo.
@popd
