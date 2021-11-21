using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flask : MonoBehaviour
{
    public GameObject[] contentFlask;
    public float selectedPositionHeight = .5f;

    private int maxSize = 4;
    private List<Color> colors = new List<Color>();
    private bool selected = false;

    public void AddColor(Color color)
    {
        if (colors.Count < maxSize)
        {
            colors.Add(color);
            GameObject content = contentFlask[colors.Count - 1];
            content.SetActive(true);
            content.GetComponent<SpriteRenderer>().color = color;
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
            GameObject content = contentFlask[i];
            content.SetActive(false);
            colors.RemoveAt(i);
        }
    }

    public void SetSelected()
    {
        if (!selected)
        {
            gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + selectedPositionHeight);
            selected = true;
            Debug.Log(colors.Count);
        }
    }

    public void SetUnselected()
    {
        if (selected)
        {
            gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y - selectedPositionHeight);
            selected = false;
        }
    }

    public bool SpillTo(Flask flask)
    {
        List<Color> colorsToSpill = this.PopColors();
        if (CanSpillTo(flask, colorsToSpill))
        {
            RemoveColors(colorsToSpill.Count);
            colorsToSpill.ForEach(color => { flask.AddColor(color); });
            return true;
        }
        return false;
    }

    private bool CanSpillTo(Flask flask, List<Color> colorSpill)
    {
        int size = flask.GetColors().Count + colorSpill.Count;
        return flask.maxSize >= size;
    }

    public List<Color> GetColors()
    {
        return colors;
    }

    public int GetMaxSize()
    {
        return maxSize;
    }

    void Start()
    {

    }
}
