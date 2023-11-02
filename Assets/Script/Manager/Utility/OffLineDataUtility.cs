using System.IO;
using System.Text;
using UnityEngine;
using System.Collections.Generic;
using JuneSample;

public class OffLineDataUtility
{
    public static T LoadData<T>(string dataPath) where T : class
    {
        string filePath = OffLineDataManager.Instance.OfflineDataFolder + dataPath;
        FileInfo file = new FileInfo(filePath);
        if (file.Exists)
        {
            byte[] data = File.ReadAllBytes(filePath);
        }
        return default(T);
    }

    public static Dictionary<TKey, TValue> LoadData<TKey, TValue>(string dataPath)
    {
        string filePath = OffLineDataManager.Instance.OfflineDataFolder + dataPath;
        FileInfo file = new FileInfo(filePath);
        if (file.Exists)
        {
            byte[] data = File.ReadAllBytes(filePath);
        }
        return default;
    }

    public static void SaveData<T>(string dataPath, T obj)
    {
        OffLineDataManager.Instance.CreateFolder();

        string filePath = OffLineDataManager.Instance.OfflineDataFolder + dataPath;
        string data = JsonUtility.ToJson(obj, true);

        File.WriteAllBytes(filePath, Encoding.UTF8.GetBytes(data));
    }
}
