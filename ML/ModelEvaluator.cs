using Microsoft.ML;
using System.IO;

namespace CaptchaApi.ML;

public class ModelEvaluator
{
    private static readonly string modelPath = @"C:\Nour\logs\mouse-model.zip";
    private static readonly MLContext mlContext = new MLContext();
    private static ITransformer? trainedModel;
    private static PredictionEngine<MouseData, MousePrediction>? predictionEngine;

    static ModelEvaluator()
    {
        try
        {
            if (File.Exists(modelPath))
            {
                using var stream = new FileStream(modelPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                trainedModel = mlContext.Model.Load(stream, out _);
                predictionEngine = mlContext.Model.CreatePredictionEngine<MouseData, MousePrediction>(trainedModel);
            }
            else
            {
                Console.WriteLine($"⛔ Model file not found: {modelPath}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⛔ Failed to load model: {ex.Message}");
        }
    }


    public static float PredictScore(MouseData input)
    {
        if (predictionEngine is null)
            throw new InvalidOperationException("Model not loaded.");

        var prediction = predictionEngine.Predict(input);

        // ✅ score[1] = احتمال ان يكون إنسان
        return prediction.Score;

    }
}
