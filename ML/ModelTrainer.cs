using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Linq;

namespace CaptchaApi.ML;

public class MouseData
{
    [LoadColumn(4)] public string BehaviorType { get; set; } = "";
    [LoadColumn(5)] public float MaxSpeed;
    [LoadColumn(6)] public float LastSpeed;
    [LoadColumn(7)] public float SpeedStability;
    [LoadColumn(8)] public float MovementTime;
}


public class MouseDataBool
{
    [ColumnName("Label")]
    public bool Label { get; set; }

    public float MaxSpeed { get; set; }
    public float LastSpeed { get; set; }
    public float SpeedStability { get; set; }
    public float MovementTime { get; set; }
}

public class MousePrediction
{
    [ColumnName("PredictedLabel")]
    public bool PredictedLabel { get; set; }

    [ColumnName("Probability")]
    public float Score { get; set; }
}


public static class ModelTrainer
{
    public static void TrainAndSaveModel()
    {
        var mlContext = new MLContext();

        string dataPath = @"C:\Nour\logs\access-log.csv";
        string modelPath = @"C:\Nour\logs\mouse-model.zip";

        Console.WriteLine("🔥 Training Start");

        if (!File.Exists(dataPath))
        {
            Console.WriteLine("⛔ CSV not found.");
            return;
        }

        // تحميل كل البيانات من CSV
        var allData = mlContext.Data.LoadFromTextFile<MouseData>(
            dataPath, hasHeader: true, separatorChar: ',');

        // تصفية الصفوف التي تحتوي فقط على "human"

        var dataEnumerable = mlContext.Data.CreateEnumerable<MouseData>(allData, reuseRowObject: false);

        // ✅ طباعة أول القيم للتحقق
        foreach (var row in dataEnumerable.Take(10))
            Console.WriteLine($"🧪 Read: '{row.BehaviorType}'");

        var humansOnly = dataEnumerable
      .Where(x => !string.IsNullOrWhiteSpace(x.BehaviorType) && x.BehaviorType.Trim().Replace("\"", "").ToLower() == "human")
      .Select(x => new MouseDataBool
      {
          Label = true,
          MaxSpeed = x.MaxSpeed,
          LastSpeed = x.LastSpeed,
          SpeedStability = x.SpeedStability,
          MovementTime = x.MovementTime
      }).ToList();



        Console.WriteLine($"📊 Filtered rows (human): {humansOnly.Count}");

        if (humansOnly.Count == 0)
        {
            Console.WriteLine("⛔ No data to train on.");
            return;
        }

        var trainingData = mlContext.Data.LoadFromEnumerable(humansOnly);

        var pipeline = mlContext.Transforms.Concatenate("Features", nameof(MouseDataBool.MaxSpeed), nameof(MouseDataBool.LastSpeed), nameof(MouseDataBool.SpeedStability), nameof(MouseDataBool.MovementTime))
            .Append(mlContext.Transforms.NormalizeMinMax("Features"))
            .Append(mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(labelColumnName: "Label", featureColumnName: "Features"));

        var model = pipeline.Fit(trainingData);
        mlContext.Model.Save(model, trainingData.Schema, modelPath);

        Console.WriteLine($"✅ Model trained and saved to: {modelPath}");
        Console.ReadKey();
    }
}


