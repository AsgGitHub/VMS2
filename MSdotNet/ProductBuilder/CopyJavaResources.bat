@rem --------------------------------------------------------------------------
@rem CopyJavaResources.bat - copy resources into package paths
@rem
@rem Usage: copyjavaresources source classRoot package 
@rem 
@rem         source	
@rem              path to source file
@rem         classRoot
@rem              root of class output directory
@rem         package
@rem              package directory relative to classRoot
@rem
@rem --------------------------------------------------------------------------
@setlocal
@pushd .

@xcopy /f /r /k /y /c %~1 %~2\%~3\ || @copy nul %~2\java.err

@endlocal
@popd

