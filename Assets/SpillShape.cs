using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpillShape : MonoBehaviour
{
    private Material material;
    private float width = .03f;
    private int nbPoints;

    public void Init(Material material, Vector2 position, int nbPoints)
    {
        transform.position = position;
        this.material = material;
        this.nbPoints = nbPoints;
        GenerateMesh(GetVertices(width, position, position, nbPoints), material);
    }

    public void UpdateShape(Vector2 target, float eulerAngle)
    {
        UpdateMesh(GetVertices(width, transform.position, target, nbPoints));
        // Rotate to 0 relative to world space
        transform.eulerAngles = new Vector3();
    }

    public void DestroySpillShape()
    {
        Destroy(gameObject);
    }

    public Vector2[] GetVertices(float width, Vector2 position, Vector2 target, int nbPoints)
    {
        Vector2 relativeTarget = target - position;
        List<Vector2> list = new List<Vector2>() {
            // Bottom Left
            new Vector2(relativeTarget.x - width, relativeTarget.y),
            // Top Left
             new Vector2(0 - width, 0),
            // Top Right
            new Vector2(0 + width, 0),
            // Bottom Right
            new Vector2(relativeTarget.x + width, relativeTarget.y),
        };
        return list.ToArray();
    }

    public void UpdateMesh(Vector2[] list)
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector2[] uv = new Vector2[list.Length];

        // Create the Vector3 vertices
        Vector3[] vertices = new Vector3[list.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector3(list[i].x, list[i].y);
            uv[i] = vertices[i];
        }

        // Use the triangulator to get indices for creating triangles
        Triangulator tr = new Triangulator(list);
        int[] indices = tr.Triangulate();
        // Update mesh
        mesh.Clear();
        MeshFilter filter = gameObject.GetComponent<MeshFilter>();
        filter.mesh = mesh;
        mesh.vertices = vertices;
        mesh.triangles = indices;
        mesh.uv = uv;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    public void GenerateMesh(Vector2[] listVertices, Material material)
    {
        Vector2[] list = listVertices;
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
        msh.uv = listVertices;
        msh.RecalculateNormals();
        msh.RecalculateBounds();

        gameObject.AddComponent(typeof(MeshRenderer));
        gameObject.AddComponent(typeof(EdgeCollider2D));
        gameObject.GetComponent<EdgeCollider2D>().points = listVertices;
        MeshFilter filter = gameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
        filter.mesh = msh;
        Renderer renderer = gameObject.GetComponent<MeshRenderer>();
        Material mat = new Material(material);
        renderer.material = mat;
    }
}
