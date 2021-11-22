using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flask : MonoBehaviour
{
    public Material material;

    private int maxSize = 4;
    private List<GameObject> contentFlask = new List<GameObject>();
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
            GameObject content = contentFlask[colors.Count - 1];
            Debug.Log(content);
            content.SetActive(true);
            content.GetComponent<MeshRenderer>().material.SetColor("_color", color);
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
            AnimateFlask(flask);
            return true;
        }
        return false;
    }

    void AnimateFlask(Flask flask)
    {
        animFlask.SpillAnimation(flask);
    }

    public List<Color> GetColors()
    {
        return colors;
    }

    public int GetMaxSize()
    {
        return maxSize;
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
        GameObject firstContent = CreateMesh(bottomList, -1.5f);
        GameObject secondContent = CreateMesh(squareList, -.5f);
        GameObject thirdContent = CreateMesh(squareList, .5f);
        GameObject fourthContent = CreateMesh(squareList, 1.5f);
        contentFlask.AddRange(new List<GameObject>() { firstContent, secondContent, thirdContent, fourthContent });
    }

    GameObject CreateMesh(List<Vector2> listVertices, float positionY)
    {
        Vector2[] list = listVertices.ToArray();
        // Use the triangulator to get indices for creating triangles
        Triangulator tr = new Triangulator(list);
        int[] indices = tr.Triangulate();

        // Create the Vector3 vertices
        Vector3[] vertices = new Vector3[list.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector3(list[i].x, list[i].y);
        }

        // Create the mesh
        Mesh msh = new Mesh();
        msh.vertices = vertices;
        msh.triangles = indices;
        msh.RecalculateNormals();
        msh.RecalculateBounds();

        // Set up game object with mesh;
        GameObject meshObj = new GameObject("content");
        meshObj.transform.parent = gameObject.transform;
        meshObj.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + positionY);
        meshObj.AddComponent(typeof(MeshRenderer));
        MeshFilter filter = meshObj.AddComponent(typeof(MeshFilter)) as MeshFilter;
        filter.mesh = msh;
        Renderer renderer = meshObj.GetComponent<MeshRenderer>();
        Material mat = new Material(material);
        renderer.material = mat;
        meshObj.SetActive(false);
        return meshObj;
    }

    void MoveContent()
    {
        Mesh mesh = contentFlask[0].GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        Vector2[] indices = new Vector2[vertices.Length];
        Vector2[] uv = new Vector2[vertices.Length];

        for (var i = 0; i < vertices.Length; i++)
        {
            vertices[i] += vertices[i] * 1.5f;
            indices[i] = new Vector2(vertices[i].x, vertices[i].y);
            uv[i] = vertices[i];
        }

        mesh.Clear();
        mesh.vertices = vertices;
        Triangulator tr = new Triangulator(indices);
        mesh.triangles = tr.Triangulate();
        mesh.RecalculateNormals();
        mesh.uv = uv;
    }

    public void InitFlask()
    {
        animFlask = gameObject.GetComponent<AnimFlask>();
        CreateContent();
    }
}
