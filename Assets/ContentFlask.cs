using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ContentFlask : MonoBehaviour
{
    private List<Vector3> defaultVertices = new List<Vector3>();
    private Material material;
    private Flask flask;
    public bool spill = false;
    public bool fill = false;
    public ContentType type;
    float maxHeightFlask = 1f;
    float minHeightFlask = -1.5f;
    public enum ContentType
    {
        Bottom,
        Default
    }

    public void InitContentFlask(Material material, List<Vector2> listVertices, float positionY, ContentType type = ContentType.Default)
    {
        List<Vector3> defaultList = new List<Vector3>();
        listVertices.ForEach(v2 => { defaultList.Add(new Vector3(v2.x, v2.y)); });
        // Set default vertices
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
        // Hide object
        Spill();
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
                maxHeightFlask = (yRight >= maxHeightFlask) ? maxHeightFlask : yRight;
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

    public float GetTop(Vector3[] vertices)
    {
        float max = vertices.Length > 0 ? vertices[0].y : 0;
        for (int i = 0; i < vertices.Length; i++)
        {
            max = vertices[i].y > max ? vertices[i].y : max;
        }
        return max;
    }

    public float GetBottom(Vector3[] vertices)
    {
        float min = vertices.Length > 0 ? vertices[0].y : 0;
        for (int i = 0; i < vertices.Length; i++)
        {
            min = vertices[i].y < min ? vertices[i].y : min;
        }
        return min;
    }

    public Vector3[] GetLeftPoints(Vector3[] allPoints)
    {
        List<Vector3> points = new List<Vector3>();
        for (int i = 0; i < allPoints.Length; i++)
        {
            if (allPoints[i].x < 0)
            {
                points.Add(allPoints[i]);
            }
        }
        return points.ToArray();
    }

    public Vector3[] GetRightPoints(Vector3[] allPoints)
    {
        List<Vector3> points = new List<Vector3>();
        for (int i = 0; i < allPoints.Length; i++)
        {
            if (allPoints[i].x > 0)
            {
                points.Add(allPoints[i]);
            }
        }
        return points.ToArray();
    }

    public bool isEmpty()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        Vector3[] leftPoints = GetLeftPoints(vertices);
        Vector3[] rightPoints = GetRightPoints(vertices);
        float bottomLeft = GetBottom(leftPoints);
        float bottomRight = GetBottom(rightPoints);
        float topLeft = GetTop(leftPoints);
        float topRight = GetTop(rightPoints);
        float heightLeft = System.Math.Abs(topLeft - bottomLeft);
        float heightRight = System.Math.Abs(topRight - bottomRight);

        return (heightLeft <= 0 && heightRight <= 0);
    }

    public void Fill(float time = 1)
    {
        switch (type)
        {
            case ContentType.Bottom:
                fill = FillBottom(time);
                break;
            default:
                fill = FillDefault(time);
                break;
        }
    }

    public void Spill(float time = 1)
    {
        switch (type)
        {
            case ContentType.Bottom:
                spill = SpillBottom(time);
                break;
            default:
                spill = SpillDefault(time);
                break;
        }
    }

    bool FillBottom(float time = 1)
    {
        return false;
    }

    bool SpillBottom(float time = 1)
    {
        return false;
    }

    public bool FillDefault(float time = 1)
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        Vector2[] indices = new Vector2[vertices.Length];
        Vector2[] uv = new Vector2[vertices.Length];
        float height = defaultVertices[1].y - defaultVertices[0].y;
        time = 1 - time;

        // Add top left
        if (vertices[1].y < defaultVertices[1].y)
        {
            vertices[1].y = defaultVertices[1].y - (time * height);
        }
        // Add top right
        if (vertices[2].y < defaultVertices[2].y)
        {
            vertices[2].y = defaultVertices[2].y - (time * height);
        }

        for (int i = 0; i < vertices.Length; i++)
        {
            indices[i] = new Vector2(vertices[i].x, vertices[i].y);
            uv[i] = vertices[i];
        }

        mesh.Clear();
        mesh.vertices = vertices;
        Triangulator tr = new Triangulator(indices);
        mesh.triangles = tr.Triangulate();
        mesh.RecalculateNormals();
        mesh.uv = uv;
        // Finish fill
        if (time <= 0)
        {
            return false;
        }
        return true;
    }

    public bool SpillDefault(float time = 1)
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        Vector2[] indices = new Vector2[vertices.Length];
        Vector2[] uv = new Vector2[vertices.Length];

        Vector3[] leftPoints = GetLeftPoints(vertices);
        Vector3[] rightPoints = GetRightPoints(vertices);
        float bottomLeft = GetBottom(leftPoints);
        float bottomRight = GetBottom(rightPoints);
        float topLeft = GetTop(leftPoints);
        float topRight = GetTop(rightPoints);
        float heightLeft = topLeft - bottomLeft;
        float heightRight = topRight - bottomRight;

        for (var i = 0; i < vertices.Length; i++)
        {
            // Left vertices
            if (vertices[i].x < 0)
            {
                if (vertices[i].y > bottomLeft)
                {
                    vertices[i] = new Vector3(vertices[i].x, vertices[i].y - (time * heightLeft));
                }
            }
            else
            {
                // Right vertices
                if (vertices[i].y > bottomRight)
                {
                    vertices[i] = new Vector3(vertices[i].x, vertices[i].y - (time * heightRight));
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
        // Finished spill
        if (time >= 1)
        {
            return false;
        }
        return true;
    }

    public void InitColor(Color color)
    {
        Fill();
        gameObject.GetComponent<MeshRenderer>().material.SetColor("_color", color);
    }

    public void SetColor(Color color)
    {
        // Start filling
        fill = true;
        gameObject.GetComponent<MeshRenderer>().material.SetColor("_color", color);
    }

    public void RemoveColor()
    {
        // Start spilling
        spill = true;
    }
}
