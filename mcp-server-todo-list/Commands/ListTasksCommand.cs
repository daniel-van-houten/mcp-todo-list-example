using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using mcp_server_todo_list.Interfaces; // For ITaskManager
using mcp_server_todo_list.Models; // For TodoTask
using System.Collections.Generic; // Add for IReadOnlyDictionary

namespace mcp_server_todo_list.Commands;

/// <summary>
/// Command to handle the 'list_tasks' tool request.
/// </summary>
public class ListTasksCommand : ToolCommandBase
{
    public ListTasksCommand(ITaskManager taskManager) : base(taskManager) { }

    /// <inheritdoc />
    public override async Task<string> ExecuteAsync(IReadOnlyDictionary<string, JsonElement> arguments, CancellationToken cancellationToken)
    {
        var allTasks = await _taskManager.GetAllTasksAsync(cancellationToken);

        if (allTasks.Any())
        {
            var taskDescriptions = 
                allTasks.Select(t => $"ID: {t.Id}, Task Name: {t.Name}, Status:{t.Status}");

            return "Here are the current tasks:\n" + string.Join("\n", taskDescriptions);
        }
        else
        {
            return "The todo list is currently empty.";
        }
    }
}
