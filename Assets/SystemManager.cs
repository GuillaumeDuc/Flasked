using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemManager : MonoBehaviour
{
    public int nbContent = 4;
    public int nbEmpty = 2;
    public List<Flask> flasks = new List<Flask>();
    private List<Color> colors;
    private Flask selectedFlask;
    void Start()
    {
        bool solved;
        int tentative = 1;
        do
        {
            InitFlasks();
            solved = Solver.Solve(flasks);
            tentative += 1;
        } while (!solved);
        Debug.Log(tentative);
    }

    void InitFlasks()
    {
        List<Color> colorList = GetColorFullContent(nbEmpty);
        // Loop through all flask
        for (int i = 0; i < flasks.Count; i++)
        {
            flasks[i].InitFlask(7 + i);
            if (i < flasks.Count - nbEmpty)
            {
                // Fill until it reaches top
                for (int j = 0; j < nbContent; j++)
                {
                    int colorIndex = Random.Range(0, colorList.Count);
                    Color color = colorList[colorIndex];
                    colorList.RemoveAt(colorIndex);
                    flasks[i].AddColor(color, flasks[i].contentHeight);
                }
            }
        }
    }

    List<Color> GetColorFullContent(int nbEmpty)
    {
        List<Color> colorList = new List<Color>();
        // Fill list color
        for (int i = 0; i < (flasks.Count - nbEmpty); i++)
        {
            for (int j = 0; j < nbContent; j++)
            {
                colorList.Add(GetColors()[i]);
            }
        }
        return colorList;
    }

    List<Color> GetColors()
    {
        return new List<Color>() {
            Color.cyan,
            Color.red,
            Color.green,
            Color.yellow,
            Color.white,
            Color.magenta,
            Color.gray
        };
    }

    bool SpillBottle(Flask giver, Flask receiver)
    {
        bool spilled = (giver != null) ? giver.SpillTo(receiver) : false;
        return spilled;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100))
            {
                Flask clickedFlask = hit.transform.gameObject.GetComponent<Flask>();
                bool spilled = false;
                // GameObject clicked is flask
                if (clickedFlask != null)
                {
                    if (!clickedFlask.Equals(selectedFlask))
                    {
                        // Clicked flask, try to spill
                        spilled = SpillBottle(selectedFlask, clickedFlask);
                        // If not spilled, select
                        if (!spilled)
                        {
                            clickedFlask.SetSelected();
                        }
                    }
                    selectedFlask?.SetUnselected();
                }
                else // GameObject clicked is not flask
                {
                    selectedFlask?.SetUnselected();
                    selectedFlask = null;
                }
                // Change selected flask to new clicked flask
                // If clicked on the same flask two times, unselect
                // If spilled, unselect
                if (spilled || clickedFlask.Equals(selectedFlask))
                {
                    selectedFlask = null;
                }
                else
                {
                    selectedFlask = clickedFlask;
                }
            }
            else
            { // No object selected, unselect
                selectedFlask?.SetUnselected();
                selectedFlask = null;
            }
        }
    }
}
