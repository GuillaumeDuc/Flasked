using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour
{
    public ContentFlask AddContentFlask(float width, float height, Material material, int nbPoints)
    {
        // Set up game object with mesh
        GameObject meshObj = new GameObject("content");
        meshObj.transform.parent = gameObject.transform;
        meshObj.layer = 6;
        Vector2 highestPoint = transform.TransformPoint(GetMaxHeight(GetComponent<EdgeCollider2D>().points));
        highestPoint.x = GetComponent<EdgeCollider2D>().bounds.center.x;
        RaycastHit2D hit = Physics2D.Raycast(highestPoint, Vector2.down);
        ContentFlask content = meshObj.AddComponent<ContentFlask>();
        if (hit.collider != null)
        {
            content.CreateContentFlask(width, height, hit.point + new Vector2(0, 0.00001f), material, nbPoints);
        }
        return content;
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

    public ContentFlask GetContentAt(int index)
    {
        if (transform.childCount <= 0 || index > gameObject.transform.childCount - 1)
        {
            return null;
        }
        return gameObject.transform.GetChild(index).GetComponentInChildren<ContentFlask>();
    }
}
