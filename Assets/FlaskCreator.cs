using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FlaskCreator
{
    static List<Color> listColor = new List<Color>() {
        Color.cyan,
        Color.red,
        Color.green,
        Color.yellow,
        Color.white,
        Color.magenta,
        Color.gray,
        new Color(.3f,.3f,.3f),
        new Color(.3f,0,.3f),
        new Color(.3f,.3f,0f),
        new Color(.3f,.9f,.3f),
    };

    public static List<Flask> CreateFlasks(GameObject prefab, int nbFlask, int nbContent, int nbEmpty, float contentHeight)
    {
        List<Flask> flasks = new List<Flask>();
        List<Color> colorList = GetColorFullContent(nbFlask - nbEmpty, nbContent, nbEmpty);
        float xStep = .08f;
        float yStep = .42f;
        float minX = .2f;
        float maxX = 1 - minX;
        float maxHeight = .6f;
        Vector3 pos = new Vector3(minX, maxHeight, 10);

        for (int i = 0; i < nbFlask; i++)
        {
            GameObject flaskGO = GameObject.Instantiate(prefab, Camera.main.ViewportToWorldPoint(pos), Quaternion.identity);
            pos += new Vector3(xStep, 0);
            // Next row
            if (pos.x > maxX)
            {
                pos = new Vector3(minX, pos.y - yStep, pos.z);
            }
            Flask flask = flaskGO.GetComponent<Flask>();
            flasks.Add(flask);
            flask.InitFlask(7 + i, nbContent);
            // Fill flask randomly
            if (i < nbFlask - nbEmpty)
            {
                // Fill until it reaches top
                for (int j = 0; j < nbContent; j++)
                {
                    int colorIndex = Random.Range(0, colorList.Count);
                    Color color = colorList[colorIndex];
                    colorList.RemoveAt(colorIndex);
                    flask.AddColor(color, contentHeight);
                }
            }
        }
        return flasks;
    }

    public static void FillFlasksRandom(List<Flask> flasks, int nbFlask, int nbContent, int nbEmpty, float contentHeight)
    {
        List<Color> colorList = GetColorFullContent(nbFlask - nbEmpty, nbContent, nbEmpty);
        for (int i = 0; i < nbFlask - nbEmpty; i++)
        {
            flasks[i].Clear();
            for (int j = 0; j < nbContent; j++)
            {
                int colorIndex = Random.Range(0, colorList.Count);
                Color color = colorList[colorIndex];
                colorList.RemoveAt(colorIndex);
                flasks[i].AddColor(color, contentHeight);
            }
        }
    }

    static List<Color> GetColorFullContent(int nbFullFlask, int nbContent, int nbEmpty)
    {
        List<Color> colorList = new List<Color>();
        // Fill list color
        for (int i = 0; i < nbFullFlask; i++)
        {
            for (int j = 0; j < nbContent; j++)
            {
                colorList.Add(listColor[i]);
            }
        }
        return colorList;
    }

    static void GetPos()
    {

    }
}
