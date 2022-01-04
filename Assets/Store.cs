using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Store
{
    public static int level = 0;
    public static List<List<Color>> savedFlasks = new List<List<Color>>();
    public static List<List<List<Color>>> savedScene = new List<List<List<Color>>>();
    public static int retryCount = 3;
    public static int undoCount = 5;

    public static void SaveFlasksBeginLevel(List<Flask> flasks)
    {
        savedFlasks.Clear();
        flasks.ForEach(f =>
        {
            savedFlasks.Add(new List<Color>(f.GetColors()));
        });
    }

    public static void SaveCurrentScene(List<Flask> flasks)
    {
        List<List<Color>> currentScene = new List<List<Color>>();
        flasks.ForEach(f =>
        {
            currentScene.Add(new List<Color>(f.GetColors()));
        });
        savedScene.Add(currentScene);
    }
}
