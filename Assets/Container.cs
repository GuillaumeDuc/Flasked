using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour
{
    public Material material;
    public float height = 2;
    public int nbPoints = 10;

    void Start()
    {
        /*
        Vector2 highestPoint = GetMaxHeight(GetComponent<EdgeCollider2D>().points);
        highestPoint.x = GetComponent<EdgeCollider2D>().bounds.center.x;
        // CreateMesh(new Vector2[] { new Vector2(highestPoint.x - .1f, highestPoint.y - .1f), new Vector2(highestPoint.x - .1f, highestPoint.y + .1f), new Vector2(highestPoint.x + .1f, highestPoint.y + .1f), new Vector2(highestPoint.x + .1f, highestPoint.y - .1f) });

        // Cast a ray straight down.
        RaycastHit2D hit = Physics2D.Raycast(highestPoint, Vector2.down);
        if (hit.collider != null)
        {
            GameObject go = GenerateMesh(GetListSquare(.1f, height, hit.point), hit.point);
            // Stretch(go);

            // StretchSides(go);
            // CreateMesh(new Vector2[] { new Vector2(), new Vector2(0, .1f), new Vector2(.1f, .1f), new Vector2(.1f, 0) }, hit.point);
        }
        // Cast a ray straight down.
        RaycastHit2D hit2 = Physics2D.Raycast(highestPoint, Vector2.down);
        if (hit.collider != null)
        {
            // GameObject go = GenerateMesh(GetListSquare(.1f, height), hit2.point);

            GenerateMesh(GetListSquare(.1f, height, hit2.point + new Vector2(0, .00001f)), hit2.point + new Vector2(0, .00001f));
            // CreateMesh(new Vector2[] { new Vector2(hit2.point.x - .1f, hit2.point.y - .1f), new Vector2(hit2.point.x - .1f, hit2.point.y + .1f), new Vector2(hit2.point.x + .1f, hit2.point.y + .1f), new Vector2(hit2.point.x + .1f, hit2.point.y - .1f) });

            // Debug.Log(hit.point);
        }*/
    }

    public ContentFlask AddContentFlask(float width, float height, Material material, int nbPoints)
    {
        // Set up game object with mesh
        GameObject meshObj = new GameObject("content");
        meshObj.transform.parent = gameObject.transform;
        Vector2 highestPoint = transform.TransformPoint(GetMaxHeight(GetComponent<EdgeCollider2D>().points));
        highestPoint.x = GetComponent<EdgeCollider2D>().bounds.center.x;
        RaycastHit2D hit = Physics2D.Raycast(highestPoint, Vector2.down);
        ContentFlask content = meshObj.AddComponent<ContentFlask>();
        if (hit.collider != null)
        {
            content.CreateContentFlask(width, height, hit.point + new Vector2(0, .00001f), material, nbPoints);
        }
        return content;
    }

    Vector2[] GetListSquare(float width, float height, Vector2 origin)
    {
        Vector2[] left = StretchVectors(GetLeftVectors(width, height), Vector2.left, origin);
        Vector2[] right = StretchVectors(GetRightVectors(width, height), Vector2.right, origin);
        Vector2[] all = new Vector2[left.Length + right.Length];
        left.CopyTo(all, 0);
        right.CopyTo(all, right.Length);
        return all;
    }

    Vector2[] GetLeftVectors(float width, float height)
    {
        Vector2[] res = new Vector2[nbPoints];
        // Bottom to up
        for (int i = 0; i < nbPoints; i++)
        {
            res[i] = new Vector2(-width, i * height / nbPoints);
        }
        return res;
    }

    Vector2[] GetRightVectors(float width, float height)
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

    Vector2 GetMaxHeight(Vector2[] points)
    {
        Vector2 maxHeight = points[0];
        for (int i = 0; i < points.Length; i++)
        {
            maxHeight = points[i].y > maxHeight.y ? points[i] : maxHeight;
        }
        return maxHeight;
    }

    Vector3[] Vector2ToVector3(Vector2[] list)
    {
        List<Vector3> res = new List<Vector3>();
        for (int i = 0; i < list.Length; i++)
        {
            res.Add(list[i]);
        }
        return res.ToArray();
    }

    public GameObject GenerateMesh(Vector2[] listVertices, Vector2 target)
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

        GameObject meshObj = new GameObject("content");
        meshObj.transform.parent = gameObject.transform;
        meshObj.transform.position = target;
        meshObj.AddComponent(typeof(MeshRenderer));
        meshObj.AddComponent(typeof(EdgeCollider2D));
        meshObj.GetComponent<EdgeCollider2D>().points = listVertices;
        MeshFilter filter = meshObj.AddComponent(typeof(MeshFilter)) as MeshFilter;
        filter.mesh = msh;
        Renderer renderer = meshObj.GetComponent<MeshRenderer>();
        Material mat = new Material(material);
        renderer.material = mat;
        return meshObj;
    }
}
