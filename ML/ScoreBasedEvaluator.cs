using System;
using System.IO;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace CaptchaApi.ML
{
    // This class is responsible for evaluating user input using a pre-trained ML model
    public class ScoreBasedEvaluator
    {
        // Path to the ZIP file containing the trained machine learning model
        private static readonly string modelPath = Path.Combine(AppContext.BaseDirectory, "ML", "score-model.zip");

        private readonly MLContext _mlContext; // ML.NET context used for model operations
        private readonly ITransformer _model; // Loaded ML model
        private readonly PredictionEngine<UserBehaviorInput, AnomalyPrediction> _predictionEngine; // Engine for making predictions

        public ScoreBasedEvaluator()
        {
            _mlContext = new MLContext();

            // Load the model from the ZIP file located in the "ML" folder
            using var stream = new FileStream(modelPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            _model = _mlContext.Model.Load(stream, out _);

            // Prepare the prediction engine for scoring input
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<UserBehaviorInput, AnomalyPrediction>(_model);
        }

        // Runs the input through the ML model and returns a score
        public float Evaluate(UserBehaviorInput input)
        {
            var prediction = _predictionEngine.Predict(input);
            return prediction.Score;
        }

        // Internal class to map the model output
        private class AnomalyPrediction
        {
            [ColumnName("Score")]
            public float Score { get; set; } // The numeric score returned by the model
        }
    }
}
