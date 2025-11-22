using HttpButler.TestApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace HttpButler.TestApi.Controllers;

[ApiController]
[Route("[controller]")]
public class MyTodoController : Controller
{
    private readonly IJsonPlaceHolderTodo _jsonPlaceHolderTodoService;

    public MyTodoController(IJsonPlaceHolderTodo jsonPlaceHolderTodoService)
    {
        _jsonPlaceHolderTodoService = jsonPlaceHolderTodoService;
    }

    [HttpGet("/{todoId}")]
    public async Task<IActionResult> GetTodo(int todoId)
    {
        var todo = await _jsonPlaceHolderTodoService.GetTodoAsync(todoId);
        return Ok(todo);
    }
}
