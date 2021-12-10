using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ContentFlask : MonoBehaviour
{
    public int nbPoints;
    public float height;
    public float currentHeight;
    public bool fill;
    public bool firstContent = false;

    public void SetColor(Color color)
    {
        // Start filling
        float intensity = 7;
        float factor = Mathf.Pow(2, intensity);
        Color newColor = new Color(color.r * factor, color.g * factor, color.b * factor);
        gameObject.GetComponent<MeshRenderer>().material.SetColor("_color", color);
    }

    public bool HasSameColor(Color color)
    {
        return GetMaterial().GetColor("_color") == color;
    }

    public Color GetColor()
    {
        return GetMaterial().GetColor("_color");
    }

    public Material GetMaterial()
    {
        return gameObject.GetComponent<MeshRenderer>().material;
    }

    public void SetCurrentHeight(float height)
    {
        this.currentHeight = height;
    }

    public void UpdateContent(float eulerAngle = 0, float time = 0)
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector2[] list = GetListSquare(.01f, GetOrigin(), nbPoints, eulerAngle, time);
        Vector2[] uv = new Vector2[list.Length];

        // Create the Vector3 vertices
        Vector3[] vertices = new Vector3[list.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            list[i] = Quaternion.Euler(0, 0, -eulerAngle) * list[i];
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

        // Collider
        EdgeCollider2D edge = GetComponent<EdgeCollider2D>();
        if (edge != null)
        {
            edge.SetPoints(new List<Vector2>(list));
        }
    }

    Vector2 GetOrigin()
    {
        return transform.position;
    }

    public GameObject CreateContentFlask(float width, float height, Vector2 origin, Color color, Material material, int nbPoints, bool firstContent)
    {
        this.height = height;
        this.currentHeight = height;
        this.nbPoints = nbPoints;
        this.firstContent = firstContent;
        return GenerateMesh(GetListSquare(width, origin, nbPoints), origin, color, material);
    }

    Vector2[] GetListSquare(float width, Vector2 origin, int nbPoints, float eulerAngle = 0, float time = 0)
    {
        Vector2[] left = StretchVectors(GetLeftVectors(width, nbPoints), Vector2.left, origin, eulerAngle);
        Vector2[] right = StretchVectors(GetRightVectors(width, nbPoints), Vector2.right, origin, eulerAngle, true);
        Vector2[] top = GetTopVectors(left[left.Length - 1].x, right[0].x, left[left.Length - 1].y, nbPoints, time);
        List<Vector2> all = new List<Vector2>(left);
        all.AddRange(top);
        all.AddRange(right);
        return all.ToArray();
    }

    Vector2[] GetLeftVectors(float width, int nbPoints)
    {
        Vector2[] res = new Vector2[nbPoints];
        // Bottom to up
        for (int i = 0; i < nbPoints; i++)
        {
            res[i] = new Vector2(-width, i * currentHeight / nbPoints);
        }
        return res;
    }

    Vector2[] GetRightVectors(float width, int nbPoints)
    {
        Vector2[] res = new Vector2[nbPoints];
        int count = nbPoints - 1;
        // Up to bottom
        for (int i = 0; i < nbPoints; i++)
        {
            res[i] = new Vector2(width, count * currentHeight / nbPoints);
            count--;
        }
        return res;
    }

    Vector2[] GetTopVectors(float start, float end, float height, int nbPoints, float time)
    {
        float step = (end - start) / (nbPoints - 1);
        float current = start + step;
        List<Vector2> res = new List<Vector2>();
        while (current < end)
        {
            float transformedHeight = height;
            if (fill)
            {
                transformedHeight = transformedHeight + (Mathf.Sin(3f * (current + transformedHeight) * time) / 10);
            }
            res.Add(new Vector2(current, transformedHeight));
            current += step;
        }
        return res.ToArray();
    }

    Vector2[] StretchVectors(Vector2[] points, Vector2 direction, Vector2 origin, float eulerAngle, bool ignoreLast = false)
    {
        int layerMask = GetContentLayerMask();

        Vector2[] res = new Vector2[points.Length];
        for (int i = 0; i < points.Length; i++)
        {

            Vector2 transformedPoint = (points[i] + origin);
            transformedPoint = RotatePointAroundPivot(transformedPoint, origin, new Vector3(0, 0, eulerAngle));
            RaycastHit2D hit;
            // Raycast direction change to fill bottom content of first content
            if (firstContent && ignoreLast && i > points.Length / 2)
            {
                hit = Physics2D.Raycast(transformedPoint, Quaternion.Euler(0, 0, eulerAngle) * direction, 100, layerMask);
            }
            else
            {
                hit = Physics2D.Raycast(transformedPoint, direction, 100, layerMask);
            }
            if (hit.collider != null)
            {
                res[i] = (hit.point - origin);
            }
        }
        return res;
    }

    Vector2 RotatePointAroundPivot(Vector2 point, Vector2 pivot, Vector3 angles)
    {
        Vector2 dir = point - pivot; // get point direction relative to pivot
        dir = Quaternion.Euler(angles) * dir; // rotate it
        point = dir + pivot; // calculate rotated point
        return point; // return it
    }

    int GetContentLayerMask()
    {
        return 1 << GetComponentInParent<Container>().gameObject.layer;
        // int layerMask = 1 << 6;
        // return ~layerMask;
    }

    public GameObject GenerateMesh(Vector2[] listVertices, Vector2 target, Color color, Material material)
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

        transform.position = target;
        gameObject.AddComponent(typeof(MeshRenderer));
        gameObject.AddComponent(typeof(EdgeCollider2D));
        gameObject.GetComponent<EdgeCollider2D>().points = listVertices;
        MeshFilter filter = gameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
        filter.mesh = msh;
        Renderer renderer = gameObject.GetComponent<MeshRenderer>();
        Material mat = new Material(material);
        renderer.material = mat;
        SetColor(color);
        return gameObject;
    }
}
