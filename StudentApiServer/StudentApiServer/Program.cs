var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("StudentApiCorsPolicy", policy =>
    {
        policy.WithOrigins("https://localhost:7036", "http://localhost:5003")
        .AllowAnyHeader()
        .AllowAnyMethod(); // Configures CORS to allow requests from specified origins with any header and method.
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("StudentApiCorsPolicy"); // Applies the defined CORS policy to the application.

app.MapControllers();

app.Run();
