using HospitalAutomation.Service.Interfaces;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class OnnxService : IOnnxService
{
    private readonly InferenceSession _session;

    public OnnxService()
    {
        // Model dosya yolu
        var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "OnnxModel", "model.onnx");

        if (!File.Exists(modelPath))
        {
            throw new FileNotFoundException($"❌ ONNX modeli bulunamadı: {modelPath}");
        }

        // Modeli yükle
        _session = new InferenceSession(modelPath);
    }

    public string Predict(string inputText)
    {
        if (string.IsNullOrWhiteSpace(inputText))
            throw new ArgumentException("Giriş metni boş olamaz.");

        // Giriş tensörü oluştur
        var inputTensor = new DenseTensor<string>(new[] { inputText }, new[] { 1, 1 });

        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input", inputTensor)
        };

        // Modeli çalıştır
        using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = _session.Run(inputs);

        // Tahmin sonucunu al
        var prediction = results.First().AsEnumerable<string>().First();

        return prediction;
    }
}
