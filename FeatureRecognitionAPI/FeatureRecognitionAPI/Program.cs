using FeatureRecognitionAPI.Services;
using Microsoft.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddSwaggerGen();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<IFeatureRecognitionService, FeatureRecognitionService>();
builder.Services.AddScoped<IPricingService, PricingService>();
builder.Services.AddScoped<IPricingDataService, PricingDataService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FeatureRecognitionPolicy", builder =>
    {
        builder.WithOrigins("*")
                .WithHeaders(HeaderNames.AccessControlAllowOrigin)
               .AllowAnyMethod() // Temporarily allow any method
               .AllowAnyHeader(); // Temporarily allow any header
                                  //.AllowCredentials(); // Allow credentials if needed
                                  //.WithMethods("POST", "GET", "PUT", "DELETE")
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
