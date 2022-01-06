using System.Collections.Generic;
using UnityEngine;

public static class SaveDataManager
{
    public static void SaveJsonData(Store store)
    {
        FileManager.WriteToFile("Save.dat", JsonUtility.ToJson(store));
    }

    public static void LoadJsonData(Store store)
    {
        if (FileManager.LoadFromFile("Save.dat", out var json))
        {
            JsonUtility.FromJsonOverwrite(json, store);
        }
    }
}
