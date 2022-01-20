using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;


public class ServerManager : MonoBehaviour
{
    public int nbContent = 4;
    public float contentHeight = 1;
    public int nbEmpty = 1;
    public int nbRetry = 3;
    public int nbUndo = 5;
    public GameObject flaskPrefab;
    private List<Flask> flasks = new List<Flask>();
    private Flask selectedFlask;
    private Store Store;

    void Start()
    {
        // Load store from file
        Store = new Store();
        Store.FetchData();
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            StartButtons();
        }
        else 
        {
            StatusLabels();

            if (GUILayout.Button(NetworkManager.Singleton.IsServer ? "Generate" : "Nothing")) Init();
        }

        GUILayout.EndArea();
    }

    static void StartButtons()
    {
        if (GUILayout.Button("Host")) NetworkManager.Singleton.StartHost();
        if (GUILayout.Button("Client")) NetworkManager.Singleton.StartClient();
        if (GUILayout.Button("Server")) NetworkManager.Singleton.StartServer();
    }

        static void StatusLabels()
    {
        var mode = NetworkManager.Singleton.IsHost ?
            "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";

        GUILayout.Label("Transport: " +
            NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
        GUILayout.Label("Mode: " + mode);
    }


    void Init()
    {
        // Create flasks GameObjects
        flasks = FlaskCreator.CreateFlasks(flaskPrefab, 12, 4, 2, 1);
        List<List<Color>> listColorFlasks = FlaskCreator.GetSolvedRandomFlasks(flasks.Count, Store.nbContent, ref Store.nbEmptyFlask);
        // Fill flasks
        FlaskCreator.RefillFlask(flasks, listColorFlasks, contentHeight);
        // Spawn on server
        flasks.ForEach(flask => {
            flask.GetComponent<NetworkObject>().Spawn();
        });
    }

    bool SpillBottle(Flask giver, Flask receiver)
    {
        bool spilled = (giver != null) ? giver.SpillTo(receiver) : false;
        // If spilled, save current scene
        if (spilled)
        {
            Store.SaveCurrentScene(flasks);
            Store.SaveData();
        }
        return spilled;
    }

    void End()
    {
        bool end = true;
        flasks.ForEach(flask =>
        {
            if (!flask.IsCleared() && !flask.IsEmpty())
            {
                end = false;
            }
        });
        if (end)
        {
            Debug.Log("End");
        }
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
                // GameObject clicked is flask and is not moving
                if (clickedFlask != null && !clickedFlask.IsMoving())
                {
                    // Flask clicked is not already selected and selected flask is not filling
                    bool selectedIsFilling = selectedFlask == null ? false : selectedFlask.IsFilling();
                    if (!clickedFlask.Equals(selectedFlask) && !selectedIsFilling)
                    {
                        // Clicked flask, try to spill
                        spilled = SpillBottle(selectedFlask, clickedFlask);
                        // If not spilled, select
                        if (!spilled)
                        {
                            clickedFlask.SetSelected();
                        }
                        else
                        {
                            // Spilled, try to end
                            End();
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
