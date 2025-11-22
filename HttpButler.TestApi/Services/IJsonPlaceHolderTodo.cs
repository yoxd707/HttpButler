using HttpButler.Attributes;

namespace HttpButler.TestApi.Services;

public class Todo
{
    public int UserId { get; set; }
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool Completed { get; set; }
}

[HttpButler]
[Route("https://jsonplaceholder.typicode.com/")]
public interface IJsonPlaceHolderTodo
{
    [HttpGet("todos/{todoId}")]
    Task<Todo?> GetTodoAsync(int todoId);

    [HttpPost("todos")]
    Task PostTodoAsync([ToBody]Todo todo);
}
