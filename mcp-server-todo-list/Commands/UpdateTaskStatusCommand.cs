using System;
using System.Text.Json;
using mcp_server_todo_list.Interfaces; 

namespace mcp_server_todo_list.Commands;

/// <summary>
/// Command to handle the 'update_task_status' tool request.
/// </summary>
public class UpdateTaskStatusCommand : ToolCommandBase
{
    public UpdateTaskStatusCommand(ITaskManager taskManager) : base(taskManager) { }

    /// <inheritdoc />
    public override async Task<string> ExecuteAsync(IReadOnlyDictionary<string, JsonElement> arguments, CancellationToken cancellationToken)
    {
        var taskId = GetRequiredIntArgument(arguments, "task_id");
        var newStatus = GetRequiredStringArgument(arguments, "new_status");

        await _taskManager.UpdateTaskStatusAsync(taskId, newStatus, cancellationToken);
        return $"Task with ID: {taskId} status updated to '{newStatus}'.";
    }
}
