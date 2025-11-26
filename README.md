# HttpButler

HttpButler es una librería ligera de C# que simplifica la creación de clientes HTTP tipados generando automáticamente implementaciones de interfaces mediante source generators. Permite definir contratos HTTP declarativamente sin necesidad de escribir código repetitivo.

## Características

- **Source Generators**: Genera automáticamente implementaciones de interfaces marcadas con `[HttpButler]`
- **Atributos declarativos**: Define rutas y métodos HTTP usando atributos
- **Inyección de dependencias**: Integración perfecta con `IServiceCollection`
- **Tipado fuerte**: Aprovecha el sistema de tipos de C# para validación en tiempo de compilación
- **Parámetros flexibles**: Soporte para parámetros en ruta, query y body

## Instalación

Agrega el paquete HttpButler a tu proyecto:

```bash
dotnet add package HttpButler
```

También necesitarás el paquete del generador:

```bash
dotnet add package HttpButler.Generator
```

## Uso Básico

### 1. Define una interfaz HTTP

Crea una interfaz con los atributos `[HttpButler]` y `[Route]`:

```csharp
using HttpButler.Attributes;

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
    Task PostTodoAsync([ToBody] Todo todo);
}
```

### 2. Registra la interfaz en DI

En tu `Program.cs`, registra la interfaz usando `AddHttpButler`:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Registrar todas las interfaces generadas automáticamente
builder.Services.AddHttpButler();

// O registrar manualmente cada interface
builder.Services.AddHttpButler<IJsonPlaceHolderTodo, gHttpButler_IJsonPlaceHolderTodo>();

var app = builder.Build();
```

### 3. Usa la interfaz en tu código

Inyecta la interfaz y úsala como cualquier otro servicio:

```csharp
[ApiController]
[Route("api/[controller]")]
public class TodosController : ControllerBase
{
    private readonly IJsonPlaceHolderTodo _todoClient;

    public TodosController(IJsonPlaceHolderTodo todoClient)
    {
        _todoClient = todoClient;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTodo(int id)
    {
        var todo = await _todoClient.GetTodoAsync(id);
        if (todo == null)
            return NotFound();

        return Ok(todo);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTodo([FromBody] Todo todo)
    {
        await _todoClient.PostTodoAsync(todo);
        return Created();
    }
}
```

## Atributos Disponibles

### `[HttpButler]`
Marca una interfaz como cliente HTTP. HttpButler generará automáticamente su implementación.

```csharp
[HttpButler]
public interface IMyApiClient
{
    // métodos...
}
```

### `[Route]`
Define la URL base para el cliente HTTP.

```csharp
[Route("https://api.example.com/v1/")]
public interface IMyApiClient
{
    // métodos...
}
```

### Atributos de Método HTTP

- `[HttpGet(path)]` - Solicitud GET
- `[HttpPost(path)]` - Solicitud POST
- `[HttpPut(path)]` - Solicitud PUT
- `[HttpDelete(path)]` - Solicitud DELETE

```csharp
[HttpGet("users")]
Task<List<User>> GetUsersAsync();

[HttpPost("users")]
Task CreateUserAsync([ToBody] User user);
```

### Atributos de Parámetro

- `[ToRoute]` - Parámetro en la ruta (reemplaza `{paramName}`)
- `[ToQuery]` - Parámetro de query string
- `[ToBody]` - Cuerpo de la solicitud

```csharp
[HttpGet("users/{id}")]
Task<User> GetUserAsync([ToRoute] int id);

[HttpGet("users")]
Task<List<User>> SearchUsersAsync([ToQuery] string name, [ToQuery] int age);

[HttpPost("users")]
Task CreateUserAsync([ToBody] User user);
```

## Cómo Funciona

1. **Atributos**: Defines una interfaz con atributos que describen las llamadas HTTP
2. **Source Generator**: Durante la compilación, `InterfaceImplementationGenerator` escanea las interfaces marcadas con `[HttpButler]`
3. **Generación de Código**: Genera automáticamente una clase implementación (ej: `gHttpButler_IJsonPlaceHolderTodo`) que implementa la interfaz
4. **Inyección de Dependencias**: Registra la interfaz y su implementación en el contenedor DI
5. **Uso**: Inyecta la interfaz en tus controladores o servicios y úsala normalmente

## Arquitectura

```
HttpButler/
├── Attributes/                       # Atributos para definir clientes HTTP
├── Services/                         # Servicios auxiliares
│   ├── IHttpClientService            # Interfaz para manejar solicitudes HTTP
│   └── IPathResolveService           # Interfaz para resolver rutas y parámetros
└── ServiceCollectionExtension        # Métodos de extensión para DI

HttpButler.Generator/
└── InterfaceImplementationGenerator  # Source Generator que crea las implementaciones
```

## Ejemplo Completo

Consulta `HttpButler.TestApi` para ver un ejemplo funcional que usa `IJsonPlaceHolderTodo` para consumir la API de JSONPlaceholder.

```csharp
// Services/IJsonPlaceHolderTodo.cs
[HttpButler]
[Route("https://jsonplaceholder.typicode.com/")]
public interface IJsonPlaceHolderTodo
{
    [HttpGet("todos/{todoId}")]
    Task<Todo?> GetTodoAsync(int todoId);

    [HttpPost("todos")]
    Task PostTodoAsync([ToBody] Todo todo);
}

// Program.cs
builder.Services.AddHttpButler();

// Controllers/TodosController.cs
[ApiController]
public class TodosController : ControllerBase
{
    private readonly IJsonPlaceHolderTodo _client;

    public TodosController(IJsonPlaceHolderTodo client) => _client = client;

    [HttpGet("{id}")]
    public Task<Todo?> GetTodo(int id) => _client.GetTodoAsync(id);
}
```

## Licencia

MIT License - Ver `LICENSE.txt` para más detalles.

## Contribuir

Las contribuciones son bienvenidas. Por favor abre un issue o un pull request para reportar bugs o sugerir mejoras.
