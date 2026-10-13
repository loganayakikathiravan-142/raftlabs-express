@echo off
title RaftLabs Express - Frontend Dev Server
echo ========================================================
echo Starting RaftLabs Express React Frontend (Vite + TypeScript)
echo ========================================================

:: Ensure Visual Studio Node.js and Git are in PATH if not already in system PATH
set "PATH=C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Microsoft\VisualStudio\NodeJs;C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\CommonExtensions\Microsoft\TeamFoundation\Team Explorer\Git\cmd;%PATH%"

cd /d "%~dp0frontend"

echo Checking dependencies...
if not exist "node_modules" (
    echo Installing npm dependencies...
    call npm install
)

echo.
echo Launching Vite development server at http://localhost:5173 ...
echo.
call npm run dev
pause
