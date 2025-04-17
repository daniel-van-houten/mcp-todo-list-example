using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using mcp_server_todo_list.Models; // Add namespace for TodoTask

namespace mcp_server_todo_list.Interfaces;

public interface ITaskRepository
{
    Task<TodoTask?> FindByIdAsync(int taskId, CancellationToken cancellationToken = default); // Return nullable if not found
    Task<List<TodoTask>> GetAllAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(TodoTask task, CancellationToken cancellationToken = default);
    Task DeleteAsync(int taskId, CancellationToken cancellationToken = default);
    Task<int> GetNextIdAsync(CancellationToken cancellationToken = default); // Added for ID generation
}
