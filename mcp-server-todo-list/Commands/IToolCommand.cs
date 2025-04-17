using System.Text.Json;

namespace mcp_server_todo_list.Commands;

/// <summary>
/// Defines the contract for a command that handles a specific tool request.
/// </summary>
public interface IToolCommand
{
    /// <summary>
    /// Executes the command logic.
    /// </summary>
    /// <param name="arguments">The arguments provided for the tool call.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A string representing the result of the command execution.</returns>
    Task<string> ExecuteAsync(IReadOnlyDictionary<string, JsonElement> arguments, CancellationToken cancellationToken);
}
