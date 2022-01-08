using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class Solver
{
    public static bool Solve(List<Flask> listFlask)
    {
        List<ObjectFlask> objectFlasks = CreateList(listFlask);
        bool stop = false;
        Node root = new Node();
        ConstructTree(objectFlasks, listFlask.Count * listFlask.Count, 0, ref stop, root);
        int size = 0;
        dfs(root, ref size);
        Debug.Log("Size " + size);
        Node node = new Node();
        GetClearedNode(root, ref node);
        // PrintSolution(node);
        return node.cleared;
    }

    private static void ConstructTree(List<ObjectFlask> list, int maxDepth, int depth, ref bool end, Node current)
    {
        if (depth < maxDepth && !end)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (end)
                {
                    break;
                }

                List<ObjectFlask> newList = CreateList(list);
                ObjectFlask selectedFlask = newList[i];

                // Ignore empty selectedFlask or cleared selected flask or flask containing only one color
                if (!selectedFlask.IsEmpty() && !selectedFlask.IsCleared())
                {
                    ObjectFlask targetFlask = GetBestSpillableFlask(selectedFlask, newList);
                    // If target flask is empty and selected flask is only one color do nothing
                    if (targetFlask != null && !(targetFlask.IsEmpty() && selectedFlask.HasOneColor()))
                    {
                        // Spill
                        selectedFlask.SpillTo(targetFlask);

                        Node nextNode = new Node(selectedFlask, targetFlask);
                        current.AddNext(nextNode);

                        if (ClearedScene(newList))
                        {
                            nextNode.cleared = true;
                            end = true;
                        }
                        if (!end)
                        {
                            ConstructTree(newList, maxDepth, depth + 1, ref end, nextNode);
                        }
                    }
                }
            }
        }
    }

    static void PrintSolution(Node clearedNode)
    {
        List<Node> clearedList = new List<Node>();
        Node current = clearedNode;
        while (current.previous != null)
        {
            clearedList.Add(current);
            current = current.previous;
        }
        clearedList.Reverse();
        Debug.Log("----- Solver -----");
        clearedList.ForEach(node =>
        {
            Debug.Log("selected : " + node.selectedFlask.position + " target : " + node.targetFlask.position);
        });
    }

    static void dfs(Node root, ref int size)
    {
        for (int i = 0; i < root.next.Count; i++)
        {
            size += 1;
            dfs(root.next[i], ref size);
        }
    }

    static void GetClearedNode(Node current, ref Node cleared)
    {
        if (current.cleared)
        {
            cleared = current;
        }

        for (int i = 0; i < current.next.Count; i++)
        {
            GetClearedNode(current.next[i], ref cleared);
        }
    }

    private static ObjectFlask GetBestSpillableFlask(ObjectFlask flask, List<ObjectFlask> list)
    {
        int maxColor = -1;
        ObjectFlask maxFlask = null;
        list.FindAll(f => flask.CanSpill(f, flask.PopColors())).ForEach(f =>
        {
            if (f.PopColors().Count > maxColor)
            {
                maxColor = f.PopColors().Count;
                maxFlask = f;
            }
        });
        return maxFlask;
    }

    static bool ClearedScene(List<ObjectFlask> list)
    {
        bool cleared = true;
        list.ForEach(flask =>
        {
            if (!flask.IsCleared() && !flask.IsEmpty())
            {
                cleared = false;
            }
        });
        return cleared;
    }

    private static List<ObjectFlask> CreateList(List<Flask> flasks)
    {
        List<ObjectFlask> newList = new List<ObjectFlask>();
        int i = 0;
        flasks.ForEach(flask =>
        {
            ObjectFlask newFlask = new ObjectFlask(flask);
            newFlask.position = i;
            newList.Add(newFlask);
            i++;
        });
        return newList;
    }

    private static List<ObjectFlask> CreateList(List<ObjectFlask> flasks)
    {
        List<ObjectFlask> newList = new List<ObjectFlask>();
        flasks.ForEach(flask =>
        {
            newList.Add(new ObjectFlask(flask));
        });
        return newList;
    }
}

public class Node
{
    public bool cleared = false;
    public Node previous = null;
    public List<Node> next = new List<Node>();
    public ObjectFlask selectedFlask = null;
    public ObjectFlask targetFlask = null;
    public Node() { }

    public Node(ObjectFlask selectedFlask, ObjectFlask targetFlask)
    {
        this.selectedFlask = selectedFlask;
        this.targetFlask = targetFlask;
    }

    public void AddNext(Node node)
    {
        this.next.Add(node);
        node.previous = this;
    }

    public override string ToString()
    {
        return "SELECTED " + selectedFlask?.position + "\nSPILL TO " + targetFlask?.position;
    }
}

public class ObjectFlask
{
    public List<Color> colors = new List<Color>();
    public int maxSize;
    public int position;
    public ObjectFlask nextFlask, prevFlask;

    public ObjectFlask(ObjectFlask objectFlask)
    {
        this.colors = new List<Color>(objectFlask.colors);
        this.maxSize = objectFlask.maxSize;
        this.position = objectFlask.position;
    }

    public ObjectFlask(Flask flask)
    {
        this.colors = new List<Color>(flask.GetColors());
        this.maxSize = flask.GetMaxSize();
    }

    private bool HasEnoughSpace(ObjectFlask flask, List<Color> colorSpill)
    {
        int size = flask.colors.Count + colorSpill.Count;
        return flask.maxSize >= size;
    }

    public bool EqualsTopColor(ObjectFlask flask)
    {
        // Empty flask return true
        if (flask.colors.Count == 0)
        {
            return true;
        }
        if (colors.Count > 0 && flask.colors.Count > 0 && flask != null)
        {
            // Top color this flask is equal top color reference flask
            return this.colors[colors.Count - 1].Equals(flask.colors[flask.colors.Count - 1]);
        }
        return false;
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

    public bool CanSpill(ObjectFlask flask, List<Color> colorsToSpill)
    {
        return !this.Equals(flask) && flask != null && HasEnoughSpace(flask, colorsToSpill) && EqualsTopColor(flask) && !IsEmpty();
    }

    public bool SpillTo(ObjectFlask flask)
    {
        List<Color> colorsToSpill = this.PopColors();
        // Spill only when not in own flask, if flask is not null, if space is enough, if both top color match
        if (CanSpill(flask, colorsToSpill))
        {
            RemoveColors(colorsToSpill.Count);
            colorsToSpill.ForEach(color => { flask.colors.Add(color); });
            return true;
        }
        return false;
    }

    public bool IsCleared()
    {
        if (colors.Count == 0)
        {
            return false;
        }
        return HasOneColor() && colors.Count == maxSize;
    }

    public bool HasOneColor()
    {
        bool sameColor = true;
        Color color = colors[0];
        colors.ForEach(c =>
        {
            if (!c.Equals(color))
            {
                sameColor = false;
            }
        });
        return sameColor;
    }

    public bool IsEmpty()
    {
        return colors.Count == 0;
    }

    public override bool Equals(System.Object obj)
    {
        //Check for null and compare run-time types.
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
        {
            return false;
        }
        else
        {
            ObjectFlask o = (ObjectFlask)obj;
            return this.position == o.position;
        }
    }

    public override int GetHashCode()
    {
        return (position << 2);
    }

}