@echo off
call restore.cmd
IF ERRORLEVEL 1 GOTO error
start POS_exam_dotnet_seed.slnx
GOTO end

:error
pause
:end