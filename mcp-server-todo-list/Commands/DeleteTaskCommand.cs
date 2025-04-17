using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using mcp_server_todo_list.Interfaces; // For ITaskManager
using System.Collections.Generic; // Add for IReadOnlyDictionary

namespace mcp_server_todo_list.Commands;

/// <summary>
/// Command to handle the 'delete_task' tool request.
/// </summary>
public class DeleteTaskCommand : ToolCommandBase
{
    public DeleteTaskCommand(ITaskManager taskManager) : base(taskManager) { }

    /// <inheritdoc />
    public override async Task<string> ExecuteAsync(IReadOnlyDictionary<string, JsonElement> arguments, CancellationToken cancellationToken)
    {
        var taskId = GetRequiredIntArgument(arguments, "task_id");

        await _taskManager.DeleteTaskAsync(taskId, cancellationToken);
        return $"Task with ID: {taskId} deleted successfully.";
    }
}
