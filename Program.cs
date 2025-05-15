var builder = WebApplication.CreateBuilder(args);


//For Arabic font
Console.OutputEncoding = System.Text.Encoding.UTF8;


// 🟢 إضافة إعدادات CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
            "http://127.0.0.1:5500",
            "https://norman-deen.github.io"
        )
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});


// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ✅ تدريب النموذج مرة واحدة عند التشغيل
 //  CaptchaApi.ML.ModelTrainer.TrainAndSaveModel();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 🟢 تفعيل CORS هنا
app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();
