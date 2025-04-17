#!/bin/bash
echo "Publishing To-Do List MCP Server..."
dotnet publish -c Release -o build
if [ $? -ne 0 ]; then
    echo "Publish failed."
    exit 1
fi
echo "Publish complete. Output located in build"
exit 0
