using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flask : MonoBehaviour
{
    public Material material;

    private int maxSize = 4;
    private List<ContentFlask> contentFlask = new List<ContentFlask>();
    private List<Color> colors = new List<Color>();
    private bool selected = false;
    private AnimFlask animFlask;

    public void AddColor(Color color)
    {
        if (colors.Count < maxSize)
        {
            // Color match mesh index
            colors.Add(color);
            // Mesh
            ContentFlask content = contentFlask[colors.Count - 1];
            content.SetColor(color);
        }
    }

    public void InitColor(Color color)
    {
        if (colors.Count < maxSize)
        {
            // Color match mesh index
            colors.Add(color);
            // Mesh
            ContentFlask content = contentFlask[colors.Count - 1];
            content.InitColor(color);
        }
    }

    public List<Color> PopColors()
    {
        List<Color> popedColors = new List<Color>();
        if (colors.Count > 0)
        {
            bool sameColor;
            int i = colors.Count - 1;
            do
            {
                Color color = colors[i];
                popedColors.Add(color);
                // Next color is same color
                sameColor = i - 1 >= 0 && colors[i - 1].Equals(color);
                i--;
            } while (sameColor);
        }
        return popedColors;
    }

    void RemoveColors(int nbColors)
    {
        int size = colors.Count - 1;
        int stop = colors.Count - nbColors;
        for (int i = size; i >= stop; i--)
        {
            ContentFlask content = contentFlask[i];
            content.RemoveColor();
            colors.RemoveAt(i);
        }
    }

    public void SetSelected()
    {
        if (!selected)
        {
            animFlask.MoveSelected();
            selected = true;
            Debug.Log(colors.Count);
        }
    }

    public void SetUnselected()
    {
        if (selected)
        {
            animFlask.MoveUnselected();
            selected = false;
        }
    }

    public bool EqualsTopColor(Flask flask)
    {
        // Empty flask return true
        if (flask.GetColors().Count == 0)
        {
            return true;
        }
        if (colors.Count > 0 && flask.GetColors().Count > 0 && flask != null)
        {
            // Top color this flask is equal top color reference flask
            return this.colors[colors.Count - 1].Equals(flask.GetColors()[flask.GetColors().Count - 1]);
        }
        return false;
    }

    private bool HasEnoughSpace(Flask flask, List<Color> colorSpill)
    {
        int size = flask.GetColors().Count + colorSpill.Count;
        return flask.maxSize >= size;
    }

    public bool SpillTo(Flask flask)
    {
        List<Color> colorsToSpill = this.PopColors();
        // Spill only when not in own flask, if flask is not null, if space is enough, if both top color match
        if (!this.Equals(flask) && flask != null && HasEnoughSpace(flask, colorsToSpill) && EqualsTopColor(flask))
        {
            RemoveColors(colorsToSpill.Count);
            colorsToSpill.ForEach(color => { flask.AddColor(color); });
            animFlask.SpillAnimation(flask);
            return true;
        }
        return false;
    }

    public List<Color> GetColors()
    {
        return colors;
    }

    public int GetMaxSize()
    {
        return maxSize;
    }

    public List<ContentFlask> GetContentFlask()
    {
        return contentFlask;
    }

    void CreateContent()
    {
        List<Vector2> bottomList = new List<Vector2>() {
            new Vector2(-.55f, -.3f),
            new Vector2(-.55f, .5f),
            new Vector2(.5f, .5f),
            new Vector2(.5f, -.3f),
            // Quarter Circle
            new Vector2(.4f, -.4f),
            new Vector2(.25f, -.5f),
            new Vector2(0f, -.53f),
            new Vector2(-.3f, -.5f),
            new Vector2(-.425f, -.4f),
        };
        List<Vector2> squareList = new List<Vector2>() {
            new Vector2(-.55f, -.5f),
            new Vector2(-.55f, .5f),
            new Vector2(.5f, .5f),
            new Vector2(.5f, -.5f)
        };
        ContentFlask firstContent = CreateContentFlask(bottomList, -1.5f, ContentFlask.ContentType.Bottom);
        contentFlask.Add(firstContent);
        float y = -.5f;
        for (int i = 1; i < maxSize; i++)
        {
            contentFlask.Add(CreateContentFlask(squareList, y));
            y += 1f;
        }
    }

    ContentFlask CreateContentFlask(List<Vector2> listVertices, float positionY, ContentFlask.ContentType type = ContentFlask.ContentType.Default)
    {
        // Set up game object with mesh
        GameObject meshObj = new GameObject("content");
        meshObj.transform.parent = gameObject.transform;
        ContentFlask content = meshObj.AddComponent<ContentFlask>();
        content.InitContentFlask(material, listVertices, positionY, type);
        return content;
    }

    public ContentFlask GetContentToBeFilled()
    {
        return contentFlask.FindLast(currentFlask =>
        {
            return currentFlask.fill;
        });
    }

    public ContentFlask GetContentToSpill()
    {
        return contentFlask.FindLast(currentFlask =>
        {
            return currentFlask.spill;
        });
    }

    public void InitFlask()
    {
        animFlask = gameObject.GetComponent<AnimFlask>();
        CreateContent();
    }
}
