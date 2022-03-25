using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Linq;

public class ServerManager : MonoBehaviour
{
    public int nbContent = 4;
    public float contentHeight = 1;
    public int nbEmpty = 1;
    public int nbRetry = 3;
    public int nbUndo = 5;
    public int maxLv = 5;
    public GameObject flaskPrefab;
    public GameObject MultiplayerStorePrefab;
    public Text hostFlaskCurrentLvText;
    public Text clientFlaskCurrentLvText;
    public GameObject levelHost;
    public GameObject levelClient;
    public GameObject endPanelHost;
    public GameObject endPanelClient;
    public Button RetryHostButton;
    public Button RetryClientButton;
    private Flask selectedFlask;
    private List<Color[][]> scenes = new List<Color[][]>();
    public List<Flask> hostFlasks = new List<Flask>();
    public List<Flask> clientFlasks = new List<Flask>();
    private MultiplayerStore multiplayerStore;
    private List<(Flask, Flask)> listWaitingSpill = new List<(Flask, Flask)>();
    private bool clientClear = false, hostClear = false;
    float minX = .05f;
    float maxX = .48f;
    float xStep = .055f;
    float yStep = .4f;
    float maxHeight = .7f;
    float spillingYOffset = 2;
    float spillingXOffset = 1.75f;

    void Start()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            scenes = GetListScene();
            Init();
        }
    }

    void Init()
    {
        InitMultiplayerStore();

        CreateFlasks(ref hostFlasks, scenes[multiplayerStore.hostLv.Value]);
        CreateFlasks(ref clientFlasks, scenes[multiplayerStore.clientLv.Value], true);
        multiplayerStore.InitAllFlasksClientRPC(FlaskCreator.FlattenArray(scenes[multiplayerStore.hostLv.Value]), hostFlasks.Count, nbContent);
    }

    void InitMultiplayerStore()
    {
        GameObject go = Instantiate(MultiplayerStorePrefab);
        go.GetComponent<NetworkObject>().Spawn();
        multiplayerStore = go.GetComponent<MultiplayerStore>();
    }

    public void CreateFlasks(ref List<Flask> flasks, Color[][] colors, bool isClient = false)
    {
        float offsetX = isClient ? .5f : 0;
        // Spawn flasks
        flasks = FlaskCreator.CreateFlasks(
            flaskPrefab,
            FlaskCreator.GetNbFlaskMultiplayer(isClient ? multiplayerStore.clientLv.Value : multiplayerStore.hostLv.Value),
            nbContent,
            nbEmpty,
            contentHeight,
            offsetX + minX,
            offsetX + maxX,
            xStep,
            yStep,
            maxHeight,
            spillingYOffset,
            spillingXOffset
        );
        // Fill client flasks
        FlaskCreator.RefillFlasks(flasks, colors, contentHeight);
    }

    public bool SpillBottle(Flask giver, Flask receiver)
    {
        bool spilled = (giver != null) ? giver.SpillTo(receiver) : false;
        if (spilled)
        {
            if (NetworkManager.Singleton.IsHost)
            {
                multiplayerStore.SpillBottleClientRPC(giver.gameObject.GetComponent<NetworkObject>(), receiver.gameObject.GetComponent<NetworkObject>());
            }
            else if (NetworkManager.Singleton.IsClient)
            {
                multiplayerStore.SpillBottleServerRPC(giver.gameObject.GetComponent<NetworkObject>(), receiver.gameObject.GetComponent<NetworkObject>());
            }
        }

        return spilled;
    }

    public void AddToWaitingSpillList(Flask giver, Flask receiver)
    {
        listWaitingSpill.Add((giver, receiver));
    }

    List<Color[][]> GetListScene()
    {
        List<Color[][]> scenes = new List<Color[][]>();
        for (int i = 0; i < maxLv; i++)
        {
            List<List<Color>> listColorFlasks = FlaskCreator.GetSolvedRandomFlasks(FlaskCreator.GetNbFlaskMultiplayer(i), nbContent, ref nbEmpty);
            Color[][] colors = new Color[listColorFlasks.Count][];
            for (int j = 0; j < listColorFlasks.Count; j++)
            {
                colors[j] = listColorFlasks[j].ToArray();
            }
            scenes.Add(colors);
        }
        return scenes;
    }

    void NextLevel(List<Flask> flasks, bool isClient)
    {
        // Delete networked flasks
        FlaskCreator.DeleteFlasks(flasks);
        // Recreate and respawn flasks
        Color[][] colorsScene = isClient ? scenes[multiplayerStore.clientLv.Value] : scenes[multiplayerStore.hostLv.Value];
        CreateFlasks(ref flasks, colorsScene, isClient);
        int nbFlasks = isClient ? clientFlasks.Count : hostFlasks.Count;
        multiplayerStore.CreateFlasksClientRPC(FlaskCreator.FlattenArray(colorsScene), nbFlasks, nbContent, isClient);
    }

    void TryNextLevel(ref NetworkVariable<int> currentLv, bool isClient = false)
    {
        List<Flask> flasks = isClient ? clientFlasks : hostFlasks;
        bool cleared = true;
        flasks.ForEach(flask =>
        {
            if (!flask.IsCleared() && !flask.IsEmpty())
            {
                cleared = false;
            }
        });
        if (cleared)
        {
            currentLv.Value += 1;
            if (currentLv.Value < scenes.Count)
            {
                selectedFlask = null;
                NextLevel(flasks, isClient);
            }
            else
            {
                if (isClient)
                {
                    clientClear = true;

                    levelClient.SetActive(false);
                    clientFlaskCurrentLvText.gameObject.SetActive(false);
                    endPanelClient.SetActive(true);
                    multiplayerStore.ClearedUIClientRPC(isClient);
                }
                else
                {
                    hostClear = true;

                    levelHost.SetActive(false);
                    hostFlaskCurrentLvText.gameObject.SetActive(false);
                    endPanelHost.SetActive(true);
                    multiplayerStore.ClearedUIClientRPC(isClient);
                }
            }
        }
    }

    public void RetryScene(bool isClient)
    {
        List<Flask> flasks = isClient ? clientFlasks : hostFlasks;
        int level = isClient ? multiplayerStore.clientLv.Value : multiplayerStore.hostLv.Value;

        // Refill flask on host
        FlaskCreator.RefillFlasks(flasks, scenes[level], contentHeight);
        // Refill flask on client
        multiplayerStore.CreateFlasksClientRPC(FlaskCreator.FlattenArray(scenes[level]), flasks.Count, nbContent, isClient);
        // Reset selected flask
        selectedFlask = null;
    }

    public void SetMultiplayerStore(MultiplayerStore multiplayerStore)
    {
        this.multiplayerStore = multiplayerStore;
    }

    void Update()
    {
        // Player moves
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100))
            {
                Flask clickedFlask = hit.transform.gameObject.GetComponent<Flask>();
                bool spilled = false;
                bool canInteractFlask = false;
                // GameObject clicked is flask and is not moving
                if (clickedFlask != null && !clickedFlask.IsMoving())
                {
                    // Cannot interact with others host/client flask
                    canInteractFlask = NetworkManager.Singleton.IsHost ? hostFlasks.Contains(clickedFlask) : clientFlasks.Contains(clickedFlask);

                    // Flask clicked is not already selected and selected flask is not filling
                    bool selectedIsFilling = selectedFlask == null ? false : selectedFlask.IsFilling();

                    if (!clickedFlask.Equals(selectedFlask) && !selectedIsFilling && canInteractFlask)
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
                            if (NetworkManager.Singleton.IsHost)
                            {
                                // Spilled, try to go to next scene
                                if (!hostClear)
                                {
                                    TryNextLevel(ref multiplayerStore.hostLv);
                                }
                                if (!clientClear)
                                {
                                    TryNextLevel(ref multiplayerStore.clientLv, true);
                                }
                            }
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
                // If clicked on a non selectable flask, unselect
                // If spilled, unselect
                if (spilled || clickedFlask.Equals(selectedFlask) || !canInteractFlask)
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

        // Try to play other player moves
        if (listWaitingSpill.Count != 0)
        {
            Flask giver = listWaitingSpill[listWaitingSpill.Count - 1].Item1.GetComponent<Flask>();
            Flask receiver = listWaitingSpill[listWaitingSpill.Count - 1].Item2.GetComponent<Flask>();

            bool spilled = false;
            // Try to spill
            if (giver.CanSpill(receiver))
            {
                spilled = giver.SpillTo(receiver);
            }

            if (spilled)
            {
                listWaitingSpill.RemoveAt(listWaitingSpill.Count - 1);
                if (NetworkManager.Singleton.IsHost)
                {
                    // Spilled, try to go to next scene
                    if (!hostClear)
                    {
                        TryNextLevel(ref multiplayerStore.hostLv);
                    }
                    if (!clientClear)
                    {
                        TryNextLevel(ref multiplayerStore.clientLv, true);
                    }
                }
            }
        }
    }
}
