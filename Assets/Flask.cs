using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flask : MonoBehaviour
{
    public GameObject[] contentFlask;

    private int maxSize = 4;
    private List<Color> colors = new List<Color>();

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

    public Color? PopColor()
    {
        if (colors.Count > 0)
        {
            Color color = colors[colors.Count - 1];
            GameObject content = contentFlask[colors.Count];
            content.SetActive(false);
            colors.RemoveAt(colors.Count - 1);
            return color;
        }
        return null;
    }
}
