@echo off
dotnet restore --no-cache
IF ERRORLEVEL 1 GOTO error
IF EXIST nuget.config.intern (
    ren nuget.config.intern nuget.config
)
dotnet restore --no-cache
IF ERRORLEVEL 1 GOTO error
dotnet build
IF ERRORLEVEL 1 GOTO error

GOTO end

:error
        echo ❌ Restore fehlgeschlagen. Abbruch.
pause
:end