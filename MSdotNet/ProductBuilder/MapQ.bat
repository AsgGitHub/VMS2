@rem -----------------------------------------------------------------------------------
@rem MapQ.bat 
@rem
@rem Usage: mapq path
@rem
@rem -----------------------------------------------------------------------------------
@pushd .


@set MapQError=True

@echo.
@echo Freeing drive letter Q...
  @C:
  @subst Q: /D
  @if not errorlevel 1 @goto UNMAPCHECK
  @net use Q: /delete
  
:UNMAPCHECK
@rem Check for success
  @if exist Q: goto ERRORFREE
  @echo Q unmapped successfully
  
@echo.
@echo Mapping Q to %~f1
  @subst Q: %~f1
  @if not exist Q: goto ERRORMAP
  @echo Q mapped successfully
  
  @set MapQError=False
  @goto ENDQMAP
  
:ERRORFREE
  @echo Error unmapping Q
  @goto ENDQMAP
  
:ERRORMAP
  @echo Error mapping Q to 
  @goto ENDQMAP

:ENDQMAP

@rem
@rem MapQ.bat complete.
@rem --------------------------------------------------------------------------
@rem

@popd
