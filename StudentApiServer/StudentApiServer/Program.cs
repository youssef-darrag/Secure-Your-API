using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

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

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // Sets the default authentication scheme to JWT Bearer.
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; // Sets the default challenge scheme to JWT Bearer.
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true, // Validates the token issuer to ensure it matches the expected value.
        ValidateAudience = true, // Validates the token audience to ensure it matches the expected value.
        ValidateLifetime = true, // Validates the token's expiration time to ensure it is still valid.
        ValidateIssuerSigningKey = true, // Validates the signing key to ensure the token is properly signed.
        ValidIssuer = "StudentApi", // Specifies the expected issuer of the token.
        ValidAudience = "StudentApiUsers", // Specifies the expected audience of the token.
        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("THIS_IS_A_VERY_SECRET_KEY_123456")) // Specifies the symmetric security key used to validate the token's signature.
    };
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

app.UseAuthentication(); // Enables authentication middleware to validate JWT tokens in incoming requests.

app.MapControllers();

app.Run();
