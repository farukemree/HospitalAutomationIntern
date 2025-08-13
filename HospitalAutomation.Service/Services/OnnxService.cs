using HospitalAutomation.Service.Interfaces;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class PredictResponseDto
{
    public string PredictedDisease { get; set; }
    public string Department { get; set; }
}

public class OnnxService : IOnnxService
{
    private readonly InferenceSession _session;

    // Hastalık - Bölüm eşlemesi (örnek)
    private readonly Dictionary<string, string> _diseaseToDepartment = new Dictionary<string, string>
    {
        { "Covid-19", "Dahiliye" },
        { "Grip", "Enfeksiyon" },
        { "Migren", "Nöroloji" }
        // diğer hastalıklar buraya eklenir
    };

    public OnnxService()
    {
        var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "OnnxModel", "model.onnx");

        if (!File.Exists(modelPath))
        {
            throw new FileNotFoundException($"❌ ONNX modeli bulunamadı: {modelPath}");
        }

        _session = new InferenceSession(modelPath);
    }

    public PredictResponseDto Predict(string symptoms)
    {
        var inputTensor = new DenseTensor<string>(new[] { symptoms }, new[] { 1, 1 });

        using var results = _session.Run(new List<NamedOnnxValue>
    {
        NamedOnnxValue.CreateFromTensor("input", inputTensor)
    });

        var predictedDisease = results.First().AsEnumerable<string>().First();

        var key = predictedDisease.Trim().ToLowerInvariant();

        var department = _diseaseToDepartment
            .Where(kv => kv.Key.ToLowerInvariant() == key)
            .Select(kv => kv.Value)
            .FirstOrDefault() ?? "Dahiliye";

        return new PredictResponseDto
        {
            PredictedDisease = predictedDisease,
            Department = department
        };
    }


}
