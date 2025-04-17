﻿﻿using Microsoft.Extensions.AI;
﻿using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;
using OpenAI;
using OpenAI.Models;
using System.Runtime.InteropServices; // Added for OS detection

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Read configuration values
var windowsPath = builder.Configuration["AppSettings:WindowsServerExePath"];
var linuxMacPath = builder.Configuration["AppSettings:LinuxMacServerExePath"];
var openAIKey = builder.Configuration["AppSettings:OpenAIKey"]
                ?? throw new InvalidOperationException("AppSettings:OpenAIKey not found in configuration.");

// Determine the correct server path based on the OS
string serverExePath;
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    serverExePath = windowsPath ?? throw new InvalidOperationException("AppSettings:WindowsServerExePath not found or is null in configuration for Windows OS.");
}
else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
{
    serverExePath = linuxMacPath ?? throw new InvalidOperationException("AppSettings:LinuxMacServerExePath not found or is null in configuration for Linux/macOS.");
}
else
{
    throw new PlatformNotSupportedException("Operating system not supported for determining server path.");
}


var chatClient =
    new OpenAIClient(openAIKey)
        .AsChatClient("o4-mini")
        .AsBuilder()
        .UseFunctionInvocation()
        .Build();

var clientTransport = new StdioClientTransport(new StdioClientTransportOptions
{
    Name = "To-Do List MCP Server",
    Command = serverExePath,
    Arguments = Array.Empty<string>(),
});

await using var mcpClient = await McpClientFactory.CreateAsync(clientTransport);

var tools = await mcpClient.ListToolsAsync();

var options = new ChatOptions
{
    MaxOutputTokens = 4096,
    Tools = [.. tools]
};

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("To-Do List MCP Client Started!");
Console.ResetColor();

var messages = new List<ChatMessage>
{
    new (ChatRole.System, 
    @"- You're a friendly, casual, and positive assistant whose always willing to help out and give your opinion based on prior conversation about what to do next. 
      - Don't mention the tools you're using to the user, just use them.
      - If there is an error, tell the user that you are unable to fulfill their request and why based on the error message details.
      - You're Gen Z, so use Gen Z slang and emojis.
      - Finally, always be on the lookout for opportunities to use the tools available to you, and suggest the actions they represent to the user if you think they would be helpful.
    ")
};

PromptForInput();
while (Console.ReadLine() is string query && !"exit".Equals(query, StringComparison.OrdinalIgnoreCase))
{
    // Handle Empty Inputs
    if (string.IsNullOrWhiteSpace(query))
    {
        PromptForInput();
        continue;
    }
    
    try
    {
        messages.Add(new (ChatRole.User, query));
            
        var chat = await chatClient.GetResponseAsync(messages, options);
        Console.WriteLine(chat.Messages.Last().Text);

        // Update conversation with last assistant message
        // This will be a mixed bag of text and function calls
        messages.Add(chat.Messages.Where(m => m.Role == ChatRole.Assistant).Last());
    }
    catch (Exception ex)
    {
        Console.WriteLine("Oh Noooz!");
        Console.WriteLine(ex.Message);
    }
    Console.WriteLine();
    PromptForInput();
}

static void PromptForInput()
{
    Console.WriteLine("Enter a request (or 'exit' to quit):");
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.Write("> ");
    Console.ResetColor();
}
