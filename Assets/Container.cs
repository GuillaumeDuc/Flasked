using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour
{
    public ContentFlask AddContentFlask(float width, float height, Color color, Material material, int nbPoints)
    {
        // If top content is same color, increase height and return existing content
        if (GetTopContent() != null && GetTopContent().HasSameColor(color))
        {
            GetTopContent().height += height;
            GetTopContent().nbPoints += nbPoints;
            GetTopContent().SetCurrentHeight(GetTopContent().height);
            GetTopContent().UpdateContent();
            return GetTopContent();
        }
        bool isFirstContent = !HasContent();
        // Set up game object with position
        GameObject meshObj = new GameObject("content");
        meshObj.transform.parent = gameObject.transform;
        meshObj.layer = 6;
        Bounds bounds = GetComponent<EdgeCollider2D>().bounds;
        Vector2 highestPoint = new Vector2(bounds.center.x, bounds.center.y + bounds.extents.y);
        RaycastHit2D hit = Physics2D.Raycast(highestPoint, Vector2.down);
        ContentFlask content = meshObj.AddComponent<ContentFlask>();
        if (hit.collider != null)
        {
            content.CreateContentFlask(width, height, hit.point + new Vector2(0, 0.0001f), color, material, nbPoints, isFirstContent);
        }
        return content;
    }

    bool HasContent()
    {
        return GetTopContent() != null;
    }

    public ContentFlask[] GetContents()
    {
        return GetComponentsInChildren<ContentFlask>();
    }

    public ContentFlask GetTopContent()
    {
        return GetContentAt(transform.childCount - 1);
    }

    public ContentFlask GetContentAt(int index)
    {
        if (transform.childCount <= 0 || index > gameObject.transform.childCount - 1)
        {
            return null;
        }
        return gameObject.transform.GetChild(index).GetComponentInChildren<ContentFlask>();
    }

    public void UpdateContents(float angle = 0, float time = 0)
    {
        ContentFlask[] contents = GetComponentsInChildren<ContentFlask>();
        for (int i = 0; i < contents.Length; i++)
        {
            contents[i].UpdateContent(angle, time);
        }
    }

    public void ClearContents()
    {
        ContentFlask[] contents = GetComponentsInChildren<ContentFlask>();
        for (int i = 0; i < contents.Length; i++)
        {
            DestroyImmediate(contents[i].gameObject);
        }
    }
}
