using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Flask : MonoBehaviour
{
    public Material material;
    public Material clearedMaterial;

    private int maxSize = 4;
    public float contentHeight = 1;
    public int nbPoints = 5;
    private List<Color> colors = new List<Color>();
    private bool selected = false;
    private AnimFlask animFlask;
    private bool clearedState = false;

    public void AddColor(Color color, float height)
    {
        if (colors.Count < maxSize)
        {
            // Mesh
            Container container = GetComponentInChildren<Container>();
            ContentFlask content = container.AddContentFlask(.1f, height, color, material, nbPoints);
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
            colors.RemoveAt(i);
        }
    }

    public void SetSelected()
    {
        if (!selected)
        {
            animFlask.MoveSelected();
            selected = true;
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

    public bool CanSpill(Flask flask, List<Color> colorsToSpill)
    {
        return !this.Equals(flask) &&
        flask != null &&
        HasEnoughSpace(flask, colorsToSpill) &&
        EqualsTopColor(flask) &&
        !IsEmpty() &&
        !IsMoving() &&
        !flask.IsMoving();
    }

    public bool IsMoving()
    {
        return animFlask.IsMoving();
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
        if (CanSpill(flask, colorsToSpill))
        {
            // Play animation
            animFlask.SpillAnimation(flask);
            // Remove content from flask
            RemoveColors(colorsToSpill.Count);
            // Add color to target flask
            colorsToSpill.ForEach(color => { flask.GetColors().Add(color); });
            // Check if target flask is full, if so remove collider
            if (flask.IsCleared())
            {
                flask.RemoveCollider();
            }
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

    public void InitFlask(int layerFlaskContainer, int maxSize)
    {
        this.maxSize = maxSize;
        animFlask = gameObject.GetComponent<AnimFlask>();
        GetComponentInChildren<Container>().gameObject.layer = layerFlaskContainer;
    }

    public bool IsCleared()
    {
        if (colors.Count == 0)
        {
            return false;
        }
        bool sameColor = true;
        Color color = colors[0];
        colors.ForEach(c =>
        {
            if (!c.Equals(color))
            {
                sameColor = false;
            }
        });
        return sameColor && colors.Count == maxSize;
    }

    public bool IsEmpty()
    {
        return colors.Count == 0;
    }

    public void RemoveCollider()
    {
        // Remove Collider
        Destroy(GetComponent<BoxCollider>());
    }

    public void SetClearedMaterial(int intensity)
    {
        if (!clearedState)
        {
            // Change Flask Material
            GetComponentInChildren<Container>().GetTopContent().SetMaterial(clearedMaterial, intensity);
            clearedState = true;
        }
    }
}
