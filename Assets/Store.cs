using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Store
{
    public int level = 0;
    public ListFlask savedFlasks = new ListFlask();
    public List<ListFlask> savedScenes = new List<ListFlask>();

    [System.Serializable]
    public class ListFlask
    {
        public List<ListContent> flasks = new List<ListContent>();
        public void AddFlask(List<Color> contents)
        {
            flasks.Add(new ListContent(contents));
        }
        public void Clear()
        {
            flasks.Clear();
        }
        public int Count()
        {
            return flasks.Count;
        }

        public List<List<Color>> ToList()
        {
            List<List<Color>> res = new List<List<Color>>();
            flasks.ForEach(f =>
            {
                res.Add(f.contents);
            });
            return res;
        }

        [System.Serializable]
        public class ListContent
        {
            public List<Color> contents;
            public ListContent(List<Color> contents)
            {
                this.contents = new List<Color>(contents);
            }
        }
    }

    public int retryCount = 3;
    public int undoCount = 5;

    public void SaveFlasksBeginLevel(List<Flask> flasks)
    {
        savedFlasks.Clear();
        flasks.ForEach(f =>
        {
            savedFlasks.AddFlask(f.GetColors());
        });
    }

    public void SaveCurrentScene(List<Flask> flasks)
    {
        ListFlask currentScene = new ListFlask();
        flasks.ForEach(f =>
        {
            currentScene.AddFlask(f.GetColors());
        });
        savedScenes.Add(currentScene);
    }

    public void FetchData()
    {
        SaveDataManager.LoadJsonData(this);
    }

    public void SaveData()
    {
        SaveDataManager.SaveJsonData(this);
    }

    public void NextLevel()
    {
        level += 1;
        savedFlasks = new ListFlask();
        savedScenes = new List<ListFlask>();
        SaveData();
    }

    public void Reset()
    {
        level = 0;
        savedFlasks = new ListFlask();
        savedScenes = new List<ListFlask>();
        SaveData();
    }

    public void RetryScene()
    {
        savedScenes = new List<ListFlask>() { savedFlasks };
        retryCount -= 1;
        SaveData();
    }
}
