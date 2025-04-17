@echo off
echo Publishing To-Do List MCP Server...
dotnet publish -c Release -o ..\todo-mcp-server-build
if %errorlevel% neq 0 (
    echo Publish failed.
    pause
    exit /b %errorlevel%
)
echo Publish complete. Output located in ..\todo-mcp-server-build
pause
