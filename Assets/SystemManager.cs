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
            for (int i = 0; i < Random.Range(1, 4); i++)
            {
                flask.AddColor(colors[Random.Range(1, 4)]);
            }
        });
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
                if (clickedFlask != null)
                {
                    if (!clickedFlask.Equals(selectedFlask))
                    {
                        clickedFlask.SetSelected();
                    }
                    selectedFlask?.SetUnselected();
                }
                else
                {
                    selectedFlask?.SetUnselected();
                }
                // If clicked on the same flask two times, unselect
                selectedFlask = clickedFlask.Equals(selectedFlask) ? null : clickedFlask;
            }
            else
            { // No object selected, unselect
                selectedFlask?.SetUnselected();
                selectedFlask = null;
            }
        }
    }
}
