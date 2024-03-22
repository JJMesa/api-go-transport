using System.Reflection;
using System.Text.Json.Serialization;
using GoTransport.Api.Extensions;
using GoTransport.Application;
using GoTransport.Persistence;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration
.AddUserSecrets(Assembly.GetExecutingAssembly(), true).Build();

builder.Services.AddHttpContextAccessor();
builder.Services.AddApplicationLayer(configuration);
builder.Services.AddPersistenceLayer(configuration);
builder.Services.AddControllers()
    .AddJsonOptions(opt => opt.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);

builder.Services.AddVersioningConfiguration();

builder.Services.AddCustomResponseFluentValidation();

builder.Services.AddJwtAuthentication(configuration);

builder.Services.AddCorsConfiguration();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGenConfiguration();

builder.Services.AddConventionConfiguration();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("Security");

app.UseAuthentication();

app.UseAuthorization();

app.UseErrorHandlerMiddleware();

app.MapControllers();

app.Run();