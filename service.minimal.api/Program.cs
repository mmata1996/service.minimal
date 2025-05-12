using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using service.minimal.api.Models;
using service.minimal.api.Repositories.Contracts;
using service.minimal.api.Repositories.Implentations;

namespace service.minimal.api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddSingleton(new List<TodoItem>());
            builder.Services.AddScoped<ITodoItemRepository, TodoItemRepository>();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            //Middleware para logear las peticiones
            app.Use((context, next) => { 
                Console.WriteLine($"Request: {context.Request.Method} {context.Request.Path}");
                return next(context);
            });

            app.MapGet("", () =>
            {
                return "Welcome to the Minimal API";
            }).WithName("test")
            .WithDisplayName("Probar Minimal Api")
            .WithSummary("Probar Minimal Api")
            .WithDescription("Hola, este EndPoint solo devuelve un saludo al usuario al ejecutar el mismo, ¡Gracias por llegar hasta aqui!")
            .WithOpenApi();

            app.MapGet("/todoitems", ([FromServices] ITodoItemRepository service) =>
            {
                return service.GetAllAsync();
            }).WithName("GetAllAvaibleTodos")
            .WithDisplayName("Obtener todos los TODOS")
            .WithSummary("Obtener todos los TODOS")
            .WithDescription("Este EndPoint devuelve una lista de todos los TODOs registrados en la base de datos")
            .WithOpenApi();

            app.MapGet("/todoitems/{id:long}", ([FromServices] ITodoItemRepository service, long id) =>
            {
                return service.GetByExpresionAsync(x => x.Id == id);
            }).WithName("GetAnTodoById")
            .WithDisplayName("Obtener un TODO por ID")
            .WithSummary("Obtener un TODO por ID")
            .WithDescription("Este EndPoint devuelve un TODO por ID, el cual es pasado como parametro en la URL")
            .WithOpenApi();

            app.MapPost("/todoitems", ([FromServices] ITodoItemRepository service, [FromBody] TodoItem todoItem) =>
            {
                return service.AddNewEntityAsync(todoItem);
            }).WithName("AddNewTodo")
            .WithDisplayName("Agregar un nuevo TODO")
            .WithSummary("Agregar un nuevo TODO")
            .WithDescription("Este EndPoint permite agregar un nuevo TODO a la base de datos, el cual es pasado como parametro en el body de la peticion")
            .WithOpenApi();

            app.MapPut("/todoitems", ([FromServices] ITodoItemRepository service, [FromBody] TodoItem todoItem) =>
            {
                return service.UpdateEntityAsync(todoItem);
            }).WithName("UpdateTodo")
            .WithDisplayName("Actualizar un TODO")
            .WithSummary("Actualizar un TODO")
            .WithDescription("Este EndPoint permite actualizar un TODO existente en la base de datos, el cual es pasado como parametro en el body de la peticion")
            .WithOpenApi();

            app.MapPost("/test", ([FromBody] TodoItem? TodoItem) =>
            {
                return "Test";
            }).AddEndpointFilter(async (context, next) =>
            {
                var todoItem = context.GetArgument<TodoItem>(0);

                if (todoItem == null)
                    return Results.Problem("El argumento esta nulo");

                if (todoItem.Id == 0)
                    return Results.BadRequest("El id no puede ser 0");

                if (todoItem.Title is null || string.IsNullOrEmpty(todoItem.Title))
                    return Results.BadRequest("la Title no puede nula");

                if (todoItem.Description is null || string.IsNullOrEmpty(todoItem.Description))
                    return Results.BadRequest("la descripcion no puede nula");

                var result = await next(context);

                return result;
            }).WithName("Test");

            app.Run();
        }
    }
}
