using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using mcp_server_todo_list.Interfaces;
using mcp_server_todo_list.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace mcp_server_todo_list.Repositories;

public class YamlFileTaskRepository : ITaskRepository
{
    private readonly string _taskDirectoryPath;
    private static readonly ISerializer YamlSerializer = new SerializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .Build();
    private static readonly IDeserializer YamlDeserializer = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    // Constructor receives the path to the task directory
    public YamlFileTaskRepository(string taskDirectoryPath)
    {
        if (string.IsNullOrWhiteSpace(taskDirectoryPath))
        {
            throw new ArgumentException("Task directory path cannot be empty.", nameof(taskDirectoryPath));
        }
        _taskDirectoryPath = Path.GetFullPath(taskDirectoryPath);

        // Ensure the directory exists upon instantiation
        if (!Directory.Exists(_taskDirectoryPath))
        {
            try
            {
                Directory.CreateDirectory(_taskDirectoryPath);
            }
            catch (Exception ex)
            {
                // Wrap the exception for clarity
                throw new InvalidOperationException($"Failed to create task directory: {_taskDirectoryPath}", ex);
            }
        }
    }

    // Generates the full file path for a given task ID
    private string GetTaskFilePath(int taskId)
    {
        // Basic validation for Task ID
        if (taskId <= 0)
        {
             throw new ArgumentException($"Task ID must be a positive integer.", nameof(taskId));
        }
        return Path.Combine(_taskDirectoryPath, $"{taskId}.yaml");
    }

    public async Task<TodoTask?> FindByIdAsync(int taskId, CancellationToken cancellationToken = default)
    {
        var filePath = GetTaskFilePath(taskId);

        if (!File.Exists(filePath))
        {
            return null; // Task not found
        }

        try
        {
            var yamlContent = await File.ReadAllTextAsync(filePath, cancellationToken);
            var task = YamlDeserializer.Deserialize<TodoTask>(yamlContent);

            // Consistency check
            if (task == null || task.Id != taskId)
            {
                // Log warning or throw specific exception for corrupted data?
                // For now, treat as not found/invalid.
                // Consider adding logging here if ILogger was injected.
                throw new InvalidDataException($"Data mismatch in file '{filePath}'. Expected Task ID '{taskId}'.");
            }
            return task;
        }
        catch (YamlDotNet.Core.YamlException yamlEx)
        {
            // Wrap YAML parsing errors
            throw new InvalidDataException($"Failed to parse YAML data for task '{taskId}'. File might be corrupted.", yamlEx);
        }
        catch (IOException ioEx)
        {
            // Wrap IO errors
            throw new InvalidOperationException($"An error occurred while reading task file '{filePath}'.", ioEx);
        }
    }

    public async Task<List<TodoTask>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var tasks = new List<TodoTask>();
        string[] taskFiles;

        try
        {
            taskFiles = Directory.GetFiles(_taskDirectoryPath, "*.yaml");
        }
        catch (DirectoryNotFoundException)
        {
            // If the directory got deleted after initialization, return empty or throw?
            // Returning empty list for now.
            return tasks;
        }
        catch (IOException ioEx)
        {
             throw new InvalidOperationException($"An error occurred while listing task files in '{_taskDirectoryPath}'.", ioEx);
        }


        foreach (var filePath in taskFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                // Extract ID from filename for consistency check
                if (!int.TryParse(Path.GetFileNameWithoutExtension(filePath), out var fileId) || fileId <= 0)
                {
                    // Log warning: Invalid filename format
                    continue; // Skip files not matching the ID pattern
                }

                var yamlContent = await File.ReadAllTextAsync(filePath, cancellationToken);
                var task = YamlDeserializer.Deserialize<TodoTask>(yamlContent);

                if (task != null && task.Id == fileId) // Check ID consistency
                {
                    tasks.Add(task);
                }
                else
                {
                    // Log warning: Mismatch between filename ID and content ID, or invalid content
                }
            }
            catch (YamlDotNet.Core.YamlException)
            {
                // Log warning: Skipping corrupted file
            }
            catch (IOException)
            {
                // Log warning: Skipping file due to read error
            }
            // Ignore other exceptions for individual files to be robust? Or let them bubble up?
            // Current approach: skip problematic files.
        }

        return tasks;
    }

     public async Task SaveAsync(TodoTask task, CancellationToken cancellationToken = default)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));
        if (task.Id <= 0) throw new ArgumentException("Task must have a positive ID to be saved.", nameof(task));

        var filePath = GetTaskFilePath(task.Id);
        var yamlContent = YamlSerializer.Serialize(task);

        try
        {
            await File.WriteAllTextAsync(filePath, yamlContent, cancellationToken);
        }
        catch (IOException ioEx)
        {
            // Wrap IO errors
            throw new InvalidOperationException($"An error occurred while saving task file '{filePath}'.", ioEx);
        }
    }

    public Task DeleteAsync(int taskId, CancellationToken cancellationToken = default)
    {
        var filePath = GetTaskFilePath(taskId);

        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            // If file doesn't exist, do nothing (idempotent delete)
            return Task.CompletedTask;
        }
        catch (IOException ioEx)
        {
            // Wrap IO errors
            throw new InvalidOperationException($"An error occurred while deleting task file '{filePath}'.", ioEx);
        }
    }

    public async Task<int> GetNextIdAsync(CancellationToken cancellationToken = default)
    {
        // This implementation reads all files just to get IDs.
        // Could be optimized if performance becomes an issue (e.g., separate ID counter file).
        var allTasks = await GetAllAsync(cancellationToken); // Reuse GetAllAsync logic
        return allTasks.Any() ? allTasks.Max(t => t.Id) + 1 : 1; // Start from 1 if empty
    }
}
