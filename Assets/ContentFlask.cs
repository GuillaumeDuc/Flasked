using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ContentFlask : MonoBehaviour
{
    int nbPoints;
    float height;

    public void SetColor(Color color)
    {
        // Start filling
        gameObject.GetComponent<MeshRenderer>().material.SetColor("_color", color);
    }

    public void RemoveColor()
    {
        // Start spilling
    }

    public void UpdateContent()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector2[] list = GetListSquare(.01f, height, GetOrigin(), nbPoints);
        // Use the triangulator to get indices for creating triangles
        Triangulator tr = new Triangulator(list);
        int[] indices = tr.Triangulate();

        // Create the Vector3 vertices
        Vector3[] vertices = new Vector3[list.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector3(list[i].x, list[i].y);
        }

        // Update mesh
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = indices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        gameObject.GetComponent<EdgeCollider2D>().points = list;
    }

    Vector2 GetOrigin()
    {
        return transform.position;
    }

    public GameObject CreateContentFlask(float width, float height, Vector2 origin, Material material, int nbPoints)
    {
        this.height = height;
        return GenerateMesh(GetListSquare(width, height, origin, nbPoints), origin, material);
    }

    Vector2[] GetListSquare(float width, float height, Vector2 origin, int nbPoints)
    {
        Vector2[] left = StretchVectors(GetLeftVectors(width, height, nbPoints), Vector2.left, origin);
        Vector2[] right = StretchVectors(GetRightVectors(width, height, nbPoints), Vector2.right, origin);
        Vector2[] all = new Vector2[left.Length + right.Length];
        left.CopyTo(all, 0);
        right.CopyTo(all, right.Length);
        return all;
    }

    Vector2[] GetLeftVectors(float width, float height, int nbPoints)
    {
        Vector2[] res = new Vector2[nbPoints];
        // Bottom to up
        for (int i = 0; i < nbPoints; i++)
        {
            res[i] = new Vector2(-width, i * height / nbPoints);
        }
        return res;
    }

    Vector2[] GetRightVectors(float width, float height, int nbPoints)
    {
        Vector2[] res = new Vector2[nbPoints];
        int count = nbPoints - 1;
        // Up to bottom
        for (int i = 0; i < nbPoints; i++)
        {
            res[i] = new Vector2(width, count * height / nbPoints);
            count--;
        }
        return res;
    }

    Vector2[] StretchVectors(Vector2[] points, Vector2 direction, Vector2 origin)
    {
        Vector2[] res = new Vector2[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            Vector2 transformedPoint = points[i] + origin;
            RaycastHit2D hit = Physics2D.Raycast(transformedPoint, direction);
            if (hit.collider != null)
            {

                res[i] = res[i] + (hit.point - origin);
            }
        }
        return res;
    }

    public GameObject GenerateMesh(Vector2[] listVertices, Vector2 target, Material material)
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
        msh.RecalculateNormals();
        msh.RecalculateBounds();

        transform.position = target;
        gameObject.AddComponent(typeof(MeshRenderer));
        gameObject.AddComponent(typeof(EdgeCollider2D));
        gameObject.GetComponent<EdgeCollider2D>().points = listVertices;
        MeshFilter filter = gameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
        filter.mesh = msh;
        Renderer renderer = gameObject.GetComponent<MeshRenderer>();
        Material mat = new Material(material);
        renderer.material = mat;
        return gameObject;
    }

}
