# MCP To-Do List Example

This project demonstrates the use of the Model Context Protocol (MCP) to create a simple To-Do list application managed via a language model. It consists of two .NET console applications: an MCP server that manages tasks and an MCP client that interacts with an AI model (OpenAI) and the server.

## Overview

*   **`mcp-server-todo-list`**: The MCP server application.
    *   Manages To-Do tasks (Create, List, Update Status, Delete).
    *   Stores tasks as YAML files in a specified directory (`mcp_tasks/` by default).
    *   Exposes task management functions as MCP tools over standard input/output (stdio).
    *   Configurable via `appsettings.json`.
*   **`mcp-client-todo-list`**: The MCP client application.
    *   Connects to the `mcp-server-todo-list` via stdio.
    *   Integrates with an OpenAI chat model (configurable, defaults to gpt-4o-mini).
    *   Makes the server's tools available to the AI model.
    *   Provides a command-line interface for users to interact with the AI to manage their To-Do list.
    *   Configurable via `appsettings.json` and user secrets.

## Getting Started

Follow these steps to set up and run the To-Do list application.

### Prerequisites

*   [.NET SDK](https://dotnet.microsoft.com/download) (.NET 9.0 SDK)
*   An [OpenAI API Key](https://platform.openai.com/api-keys)

### Configuration

1.  **Server Configuration (`mcp-server-todo-list/appsettings.json`):**
    *   Verify the `TaskDirectoryPath`. By default, it's set to `../mcp_tasks/` relative to the server's build output directory. You might want to adjust this or ensure the directory exists where the server expects it. 

    ```json
    {
      "TaskDirectoryPath": "../mcp_tasks/" 
    }
    ```

2.  **Client Configuration (`mcp-client-todo-list/appsettings.json`):**
    *   `WindowsServerExePath` & `LinuxMacServerExePath`: **Crucially**, these must point to the correct executable created by the **publish script** (see "Publishing the Server" section above), located in the `todo-mcp-server-build` directory relative to the client project's *runtime* location (typically `bin/Debug/netX.Y`). The application automatically detects the operating system (Windows, macOS, or Linux) and uses the appropriate path at runtime.
        *   Ensure the path specified for `WindowsServerExePath` ends with `.exe`.
        *   Ensure the path specified for `LinuxMacServerExePath` does *not* have an extension.
    ```json
    {
      "Logging": {
        "LogLevel": {
          "Default": "Information"
        }
      },
      "AppSettings": {
        "WindowsServerExePath": "..\\..\\..\\..\\todo-mcp-server-build\\mcp-server-todo-list.exe", 
        "LinuxMacServerExePath": "../../../../todo-mcp-server-build/mcp-server-todo-list", 
        "OpenAIKey": "<Your Key>" // Use .NET User Secrets for production!
      }
    }
    ```
    *   `OpenAIKey`: **IMPORTANT:** Do not commit your API key directly in `appsettings.json`. Use .NET User Secrets:
        *   Navigate to the `mcp-client-todo-list` directory in your terminal.
        *   Run `dotnet user-secrets init`
        *   Run `dotnet user-secrets set AppSettings:OpenAIKey "YOUR_ACTUAL_OPENAI_KEY"`

### Building the Projects

Navigate to the root directory (`mcp-todo-list-example`) in your terminal and run:

```bash
dotnet build mcp-client-todo-list/mcp-client-todo-list.sln
dotnet build mcp-server-todo-list/mcp-server-todo-list.sln
```

### Publishing the Server (Important!)

Before running the client for the first time, or after making changes to the server code, you may want to publish the server project using the provided scripts to ensure it's up-to-date. This creates a self-contained build in the `todo-mcp-server-build` directory, which the client expects to find.

1.  Navigate to the `mcp-server-todo-list` directory.
2.  Run the appropriate script for your operating system:
    *   **Windows:** `.\publish.bat`
    *   **macOS/Linux:** `sh ./publish.sh` (don't forget to grant execute rights on the script using the chmod command)

This ensures the client uses the latest version of the server from the correct location.

### Running the Application

1.  **Ensure Server Paths are Correct:** Verify the `WindowsServerExePath` and `LinuxMacServerExePath` in `mcp-client-todo-list/appsettings.json` point to the actual location of the published server executable relative to the client's runtime directory.
2.  **Run the Client:** Navigate to the `mcp-client-todo-list` directory (or run from the root using the `-p` flag) and execute:

    ```bash
    dotnet run --project mcp-client-todo-list/mcp-client-todo-list.csproj 
    ```
    *   The client application will start.
    *   It will automatically launch the `mcp-server-todo-list` executable as a background process using the path specified in `ServerExePath`.
    *   The client connects to the server via stdio.
    *   You can now interact with the AI in the terminal. Try commands like:
        *   "Add a task to buy milk"
        *   "What are my tasks?"
        *   "Mark the 'buy milk' task as done"
        *   "Delete the task about buying milk"
3.  **Exit:** Type `exit` in the client's prompt to shut down the client. This should also terminate the server process it launched.

## How it Works

The client application uses the `Microsoft.Extensions.AI.Chat` library to communicate with the OpenAI model. It also uses the `ModelContextProtocol.Client` library to establish an MCP connection with the server process launched via stdio.

The client fetches the list of available tools from the server (`ListToolsAsync`) and includes them in the `ChatOptions` sent to the OpenAI model. When the model decides to use a tool (Function Invocation), the client intercepts this, translates it into an MCP `CallTool` request, sends it to the server, receives the result, and sends it back to the model to continue the conversation.

The server application uses the `ModelContextProtocol.Server` library to listen for MCP requests on stdio. It defines handlers for `ListTools` (returning its predefined tool definitions) and `CallTool` (mapping tool names to specific command implementations like `CreateTaskCommand`, `ListTasksCommand`, etc.).
