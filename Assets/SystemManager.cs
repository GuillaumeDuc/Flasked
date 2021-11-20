using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemManager : MonoBehaviour
{
    public List<Flask> flasks = new List<Flask>();
    public GameObject text;
    private List<Color> colors;
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
            text.SetActive(true);
    }
}
