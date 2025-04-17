using System.Text.Json;

namespace mcp_server_todo_list.Commands
{
    /// <summary>
    /// Abstract base class for tool commands, providing common argument validation helpers.
    /// </summary>
    public abstract class ToolCommandBase : IToolCommand
    {
        protected readonly Interfaces.ITaskManager _taskManager;

        protected ToolCommandBase(Interfaces.ITaskManager taskManager)
        {
            _taskManager = taskManager;
        }

        /// <summary>
        /// Extracts and validates a required string argument.
        /// </summary>
        protected static string GetRequiredStringArgument(IReadOnlyDictionary<string, JsonElement> arguments, string key)
        {
            if (!arguments.TryGetValue(key, out var element) ||
                element.ValueKind != JsonValueKind.String ||
                string.IsNullOrWhiteSpace(element.GetString()))
            {
                throw new ArgumentException($"Missing or invalid argument '{key}' (string) is required.");
            }
            return element.GetString()!;
        }

        /// <summary>
        /// Extracts and validates a required integer argument.
        /// </summary>
        protected static int GetRequiredIntArgument(IReadOnlyDictionary<string, JsonElement> arguments, string key)
        {
            if (!arguments.TryGetValue(key, out var element) ||
                !element.TryGetInt32(out int value))
            {
                throw new ArgumentException($"Missing or invalid argument '{key}' (integer) is required.");
            }
            return value;
        }

        // IToolCommand requires this method to be implemented by derived classes.
        public abstract Task<string> ExecuteAsync(IReadOnlyDictionary<string, JsonElement> arguments, CancellationToken cancellationToken);
    }
}
