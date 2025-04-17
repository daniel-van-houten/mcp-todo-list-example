using System.Text.Json;
using ModelContextProtocol.Protocol.Types;

namespace mcp_server_todo_list.Commands
{
    public static class ToolDefinitions
    {
        public static readonly List<Tool> Tools = new()
        {
            new Tool()
            {
                Name = "list_tasks",
                Description = "Lists all current tasks in the Todo list, showing their name, status, and ID.",
                InputSchema = JsonSerializer.Deserialize<JsonElement>("""
                    {
                        "type": "object",
                        "properties": {},
                        "required": []
                    }
                    """)
            },
            new Tool()
            {
                Name = "create_task",
                Description = "Creates a new task in the Todo list.",
                InputSchema = JsonSerializer.Deserialize<JsonElement>("""
                    {
                        "type": "object",
                        "properties": {
                          "name": {
                            "type": "string",
                            "description": "The name or title of the task."
                          }
                        },
                        "required": ["name"]
                    }
                    """)
            },
            new Tool()
            {
                Name = "delete_task",
                Description = "Deletes a task from the Todo list using its ID.",
                InputSchema = JsonSerializer.Deserialize<JsonElement>("""
                    {
                        "type": "object",
                        "properties": {
                          "task_id": {
                            "type": "integer",
                            "description": "The unique ID of the task to delete."
                          }
                        },
                        "required": ["task_id"]
                    }
                    """)
            },
            new Tool()
            {
                Name = "update_task_status",
                Description = "Updates the status of an existing task.",
                InputSchema = JsonSerializer.Deserialize<JsonElement>("""
                    {
                        "type": "object",
                        "properties": {
                          "task_id": {
                            "type": "integer",
                            "description": "The unique ID of the task to update."
                          },
                           "new_status": {
                            "type": "string",
                            "description": "The new status for the task (e.g., 'Pending', 'Completed')."
                          }
                        },
                        "required": ["task_id", "new_status"]
                    }
                    """)
            }
        };
    }
}
