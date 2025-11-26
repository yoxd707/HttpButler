using HttpButler.Attributes;
using HttpButler.TestApi.Dtos;

namespace HttpButler.TestApi.Services;

[HttpButler]
[Route("https://jsonplaceholder.typicode.com/")]
public interface IJsonPlaceHolderTodo
{
    [HttpGet("todos/{todoId}")]
    Task<TodoDto?> GetTodoAsync(int todoId);

    [HttpPost("todos")]
    Task PostTodoAsync([ToBody] TodoDto todo);
}
