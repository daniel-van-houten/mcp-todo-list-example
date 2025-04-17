namespace mcp_server_todo_list.Models;

// Represents a single task item
public record TodoTask(int Id = 0, string Name = null!, string Status = "Pending")
{
    // Default constructor for deserialization
    public TodoTask() : this(0, string.Empty, string.Empty) { }
};
