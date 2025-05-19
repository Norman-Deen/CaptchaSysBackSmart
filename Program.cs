var builder = WebApplication.CreateBuilder(args);

// 🟢 دعم UTF-8 للعربية
Console.OutputEncoding = System.Text.Encoding.UTF8;

// 🟢 إعداد CORS لدعم localhost و GitHub Pages
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

// 🟢 إضافة خدمات أساسية
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 🟢 Swagger فقط أثناء التطوير
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // 🔒 HTTPS فقط للإنتاج
    app.UseHttpsRedirection();
}

// 🟢 تفعيل CORS
app.UseCors("AllowFrontend");

app.UseAuthorization();
app.MapControllers();

// 🟢 دعم Render: بورت ديناميكي
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://*:{port}");

app.Run();



























// ✅ تدريب النموذج عند بدء التشغيل (معلّق مؤقتًا)
// CaptchaApi.ML.ModelTrainer.TrainAndSaveModel();