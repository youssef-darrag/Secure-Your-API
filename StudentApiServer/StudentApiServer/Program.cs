using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using StudentApiServer.Authorization;

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
        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("THIS_IS_A_VERY_SECRET_KEY_123456")), // Specifies the symmetric security key used to validate the token's signature.
        ClockSkew = TimeSpan.Zero // Sets the clock skew to zero to prevent any leeway in token expiration validation.
    };
});

builder.Services.AddSingleton<IAuthorizationHandler, StudentOwnerOrAdminHandler>(); // Registers the custom authorization handler for the StudentOwnerOrAdminRequirement.

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("StudentOwnerOrAdmin", policy =>
        policy.Requirements.Add(new StudentOwnerOrAdminRequirement())); // Adds the custom authorization requirement to the policy.
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
// Register Swagger generator and customize its behavior.
builder.Services.AddSwaggerGen(options =>
{
    // ===============================
    // 1) Define the JWT Bearer security scheme
    // ===============================
    //
    // This tells Swagger that our API uses JWT Bearer authentication
    // through the HTTP Authorization header.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        // The name of the HTTP header where the token will be sent.
        Name = "Authorization",

        // Indicates this is an HTTP authentication scheme.
        Type = SecuritySchemeType.Http,

        // Specifies the authentication scheme name.
        // Must be exactly "Bearer" for JWT Bearer tokens.
        Scheme = "Bearer",

        // Optional metadata to describe the token format.
        BearerFormat = "JWT",

        // Specifies that the token is sent in the request header.
        In = ParameterLocation.Header,

        // Text shown in Swagger UI to guide the user.
        Description = "Enter: Bearer {your JWT token}"
    });

    // ===============================
    // 2) Require the Bearer scheme for secured endpoints
    // ===============================
    //
    // This tells Swagger that endpoints protected by [Authorize]
    // require the Bearer token defined above.
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document, null)] = new List<string>()
    });
});

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

app.UseAuthorization(); // Enables authorization middleware to enforce access control based on user roles and claims.

app.MapControllers();

app.Run();
