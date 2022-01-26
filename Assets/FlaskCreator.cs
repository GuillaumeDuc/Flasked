using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

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
        new Color(.5f,.3f,.5f),
        new Color(.9f,0,.3f),
        new Color(.3f,.3f,.5f),
        new Color(.3f,.9f,.9f),
        new Color(.8f,.5f,.3f),
        new Color(.3f,.9f,.2f),
        new Color(.5f,.5f,.3f),
        new Color(.2f,.2f,.5f),
        new Color(.9f,.9f,.1f),
        new Color(.6f,.7f,.1f),
        new Color(.1f,.3f,.7f),
        new Color(.2f,.5f,.2f),
        new Color(.9f,.7f,.7f),
        new Color(.25f,.25f,.25f),
        new Color(.25f,.0f,0),
        new Color(0,.25f,0),
        new Color(0,.25f,.25f),
        new Color(.75f,.75f,.75f),
        new Color(.85f,0,.5f),
        new Color(.25f,.5f,.8f),
        new Color(.8f,.8f,.15f),
        new Color(0,.45f,.35f),
        new Color(.35f,.35f,.95f),
        new Color(0,1,.25f),
        new Color(1,1,.25f),
    };

    public static void DeleteFlasks(List<Flask> flasks)
    {
        for (int i = 0; i < flasks.Count; i++)
        {
            GameObject.DestroyImmediate(flasks[i].gameObject);
        }
        flasks = new List<Flask>();
    }

    public static void DeleteFlasks(List<NetworkFlask> flasks)
    {
        for (int i = 0; i < flasks.Count; i++)
        {
            GameObject.DestroyImmediate(flasks[i].gameObject);
        }
        flasks = new List<NetworkFlask>();
    }

    public static List<Flask> CreateFlasks(
        GameObject prefab,
        int nbFlask,
        int nbContent,
        int nbEmpty,
        float contentHeight,
        float minX = .2f,
        float maxX = .8f,
        float xStep = .08f,
        float yStep = .45f,
        float maxHeight = .65f,
        float spillingYOffset = 3,
        float spillingXOffset = 2
        )
    {
        List<Flask> flasks = new List<Flask>();
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
            // Set offset when spilling
            AnimFlask anim = flaskGO.GetComponent<AnimFlask>();
            anim.spillingYOffset = spillingYOffset;
            anim.spillingXOffset = spillingXOffset;

            Flask flask = flaskGO.GetComponent<Flask>();
            flasks.Add(flask);
            flask.InitFlask(8 + i, nbContent);
        }
        return flasks;
    }

    private static List<List<Color>> GetRandomScene(int size, int nbContent, int nbEmpty)
    {
        System.Random rand = new System.Random();
        List<List<Color>> listFlask = new List<List<Color>>();
        List<Color> colorList = GetColorFullContent(size - nbEmpty, nbContent, nbEmpty);
        for (int i = 0; i < size; i++)
        {
            listFlask.Add(new List<Color>());
            if (i < size - nbEmpty)
            {
                for (int j = 0; j < nbContent; j++)
                {
                    int colorIndex = rand.Next(0, colorList.Count);
                    Color color = colorList[colorIndex];
                    colorList.RemoveAt(colorIndex);
                    listFlask[i].Add(color);
                }
            }
        }
        return listFlask;
    }

    public static void ClearFlasks(List<Flask> flasks)
    {
        for (int i = 0; i < flasks.Count; i++)
        {
            flasks[i].Clear();
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

    public static int GetNbFlask(int level)
    {
        return 4 + ((int)(level / 3) * 2);
    }

    public static int GetNbFlaskMultiplayer(int level)
    {
        return 4 + (level * 3);
    }

    public static void RefillFlasks(List<Flask> flasks, List<List<Color>> savedList, float height)
    {
        // Remove content and add new content
        for (int i = 0; i < flasks.Count; i++)
        {
            flasks[i].Clear();
            flasks[i].FillWithList(savedList[i], height);
        }
    }

    public static List<List<Color>> GetSolvedRandomFlasks(int size, int nbContent, ref int nbEmpty)
    {
        // Random scene
        List<List<Color>> listColorFlasks = GetRandomScene(size, nbContent, nbEmpty);
        //Try to solve them
        bool solved = Solver.Solve(listColorFlasks, nbContent);
        int tentative = 1;
        while (!solved)
        {
            // 3 tentative, then add 1 empty flask
            if (tentative == 3)
            {
                nbEmpty += 1;
                tentative = 0;
            }
            listColorFlasks = FlaskCreator.GetRandomScene(size, nbContent, nbEmpty);
            solved = Solver.Solve(listColorFlasks, nbContent);
            tentative += 1;
        }
        return listColorFlasks;
    }

    public static List<List<Color>> BenchMark()
    {
        // Random scene
        List<List<Color>> listColorFlasks = GetClearable();
        LogFlasks(listColorFlasks);
        //Try to solve them
        bool solved = Solver.Solve(listColorFlasks, 4);
        return listColorFlasks;
    }

    static List<List<Color>> GetClearable()
    {
        return new List<List<Color>>() {
            new List<Color>(){ new Color(.5f,.3f,.5f), new Color(1,0,1), new Color(.1f,.3f,.7f), new Color(.3f,.3f,.5f)},
            new List<Color>(){ new Color(.6f,.7f,.1f), new Color(1,0,1), new Color(1,.9f,0), new Color(1,.9f,0)},
            new List<Color>(){ new Color(.5f,.3f,.5f), new Color(.1f,.3f,.7f), new Color(.8f,.5f,.3f), new Color(.8f,.5f,.3f)},
            new List<Color>(){ new Color(.9f,.9f,.1f), new Color(.9f,0,.3f), new Color(1,1,1), new Color(1,0,0)},
            new List<Color>(){ new Color(1,.9f,0), new Color(.3f,.3f,.5f), new Color(.3f,.9f,.9f), new Color(.6f,.7f,.1f)},
            new List<Color>(){ new Color(.3f,.9f,.2f), new Color(0,1,1), new Color(.5f,.5f,.3f), new Color(.9f,.9f,.1f)},
            new List<Color>(){ new Color(1,0,1), new Color(.5f,.5f,.5f), new Color(.9f,0,.3f), new Color(0,1,0)},
            new List<Color>(){  new Color(1,1,1), new Color(1,1,1), new Color(.2f,.2f,.5f), new Color(.5f,.5f,.3f)},

            new List<Color>(){ new Color(.2f,.2f,.5f), new Color(.6f,.7f,.1f), new Color(.3f,.9f,.2f), new Color(0,1,0)},
            new List<Color>(){ new Color(.2f,.2f,.5f), new Color(.3f,.3f,.5f), new Color(0,1,1), new Color(0,1,1)},
            new List<Color>(){ new Color(.1f,.3f,.7f), new Color(.5f,.5f,.5f), new Color(1,0,0), new Color(.3f,.9f,.2f)},
            new List<Color>(){ new Color(.3f,.9f,.9f), new Color(.5f,.3f,.5f), new Color(0,1,0), new Color(.2f,.2f,.5f)},
            new List<Color>(){ new Color(1,0,0), new Color(.3f,.3f,.5f), new Color(.8f,.5f,.3f), new Color(.9f,0,.3f)},
            new List<Color>(){ new Color(.3f,.9f,.9f), new Color(.8f,.5f,.3f), new Color(1,.9f,0), new Color(.5f,.5f,.5f)},
            new List<Color>(){ new Color(.3f,.9f,.2f), new Color(.5f,.5f,.5f), new Color(.6f,.7f,.1f), new Color(.5f,.5f,.3f)},
            new List<Color>(){ new Color(0,1,1), new Color(.3f,.9f,.9f), new Color(.5f,.5f,.3f), new Color(.5f,.3f,.5f)},

            new List<Color>(){ new Color(0,1,0), new Color(.1f,.3f,.7f), new Color(.9f,.9f,.1f), new Color(.9f,.9f,.1f)},
            new List<Color>(){ new Color(1,1,1), new Color(1,0,1), new Color(1,0,0), new Color(.9f,0,.3f)},

            new List<Color>(),
            new List<Color>(),
        };
    }

    public static void LogFlasks(List<List<Color>> flasks)
    {
        Debug.Log(" -------- LIST -----------");
        flasks.ForEach(f =>
        {
            string colors = "";
            f.ForEach(color =>
            {
                colors += color + " | ";
            });
            Debug.Log(colors);
        });
        Debug.Log(" -------- END -----------");
    }
}
