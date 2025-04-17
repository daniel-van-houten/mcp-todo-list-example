using System;
using System.Collections.Generic;
using System.IO; // Required for FileNotFoundException
using System.Threading;
using System.Threading.Tasks;
using mcp_server_todo_list.Interfaces;
using mcp_server_todo_list.Models;

namespace mcp_server_todo_list.Managers;

public class FileSystemTaskManager : ITaskManager
{
    private readonly ITaskRepository _repository;

    // Inject the repository dependency
    public FileSystemTaskManager(ITaskRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public Task<List<TodoTask>> GetAllTasksAsync(CancellationToken cancellationToken = default)
    {
        // Directly pass through to the repository
        return _repository.GetAllAsync(cancellationToken);
    }

    public async Task<TodoTask> GetTaskAsync(int taskId, CancellationToken cancellationToken = default)
    {
        if (taskId <= 0)
        {
            throw new ArgumentException("Task ID must be a positive integer.", nameof(taskId));
        }

        var task = await _repository.FindByIdAsync(taskId, cancellationToken);

        // Throw a specific exception if the task is not found
        return task ?? throw new FileNotFoundException($"Task with ID '{taskId}' not found.");
    }

    public async Task<TodoTask> CreateTaskAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Task name cannot be empty.", nameof(name));
        }

        // Get the next available ID from the repository
        int nextId = await _repository.GetNextIdAsync(cancellationToken);

        var newTask = new TodoTask(
            Id: nextId,
            Name: name
            // Status defaults to "Pending" in the record definition
        );

        // Save the new task using the repository
        await _repository.SaveAsync(newTask, cancellationToken);

        return newTask;
    }

    public async Task<TodoTask> UpdateTaskStatusAsync(int taskId, string newStatus, CancellationToken cancellationToken = default)
    {
         if (taskId <= 0)
        {
            throw new ArgumentException("Task ID must be a positive integer.", nameof(taskId));
        }
        if (string.IsNullOrWhiteSpace(newStatus))
        {
            throw new ArgumentException("New status cannot be empty.", nameof(newStatus));
        }

        // First, retrieve the existing task to ensure it exists
        var existingTask = await GetTaskAsync(taskId, cancellationToken); // Reuses the not-found check from GetTaskAsync

        // Create the updated task record
        var updatedTask = existingTask with { Status = newStatus };

        // Save the updated task
        await _repository.SaveAsync(updatedTask, cancellationToken);

        return updatedTask;
    }

    public async Task DeleteTaskAsync(int taskId, CancellationToken cancellationToken = default)
    {
        if (taskId <= 0)
        {
            throw new ArgumentException("Task ID must be a positive integer.", nameof(taskId));
        }

        // Check if task exists before attempting delete?
        // The repository's DeleteAsync is idempotent, so checking might be redundant
        // unless we want to throw a specific "NotFound" exception here too.
        // For now, let's rely on the repository's behavior.
        // var existingTask = await GetTaskAsync(taskId, cancellationToken); // Uncomment if explicit check is desired

        await _repository.DeleteAsync(taskId, cancellationToken);
    }
}
