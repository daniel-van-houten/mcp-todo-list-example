using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using mcp_server_todo_list.Models; // Add namespace for TodoTask

namespace mcp_server_todo_list.Interfaces;

public interface ITaskManager
{
    Task<List<TodoTask>> GetAllTasksAsync(CancellationToken cancellationToken = default);
    Task<TodoTask> GetTaskAsync(int taskId, CancellationToken cancellationToken = default);
    Task<TodoTask> CreateTaskAsync(string name, CancellationToken cancellationToken = default);
    Task<TodoTask> UpdateTaskStatusAsync(int taskId, string newStatus, CancellationToken cancellationToken = default);
    Task DeleteTaskAsync(int taskId, CancellationToken cancellationToken = default);
}
