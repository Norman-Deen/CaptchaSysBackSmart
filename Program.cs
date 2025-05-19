var builder = WebApplication.CreateBuilder(args);

// 🟢 دعم الكتابة بالعربية في الكونسول
Console.OutputEncoding = System.Text.Encoding.UTF8;

// 🟢 إعداد CORS للسماح بالوصول من GitHub Pages والمحلي
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "http://127.0.0.1:5500",
            "https://norman-deen.github.io"
        )
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

// 🟢 إضافة الخدمات
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ✅ تدريب النموذج عند بدء التشغيل (معلّق مؤقتًا)
// CaptchaApi.ML.ModelTrainer.TrainAndSaveModel();

// 🟢 إعدادات بيئة التطوير
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 🟢 تفعيل CORS باستخدام السياسة المسمّاة
app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

// 🟢 احترام بورت Render
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://*:{port}");

app.Run();
