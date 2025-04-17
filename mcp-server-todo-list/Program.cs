﻿﻿﻿﻿﻿﻿﻿﻿using ModelContextProtocol.Protocol.Transport;
using ModelContextProtocol.Protocol.Types;
using ModelContextProtocol.Server;
using mcp_server_todo_list.Interfaces;
using mcp_server_todo_list.Managers;
using mcp_server_todo_list.Repositories;
using mcp_server_todo_list.Commands;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

// --- Constants ---
const string ServerName = "TodoListServer";
const string ServerVersion = "1.0.0";

// --- Configuration ---
IConfiguration config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

string taskDirectoryPath = 
    config["TaskDirectoryPath"] 
        ?? throw new InvalidOperationException("TaskDirectoryPath not found in configuration.");

// --- Dependencies ---
ITaskRepository taskRepository = new YamlFileTaskRepository(taskDirectoryPath);
ITaskManager taskManager = new FileSystemTaskManager(taskRepository);

// --- Command Factory Registration ---
var toolCommandFactory = new ToolCommandFactory(taskManager);

// --- Test Mode: Run Command Directly if Specified ---
if (args.Length >= 2 && args[0] == "--run-command")
{
    string commandName = args[1];
    var commandArgs = new Dictionary<string, JsonElement>();

    // Optionally support passing JSON arguments as a 3rd argument
    if (args.Length >= 3)
    {
        try
        {
            commandArgs = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(args[2]) ?? new();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to parse command arguments: {ex.Message}");
            Environment.Exit(1);
        }
    }

    try
    {
        var command = toolCommandFactory.Create(commandName);
        var resultText = await command.ExecuteAsync(commandArgs, CancellationToken.None);
        Console.WriteLine(resultText);
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Error running command '{commandName}': {ex.GetType().Name} - {ex.Message}");
        Console.Error.WriteLine(ex.StackTrace);
        Environment.Exit(1);
    }
    return;
}

// --- Server Setup ---
McpServerOptions options = new()
{
    ServerInfo = new Implementation() { Name = ServerName, Version = ServerVersion },
    Capabilities = new ServerCapabilities()
    {
        // --- Tool Capabilities ---
        Tools = new ToolsCapability()
        {
            // Reference the static methods defined below
            ListToolsHandler = (context, ct) => new ValueTask<ListToolsResult>(HandleListTools(context, taskManager, ct)),
            // Use the factory to create commands
            CallToolHandler = (context, ct) => HandleCallToolAsync(context, toolCommandFactory, ct),
        },
    },
};

// --- Create and Run the Server ---
try
{
    await using IMcpServer server = McpServerFactory.Create(new StdioServerTransport(ServerName), options);
    await server.RunAsync();
}
catch (Exception ex)
{
    Console.Error.WriteLine($"FATAL SERVER ERROR: {ex.GetType().Name} - {ex.Message}");
    Console.Error.WriteLine(ex.StackTrace);
}
finally
{
     Console.WriteLine($"Server finished.");
}

// --- Tool Handler Implementations ---
static ListToolsResult HandleListTools(RequestContext<ListToolsRequestParams> context, ITaskManager taskManager, CancellationToken cancellationToken)
{
    return new ListToolsResult()
    {
        Tools = ToolDefinitions.Tools
    };
}

static async ValueTask<CallToolResponse> HandleCallToolAsync(
    RequestContext<CallToolRequestParams> context,
    ToolCommandFactory toolCommandFactory,
    CancellationToken cancellationToken)
{
    string resultText = string.Empty;
    var responseContent = new List<Content>();
    var toolName = context.Params?.Name ?? "unknown";

    try
    {
        if (context.Params == null)
        {
            throw new ArgumentNullException(nameof(context.Params), "Tool call parameters are missing.");
        }
        if (context.Params.Arguments == null)
        {
            throw new ArgumentNullException(nameof(context.Params.Arguments), "Tool call arguments are missing.");
        }

        var command = toolCommandFactory.Create(toolName);
        resultText = await command.ExecuteAsync(context.Params.Arguments, cancellationToken);
    }
    catch (ArgumentException argEx)
    {
        Console.Error.WriteLine($"Argument Error calling tool '{toolName}': {argEx.Message}");
        resultText = $"Argument Error: {argEx.Message}";
    }
    catch (FileNotFoundException fnfEx) // Catch task not found errors
    {
        Console.Error.WriteLine($"Not Found Error calling tool '{toolName}': {fnfEx.Message}");
        resultText = $"Error: {fnfEx.Message}";
    }
    catch (InvalidDataException dataEx) // Catch data corruption errors
    {
        Console.Error.WriteLine($"Data Error calling tool '{toolName}': {dataEx.Message}");
        resultText = $"Data Error: {dataEx.Message}";
    }
    catch (InvalidOperationException opEx) // Catch other operational errors (e.g., unknown tool)
    {
        Console.Error.WriteLine($"Operation Error calling tool '{toolName}': {opEx.Message}");
        resultText = $"Operation Error: {opEx.Message}";
    }
    catch (Exception ex) // Catch any other unexpected exceptions
    {
        Console.Error.WriteLine($"Unexpected Error calling tool '{toolName}': {ex.GetType().Name} - {ex.Message}");
        Console.Error.WriteLine(ex.StackTrace);
        resultText = $"An unexpected error occurred while executing tool '{toolName}'.";
    }

    if (!string.IsNullOrEmpty(resultText))
    {
        responseContent.Add(new Content() { Text = resultText, Type = "text" });
    }

    // Ensure there's always some content to return
    if (responseContent.Count == 0)
    {
        responseContent.Add(new Content() { Text = $"Tool '{toolName}' execution finished.", Type = "text" });
    }

    return new CallToolResponse() { Content = responseContent };
}
