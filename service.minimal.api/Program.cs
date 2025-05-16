using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using service.minimal.api.Models;
using service.minimal.api.Repositories.Contracts;
using service.minimal.api.Repositories.Implentations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace service.minimal.api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    //url de quien emite el token   
                    ValidIssuer = "http://localhost:5238",
                    //user que puede usar el token
                    ValidAudience = "http://localhost:5238",
                    RequireExpirationTime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes("Tu_Clave_Secreta_Larga_Y_Segura_123456!")
                    )
                };
            });


            builder.Services.AddAuthorizationBuilder();

            builder.Services.AddAuthorizationBuilder();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(setup =>
            {
                var jwtSecurityScheme = new OpenApiSecurityScheme
                {
                    BearerFormat = "JWT",
                    Name = "JWT Authentication",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                    Description = "Put **_ONLY_** your JWT Bearer token on textbox below!",
                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };
                setup.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

                setup.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { 
                        jwtSecurityScheme, Array.Empty<string>() 
                    }
                });
            });

            builder.Services.AddSingleton(new List<TodoItem>());
            builder.Services.AddScoped<ITodoItemRepository, TodoItemRepository>();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }


            app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();

            //Middleware para logear las peticiones
            app.Use((context, next) =>
            {
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


            app.MapGet("/token/get/admin-token", () =>
            {

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Tu_Clave_Secreta_Larga_Y_Segura_123456!"));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, "mmata@gmail.com"),
                    new Claim(JwtRegisteredClaimNames.Email, "mmata@gmail.com"),
                    new Claim("role", "admin"),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                var token = new JwtSecurityToken(
                    issuer: "http://localhost:5238",
                    audience: "http://localhost:5238",
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(30),
                    signingCredentials: creds
                );

                return new JwtSecurityTokenHandler().WriteToken(token);

            }).WithDisplayName("Generar un Token")
            .WithSummary("Generar un Token JWT")
            .WithDescription("Generar un Token JWT para fines de prueba")
            .WithOpenApi();

            app.MapGet("/token/get/user-token", () =>
            {

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Tu_Clave_Secreta_Larga_Y_Segura_123456!"));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, "mmata@gmail.com"),
                    new Claim(JwtRegisteredClaimNames.Email, "mmata@gmail.com"),
                    new Claim("role", "user"),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                var token = new JwtSecurityToken(
                    issuer: "http://localhost:5238",
                    audience: "http://localhost:5238",
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(30),
                    signingCredentials: creds
                );

                return new JwtSecurityTokenHandler().WriteToken(token);

            }).WithDisplayName("Generar un Token")
            .WithSummary("Generar un Token JWT")
            .WithDescription("Generar un Token JWT para fines de prueba")
            .WithOpenApi();


            app.MapGet("/token/get/admin-user-token", () =>
            {

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Tu_Clave_Secreta_Larga_Y_Segura_123456!"));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, "mmata@gmail.com"),
                    new Claim(JwtRegisteredClaimNames.Email, "mmata@gmail.com"),
                    new Claim("role", "user"),
                    new Claim("role", "admin"),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                var token = new JwtSecurityToken(
                    issuer: "http://localhost:5238",
                    audience: "http://localhost:5238",
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(30),
                    signingCredentials: creds
                );

                return new JwtSecurityTokenHandler().WriteToken(token);

            }).WithDisplayName("Generar un Token")
            .WithSummary("Generar un Token JWT")
            .WithDescription("Generar un Token JWT para fines de prueba")
            .WithOpenApi();

            app.MapGet("/token/protegido-admin-no-police-name", () => {
                return "Token admin valido";
            })
            .AddEndpointFilter(async (context, next) => {

                var user = context.HttpContext.User;
                var identity = user.Identity as ClaimsIdentity;

                if (!identity.IsAuthenticated)
                {
                    return Results.Unauthorized();
                }

                return await next(context);
            })
            .WithDisplayName("Endpoint protegido")
            .WithSummary("Endpoint protegido")
            .WithDescription("Endpoint protegido")
            .WithOpenApi();

            app.MapGet("/token/protegido-admin", () => { 
                return "Token admin valido";
            })
            .AddEndpointFilter(async (context, next) =>
            {
                var user = context.HttpContext.User;

                if (user == null)
                    return Results.Unauthorized();

                var identity = user.Identity as ClaimsIdentity;

                if(!identity.IsAuthenticated)
                    return Results.Unauthorized();

                var haveRole = identity.Claims
                .Where(x => x.Type.Contains("role"))
                .Select(x => x.Value);

                var haveAdminrole = haveRole.Where(x => x == "admin").Any();

                if (!haveAdminrole)
                    return Results.Forbid(null, ["Solo admin, manin"]);

                return await next(context);
            })
            .WithDisplayName("Endpoint protegido")
            .WithSummary("Endpoint protegido")
            .WithDescription("Endpoint protegido")
            .WithOpenApi();

            app.MapGet("/token/protegido-user", () => {
                return "Token user valido";
            }).AddEndpointFilter(async (context, next) =>
            {
                var user = context.HttpContext.User;

                if (user == null)
                    return Results.Unauthorized();

                var identity = user.Identity as ClaimsIdentity;

                if (!identity.IsAuthenticated)
                    return Results.Unauthorized();

                var haveRole = identity.Claims
                .Where(x => x.Type.Contains("role"))
                .Select(x => x.Value);

                var haveAdminrole = haveRole.Where(x => x == "user").Any();

                if (!haveAdminrole)
                    return Results.Forbid(null, ["Solo user, manin"]);

                return await next(context);
            })
            .WithDisplayName("Endpoint protegido")
            .WithSummary("Endpoint protegido")
            .WithDescription("Endpoint protegido")
            .WithOpenApi();

            app.MapGet("/token/protegido-admin-user", () => {
                return "Token admin user valido";
            }).AddEndpointFilter(async (context, next) =>
            {
                var user = context.HttpContext.User;

                if (user == null)
                    return Results.Unauthorized();

                var identity = user.Identity as ClaimsIdentity;

                if (!identity.IsAuthenticated)
                    return Results.Unauthorized();

                var haveRole = identity.Claims
                .Where(x => x.Type.Contains("role"))
                .Select(x => x.Value);

                var haveAdminrole = haveRole.Where(x => x == "admin" || x == "user").Any();

                if (!haveAdminrole)
                    return Results.Forbid(null, ["Solo admin o user, manin"]);

                return await next(context);
            })
            .RequireAuthorization("UserOrAdmin")
            .WithDisplayName("Endpoint protegido")
            .WithSummary("Endpoint protegido")
            .WithDescription("Endpoint protegido")
            .WithOpenApi();

            app.MapGet("/token/noprotegido", () => {
                return "no protegido valido";
            })
            .WithDisplayName("Endpoint no protegido")
            .WithSummary("Endpoint no protegido")
            .WithDescription("Endpoint no protegido")
            .WithOpenApi();

            app.Run();
        }
    }
}
