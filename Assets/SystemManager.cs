using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemManager : MonoBehaviour
{
    public List<Flask> flasks = new List<Flask>();
    private List<Color> colors;
    private Flask selectedFlask;
    void Start()
    {
        // Initialize colors
        colors = new List<Color>() {
            Color.cyan,
            Color.red,
            Color.green,
            Color.yellow,
        };
        // Initialize all flask
        flasks.ForEach(flask =>
        {
            flask.InitFlask();
            for (int i = 0; i < Random.Range(0, 5); i++)
            {
                flask.AddColor(colors[Random.Range(0, 4)]);
            }
        });
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