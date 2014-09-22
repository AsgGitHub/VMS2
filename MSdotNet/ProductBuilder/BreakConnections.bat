@REM --------------------------------------------------------------------------
@REM BreakConnections.Bat - Break any file connections in the dir passed as %1.
@REM --------------------------------------------------------------------------
@echo on
@setlocal
@pushd .

@rem Don't know how to do this for a Windows drive, so do nothing...
  @if exist OpenFile.log @del /f /q OpenFile.log

:END
@popd
@endlocal

