# MCP To-Do List Example

A simple To-Do list application demonstrating the Model Context Protocol (MCP) with OpenAI. It includes two .NET console apps:

- **Server (`mcp-server-todo-list`)**: Manages tasks (create, list, update status, delete) stored as YAML (`mcp_tasks/` by default) and exposes MCP tools over stdio.
- **Client (`mcp-client-todo-list`)**: Integrates with an OpenAI chat model, connects to the server via stdio, and provides an AI-driven CLI.

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- An [OpenAI API key](https://platform.openai.com/api-keys)

## Build & Publish

From the repository root:

1. Build both projects:
   ```bash
   dotnet build mcp-server-todo-list/mcp-server-todo-list.sln
   dotnet build mcp-client-todo-list/mcp-client-todo-list.sln
   ```
2. Publish the server to `mcp-server-todo-list/build`:
   ```bash
   cd mcp-server-todo-list
   # Windows:
   .\\publish.bat
   # macOS/Linux:
   sh ./publish.sh
   ```

## Configuration

### Server

Optional: Edit `mcp-server-todo-list/appsettings.json` to set the task storage path:
```json
{
  "TaskDirectoryPath": "../mcp_tasks/"
}
```

### Client

Update `mcp-client-todo-list/appsettings.json` with the published server paths:
```json
{
  "AppSettings": {
    "WindowsServerExePath": "..\\..\\..\\..\\mcp-server-todo-list\\build\\mcp-server-todo-list.exe",
    "LinuxMacServerExePath": "../../../../mcp-server-todo-list/build/mcp-server-todo-list",
    "OpenAIKey": "<Your Key>"
  }
}
```
Use [.NET User Secrets](https://docs.microsoft.com/dotnet/core/extensions/user-secrets) for `OpenAIKey`:
```bash
cd mcp-client-todo-list
dotnet user-secrets init
dotnet user-secrets set AppSettings:OpenAIKey "YOUR_ACTUAL_OPENAI_KEY"
```

## Run

From the root or client folder:
```bash
dotnet run --project mcp-client-todo-list/mcp-client-todo-list.csproj
```
The client will launch the server, connect via stdio, and start the AI-driven CLI. Try commands like:
- "Add a task to buy milk"
- "What are my tasks?"
- "Mark the 'buy milk' task as done"
- "Delete the 'buy milk' task"

Type `exit` to quit (this also stops the server).

## How It Works

- **Client**: Uses `Microsoft.Extensions.AI.Chat` and `ModelContextProtocol.Client` to send messages to OpenAI and invoke MCP tools.
- **Server**: Uses `ModelContextProtocol.Server` to handle `ListTools` and `CallTool` over stdio, mapping tool names to commands (e.g., `CreateTaskCommand`).