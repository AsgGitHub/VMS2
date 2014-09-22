@rem --------------------------------------------------------------------------
@rem MakeJava.bat - Build Java components
@rem
@rem Usage: buildjava descriptorFileName
@rem 
@rem         descriptor filename
@rem              the component descriptor filename without extension
@rem
@rem --------------------------------------------------------------------------
@echo on
@setlocal
@pushd .
@echo.

@echo Building Java component %1
  @echo.
  @if not defined ProdBld_Path @set ProdBld_Path=P:\ProductBuilder

@echo running java first pass ...
  @echo.
  @attrib -r Q:\class\*.* /S
  @call %ProdBld_Path%\Tools\BuildJava.bat %1 -1 -f Q:\class -c Q:\class_temp -m Q:\msg\java -p Q:\
  @echo.

@echo running java second pass ...
  @echo.
  @if "%1" NEQ "" @if exist Q:\class_temp\%1\nul @rd Q:\class_temp\%1 /s /q
  @md Q:\class_temp\%1
  @call %ProdBld_Path%\Tools\BuildJava.bat %1 -2 -s Q:\class_temp\%1 -c Q:\class_temp -m Q:\msg\java -b Q:\bin -p Q:\
  @echo.

@echo %1 build Complete.
  @echo.
  
@echo Output log ...
  @echo.
  @type Q:\msg\java\%1.msg


@endlocal
@popd