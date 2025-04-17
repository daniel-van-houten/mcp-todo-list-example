using mcp_server_todo_list.Interfaces;

namespace mcp_server_todo_list.Commands
{
    public class ToolCommandFactory
    {
        private readonly ITaskManager _taskManager;

        public ToolCommandFactory(ITaskManager taskManager)
        {
            _taskManager = taskManager;
        }

        public IToolCommand Create(string commandName)
        {
            return commandName switch
            {
                "create_task" => new CreateTaskCommand(_taskManager),
                "delete_task" => new DeleteTaskCommand(_taskManager),
                "update_task_status" => new UpdateTaskStatusCommand(_taskManager),
                "list_tasks" => new ListTasksCommand(_taskManager),
                _ => throw new InvalidOperationException($"Unknown command: {commandName}")
            };
        }
    }
}
