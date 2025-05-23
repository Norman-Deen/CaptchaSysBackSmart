using CaptchaApi.ML;

var builder = WebApplication.CreateBuilder(args);

// Enable UTF-8 output encoding (useful for Arabic characters)
Console.OutputEncoding = System.Text.Encoding.UTF8;

// Configure CORS to allow frontend applications from local and GitHub Pages
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "http://127.0.0.1:5500",
            "http://localhost:5500",
            "https://norman-deen.github.io"
        )
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

// Register core services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Enable Swagger UI only in development mode
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Enable HTTPS redirection in production for security
    app.UseHttpsRedirection();
}

// Enable the defined CORS policy
app.UseCors("AllowFrontend");

app.UseAuthorization();
app.MapControllers();

// Support for Render deployment: use dynamic port assigned by the platform
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://*:{port}");

// Optionally train the ML model at application startup (currently disabled)
// new ScoreBasedTrainer().Train();

app.Run();
