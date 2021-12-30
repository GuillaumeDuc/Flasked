using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Store
{
    public static int level = 0;
    public static List<List<Color>> savedFlasks = new List<List<Color>>();
    public static int retryCount = 3;

    public static void SaveFlasksBeginLevel(List<Flask> flasks)
    {
        savedFlasks.Clear();
        flasks.ForEach(f =>
        {
            savedFlasks.Add(new List<Color>(f.GetColors()));
        });
    }
}
