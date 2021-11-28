using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flask : MonoBehaviour
{
    public Material material;

    private int maxSize = 4;
    public int height = 1;
    public int nbPoints = 20;
    private List<ContentFlask> contentFlask = new List<ContentFlask>();
    private List<Color> colors = new List<Color>();
    private bool selected = false;
    private AnimFlask animFlask;

    public void AddColor(Color color)
    {
        if (colors.Count < maxSize)
        {
            // Mesh
            ContentFlask content = CreateContentFlask(.1f, height, material, nbPoints);
            contentFlask.Add(content);
            content.SetColor(color);
            // Color match mesh index
            colors.Add(color);
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

    ContentFlask CreateContentFlask(float width, float height, Material material, int nbPoints)
    {
        // Find Container
        Container container = GetComponentInChildren<Container>();
        ContentFlask content = container.AddContentFlask(.1f, height, material, nbPoints);
        return content;
    }
    /*

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
    }*/

    public void InitFlask()
    {
        animFlask = gameObject.GetComponent<AnimFlask>();
    }
}
