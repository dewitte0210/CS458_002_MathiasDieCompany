using FeatureRecognitionAPI.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Net.Http.Headers;
using System.Net.Http.Formatting;
using static System.Net.Mime.MediaTypeNames;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IFeatureRecognitionService,  FeatureRecognitionService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FeatureRecognitionPolicy", builder =>
    {
        builder.WithOrigins("http://localhost:3000")
               //.WithMethods("POST", "GET", "PUT", "DELETE")
               //.WithHeaders(HeaderNames.ContentType);
               .AllowAnyMethod() // Temporarily allow any method
               .AllowAnyHeader() // Temporarily allow any header
               .AllowCredentials(); // Allow credentials if needed
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("FeatureRecognitionPolicy");
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
