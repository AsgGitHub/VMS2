@rem CheckProductBuild.bat - Check if build was successful

@if "%1"=="" goto USAGE

p:\productbuilder\tools2\checkbuild.exe %~1 /f:%~1\build_cfg\build_definition.xml > nul
@exit /b %errorlevel%

:USAGE
@echo.
@echo CheckProductBuild.bat - Usage:
@echo.
@echo     CheckProductBuild product_directory
@echo.
@echo     Ex: CheckProductBuild p:\products\ddrint\v2_1.w32\develop
@echo.
@exit /b 3
