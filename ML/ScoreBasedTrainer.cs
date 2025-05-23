using System;
using System.IO;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace CaptchaApi.ML
{
    // This class is responsible for training a machine learning model to detect anomalies in user behavior
    public class ScoreBasedTrainer
    {
        // Path to the dataset used for training (CSV format)
        private static readonly string dataPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "access-log.csv");

        // Path to save the trained model
        private static readonly string modelPath = Path.Combine(Directory.GetCurrentDirectory(), "ML", "score-model.zip");

        private readonly MLContext _mlContext;

        public ScoreBasedTrainer()
        {
            _mlContext = new MLContext(seed: 1); // Set seed for reproducibility
        }

        public void Train()
        {
            Console.WriteLine("Training started...");

            // Step 1: Load data from CSV file into IDataView
            IDataView data = _mlContext.Data.LoadFromTextFile<UserBehaviorInput>(
                path: dataPath,
                hasHeader: true,
                separatorChar: ',');

            Console.WriteLine("Data loaded successfully.");

            // Step 2: Replace missing values and prepare features for the model
            var pipeline = _mlContext.Transforms.ReplaceMissingValues(nameof(UserBehaviorInput.inputType))
                .Append(_mlContext.Transforms.ReplaceMissingValues(nameof(UserBehaviorInput.verticalScore)))
                .Append(_mlContext.Transforms.ReplaceMissingValues(nameof(UserBehaviorInput.verticalCount)))
                .Append(_mlContext.Transforms.ReplaceMissingValues(nameof(UserBehaviorInput.totalVerticalMovement)))
                .Append(_mlContext.Transforms.ReplaceMissingValues(nameof(UserBehaviorInput.avgSpeed)))
                .Append(_mlContext.Transforms.ReplaceMissingValues(nameof(UserBehaviorInput.stdSpeed)))
                .Append(_mlContext.Transforms.ReplaceMissingValues(nameof(UserBehaviorInput.accelerationChanges)))
                .Append(_mlContext.Transforms.ReplaceMissingValues(nameof(UserBehaviorInput.maxSpeed)))
                .Append(_mlContext.Transforms.ReplaceMissingValues(nameof(UserBehaviorInput.lastSpeed)))
                .Append(_mlContext.Transforms.ReplaceMissingValues(nameof(UserBehaviorInput.speedStability)))
                .Append(_mlContext.Transforms.ReplaceMissingValues(nameof(UserBehaviorInput.movementTime)))
                .Append(_mlContext.Transforms.ReplaceMissingValues(nameof(UserBehaviorInput.decelerationRate)))
                .Append(_mlContext.Transforms.ReplaceMissingValues(nameof(UserBehaviorInput.speedVariance)))
                .Append(_mlContext.Transforms.Concatenate("Features",
                    nameof(UserBehaviorInput.inputType),
                    nameof(UserBehaviorInput.verticalScore),
                    nameof(UserBehaviorInput.verticalCount),
                    nameof(UserBehaviorInput.totalVerticalMovement),
                    nameof(UserBehaviorInput.avgSpeed),
                    nameof(UserBehaviorInput.stdSpeed),
                    nameof(UserBehaviorInput.accelerationChanges),
                    nameof(UserBehaviorInput.maxSpeed),
                    nameof(UserBehaviorInput.lastSpeed),
                    nameof(UserBehaviorInput.speedStability),
                    nameof(UserBehaviorInput.movementTime),
                    nameof(UserBehaviorInput.decelerationRate),
                    nameof(UserBehaviorInput.speedVariance)
                ));

            // Step 3: Add the RandomizedPCA anomaly detection trainer
            var trainer = _mlContext.AnomalyDetection.Trainers.RandomizedPca(
                featureColumnName: "Features",
                rank: 3);

            var trainingPipeline = pipeline.Append(trainer);

            // Step 4: Fit the pipeline to the data (train the model)
            var model = trainingPipeline.Fit(data);

            Console.WriteLine("Model training completed.");

            // Step 5: Save the trained model to disk
            using (var fileStream = new FileStream(modelPath, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                _mlContext.Model.Save(model, data.Schema, fileStream);
            }

            Console.WriteLine("Model saved to:");
            Console.WriteLine(modelPath);
        }
    }

    // This class defines the structure of the input data used for training
    public class UserBehaviorInput
    {
        [LoadColumn(0)] public float inputType;              // 0 = mouse, 1 = touch
        [LoadColumn(1)] public float verticalScore;          // Vertical accuracy (touch only)
        [LoadColumn(2)] public float verticalCount;          // Number of vertical movements
        [LoadColumn(3)] public float totalVerticalMovement;  // Total distance of vertical movement
        [LoadColumn(4)] public float avgSpeed;               // Average interaction speed
        [LoadColumn(5)] public float stdSpeed;               // Speed standard deviation
        [LoadColumn(6)] public float accelerationChanges;    // Number of acceleration changes
        [LoadColumn(7)] public float maxSpeed;               // Max speed reached during movement
        [LoadColumn(8)] public float lastSpeed;              // Last recorded speed
        [LoadColumn(9)] public float speedStability;         // How stable the speed was
        [LoadColumn(10)] public float movementTime;          // Duration of interaction
        [LoadColumn(11)] public float decelerationRate;      // Rate of slowing down
        [LoadColumn(12)] public float speedVariance;         // Statistical variance of speeds
    }
}
