@if exist q:\bat\MediaBuild.bat @goto USEQMEDIABUILD
@rem --------------------------------------------------------------------------
@rem QAMediaBuild.bat - QA specific wrapper for BuildMedia.bat
@rem
@rem usage: qamediabuild "ProductName" ProductVersion ProductPlatform ProductLevel "OutputPath"
@rem
@rem ex: qamediabuild "DDR for the Internet" 2.1 sun prod "L:\DISTRIBU\DDRINT\DISTR2.1"
@rem
@rem note: product name, version, and platform are as in the VMS product window
@rem       and are case-insensitive
@rem
@rem --------------------------------------------------------------------------
@pushd .

@echo.
@echo process command-line arguments:
  @set ProdBld_Name=%~1
  @set ProdBld_Ver=%~2
  @set ProdBld_Platform=%~3
  @set ProductLevel=%~4
  @set OutputPath=%~5
  @rem echo back values
  @set ProdBld_Name
  @set ProdBld_Ver
  @set ProdBld_Platform
  @set ProductLevel
  @set OutputPath

@echo.
@echo derived variables:
  @set ProdBld_NoPauses=false
  @set PcopyPath=p:\products\util\v4.w32\%ProductLevel%\bin
  @set ProdBld_BuildPath=q:
  @set ProdBld_NoPauses
  @set PcopyPath
  @set ProdBld_BuildPath

@echo.
@echo subst mappings:
  @subst

@echo.
@echo call BuildMedia.bat
  @call P:\ProductBuilder\tools\BuildMedia.bat
  
@echo.
@echo QAMediaBuild complete

@echo.
@popd

@goto :eof

:USEQMEDIABUILD
@echo Use q:\bat\MediaBuild.bat instead of QAMediaBuild.bat
