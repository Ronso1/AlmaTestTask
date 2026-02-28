using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class PinSaveSystem
{
    private static string SavePath =>
        Path.Combine(Application.persistentDataPath, "pins.json");

    public static void Save(List<PinUI> pins)
    {
        PinsSaveData saveData = new PinsSaveData();

        saveData.Pins = new List<PinData>(pins.Count);

        foreach (var pin in pins)
        {
            pin.Data.Position = pin.RectTransform.anchoredPosition;
            saveData.Pins.Add(pin.Data);
        }

        string json = JsonUtility.ToJson(saveData, false);
        File.WriteAllText(SavePath, json);
    }

    public static PinsSaveData Load()
    {
        if (!File.Exists(SavePath))
            return null;

        string json = File.ReadAllText(SavePath);
        return JsonUtility.FromJson<PinsSaveData>(json);
    }
}