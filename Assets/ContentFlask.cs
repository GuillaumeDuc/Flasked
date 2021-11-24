using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContentFlask : MonoBehaviour
{
    private List<Vector3> defaultVertices = new List<Vector3>();
    private Material material;
    private Flask flask;
    private bool spill = false;
    private bool fill = false;
    public ContentType type;
    public enum ContentType
    {
        Bottom,
        Default
    }

    public void InitContentFlask(Material material, List<Vector2> listVertices, float positionY, ContentType type = ContentType.Default)
    {
        List<Vector3> defaultList = new List<Vector3>();
        listVertices.ForEach(v2 => { defaultList.Add(new Vector3(v2.x, v2.y)); });
        defaultVertices = defaultList;

        this.material = material;
        this.flask = gameObject.GetComponentInParent<Flask>();
        this.type = type;
        CreateMesh(listVertices, positionY);
    }

    public GameObject CreateMesh(List<Vector2> listVertices, float positionY)
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

        // Set up game object with mesh
        this.transform.position = new Vector3(gameObject.transform.parent.transform.position.x, gameObject.transform.parent.transform.position.y + positionY);
        this.gameObject.AddComponent(typeof(MeshRenderer));
        MeshFilter filter = this.gameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
        filter.mesh = msh;
        Renderer renderer = this.GetComponent<MeshRenderer>();
        Material mat = new Material(material);
        renderer.material = mat;
        this.gameObject.SetActive(false);
        return this.gameObject;
    }

    public void RotateContent(float angle)
    {
        switch (type)
        {
            case ContentType.Bottom:
                RotateContentBottom(angle);
                break;
            default:
                RotateContentDefault(angle);
                break;
        }
    }

    public void RotateContentBottom(float angle)
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = defaultVertices.ToArray();
        Vector2[] indices = new Vector2[vertices.Length];
        Vector2[] uv = new Vector2[vertices.Length];
        float mexHeightFlask = 1f;
        float minHeightFlask = -1.5f;

        for (var i = 0; i < vertices.Length; i++)
        {
            float yLeft = vertices[i].y - (.01f * angle);
            float yRight = vertices[i].y + .01f * angle;
            // Lower left vertices
            if (vertices[i].x < 0)
            {
                // Only upper vertices
                if (vertices[i].y > 0)
                {
                    yLeft = (yLeft <= minHeightFlask) ? minHeightFlask : yLeft;
                    vertices[i] = new Vector3(vertices[i].x, yLeft);
                }
            }
            else // Rise right vertices
            {
                // Only upper vertices
                if (vertices[i].y > 0)
                {
                    mexHeightFlask = (yLeft >= minHeightFlask) ? minHeightFlask : yLeft;
                    vertices[i] = new Vector3(vertices[i].x, yRight);
                }
            }

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

    public void RotateContentDefault(float angle)
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = defaultVertices.ToArray();
        Vector2[] indices = new Vector2[vertices.Length];
        Vector2[] uv = new Vector2[vertices.Length];
        float mexHeightFlask = 1f;
        float minHeightFlask = -1.5f;

        for (var i = 0; i < vertices.Length; i++)
        {
            float yLeft = vertices[i].y - (.01f * angle);
            float yRight = vertices[i].y + .01f * angle;
            // Lower left vertices
            if (vertices[i].x < 0)
            {
                yLeft = (yLeft <= minHeightFlask) ? minHeightFlask : yLeft;
                vertices[i] = new Vector3(vertices[i].x, yLeft);
            }
            else // Rise right vertices
            {
                mexHeightFlask = (yLeft >= minHeightFlask) ? minHeightFlask : yLeft;
                vertices[i] = new Vector3(vertices[i].x, yRight);
            }

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

    public void Fill()
    {

        Debug.Log("Fill");
    }

    public void Spill()
    {
        Debug.Log("Spill");
    }

    public void SetColor(Color color)
    {
        fill = true;
        gameObject.SetActive(true);
        gameObject.GetComponent<MeshRenderer>().material.SetColor("_color", color);
    }

    public void RemoveColor()
    {
        spill = true;
        gameObject.SetActive(false);
    }
}
