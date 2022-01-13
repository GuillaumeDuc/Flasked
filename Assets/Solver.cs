using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class Solver
{
    public static bool Solve(List<List<Color>> listFlask, int nbContent)
    {
        List<ObjectFlask> objectFlasks = CreateList(listFlask, nbContent);
        bool stop = false;
        Node root = new Node();
        List<List<ObjectFlask>> visited = new List<List<ObjectFlask>>() { objectFlasks };

        // NEED TO OPTIMIZE THAT
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        float maxTimeS = listFlask.Count * 10;
        ConstructTree(objectFlasks, 0, ref stop, root, visited, sw, maxTimeS);

        Debug.Log("visited size " + visited.Count);
        Node node = new Node();
        GetClearedNode(root, ref node);
        Debug.Log(node.cleared);
        // PrintSolution(node);
        return node.cleared;
    }

    private static void ConstructTree(List<ObjectFlask> list, int depth, ref bool end, Node current, List<List<ObjectFlask>> visited, System.Diagnostics.Stopwatch sw, float maxTimeS)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (end)
            {
                break;
            }
            for (int j = 0; j < list.Count; j++)
            {
                if (end)
                {
                    break;
                }

                if (list[i].IsCleared())
                {
                    break;
                }

                if (list[i].IsEmpty())
                {
                    break;
                }

                // If target flask can spill and move is not useless
                if (list[i].CanSpill(list[j]) && !(list[i].HasOneColor() && list[j].IsEmpty()))
                {
                    List<ObjectFlask> newList = CreateList(list);
                    ObjectFlask selectedFlask = newList[i];
                    ObjectFlask targetFlask = newList[j];

                    // Spill
                    selectedFlask.SpillTo(targetFlask);
                    if (!ExistInList(visited, newList))
                    {
                        // TODO : REMOVE THAT
                        if (sw.Elapsed > System.TimeSpan.FromSeconds(maxTimeS))
                        {
                            end = true;
                        }

                        Node nextNode = new Node(selectedFlask, targetFlask);
                        current.AddNext(nextNode);

                        if (ClearedScene(newList))
                        {
                            nextNode.cleared = true;
                            end = true;
                        }

                        if (!end)
                        {
                            visited.Add(newList);
                            // Debug.Log("depth " + depth);
                            ConstructTree(newList, depth + 1, ref end, nextNode, visited, sw, maxTimeS);
                        }
                    }
                }
            }
        }
    }

    static bool ExistInList(List<List<ObjectFlask>> list, List<ObjectFlask> newList)
    {
        bool found = false;
        if (list.Count == 0)
        {
            return found;
        }
        int i = 0;
        while (!found && i < list.Count)
        {
            if (IsSameList(list[i], newList))
            {
                found = true;
            }
            i += 1;
        }
        return found;
    }

    static bool IsSameList(List<ObjectFlask> list1, List<ObjectFlask> list2)
    {
        bool equal = true;
        int i = 0;
        if (list1.Count == 0 || list2.Count == 0)
        {
            return false;
        }

        if (list1.Count != list2.Count)
        {
            return false;
        }

        while (equal && i < list1.Count)
        {
            if (!list1[i].ContainsSameColor(list2[i]))
            {
                equal = false;
            }
            i += 1;
        }
        return equal;
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

    static bool ClearedScene(List<ObjectFlask> list)
    {
        bool cleared = true;
        int i = 0;
        while (cleared && i < list.Count)
        {
            if (!list[i].IsCleared() && !list[i].IsEmpty())
            {
                cleared = false;
            }
            i += 1;
        }
        return cleared;
    }

    private static List<ObjectFlask> CreateList(List<List<Color>> flasks, int maxSize)
    {
        List<ObjectFlask> newList = new List<ObjectFlask>();
        int i = 0;
        flasks.ForEach(flask =>
        {
            ObjectFlask newFlask = new ObjectFlask(flask, maxSize);
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

    public ObjectFlask(List<Color> flask, int maxSize)
    {
        this.colors = new List<Color>(flask);
        this.maxSize = maxSize;
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

    public bool CanSpill(ObjectFlask flask)
    {
        return !this.Equals(flask) && flask != null && HasEnoughSpace(flask, this.PopColors()) && EqualsTopColor(flask) && !IsEmpty();
    }

    public void SpillTo(ObjectFlask flask)
    {
        List<Color> colorsToSpill = this.PopColors();
        RemoveColors(colorsToSpill.Count);
        colorsToSpill.ForEach(color => { flask.colors.Add(color); });
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

    public bool ContainsSameColor(ObjectFlask objectFlask)
    {
        bool containsSameColor = true;
        int i = 0;
        if (objectFlask.colors.Count == 0 && this.colors.Count == 0)
        {
            return true;
        }

        if (objectFlask.colors.Count == 0 || this.colors.Count == 0)
        {
            return false;
        }

        if (objectFlask.colors.Count != this.colors.Count)
        {
            return false;
        }

        while (containsSameColor && i < this.colors.Count)
        {
            if (!objectFlask.colors[i].Equals(this.colors[i]))
            {
                containsSameColor = false;
            }
            i += 1;
        }
        return containsSameColor;
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