using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using mcp_server_todo_list.Interfaces; // For ITaskManager
using System.Collections.Generic; // Add for IReadOnlyDictionary

namespace mcp_server_todo_list.Commands;

/// <summary>
/// Command to handle the 'create_task' tool request.
/// </summary>
public class CreateTaskCommand : ToolCommandBase
{
    public CreateTaskCommand(ITaskManager taskManager) : base(taskManager) { }

    /// <inheritdoc />
    public override async Task<string> ExecuteAsync(IReadOnlyDictionary<string, JsonElement> arguments, CancellationToken cancellationToken)
    {
        var name = GetRequiredStringArgument(arguments, "name");

        var newTask = await _taskManager.CreateTaskAsync(name, cancellationToken);
        return $"Task '{newTask.Name}' created successfully with ID: {newTask.Id}";
    }
}
