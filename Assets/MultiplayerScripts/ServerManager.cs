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
    public int maxLv = 5;
    public GameObject flaskPrefab;
    public GameObject MultiplayerStorePrefab;
    public Text hostFlaskCurrentLvText;
    public Text clientFlaskCurrentLvText;
    public GameObject endPanelHost;
    public GameObject endPanelClient;
    public Button RetryHostButton;
    public Button RetryClientButton;
    private NetworkFlask selectedFlask;
    private List<List<List<Color>>> scenes = new List<List<List<Color>>>();
    private List<NetworkFlask> hostFlasks = new List<NetworkFlask>();
    private List<NetworkFlask> clientFlasks = new List<NetworkFlask>();
    private MultiplayerStore multiplayerStore;
    private List<(NetworkFlask, NetworkFlask)> listWaitingSpill = new List<(NetworkFlask, NetworkFlask)>();
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

        CreateFlasks(ref hostFlasks);
        CreateFlasks(ref clientFlasks, true);
    }

    void InitMultiplayerStore()
    {
        GameObject go = Instantiate(MultiplayerStorePrefab);
        go.GetComponent<NetworkObject>().Spawn();
        multiplayerStore = go.GetComponent<MultiplayerStore>();
        multiplayerStore.nbRetry.Value = nbRetry;
        multiplayerStore.nbUndo.Value = nbUndo;
    }

    void SaveLevel(List<List<List<Color>>> level, List<NetworkFlask> flasks)
    {
        level.Add(GetListFromFlasks(flasks));
    }

    List<List<Color>> GetListFromFlasks(List<NetworkFlask> flasks)
    {
        List<List<Color>> list = new List<List<Color>>();
        flasks.ForEach(flask =>
        {
            list.Add(new List<Color>(flask.GetColors()));
        });
        return list;
    }

    void CreateFlasks(ref List<NetworkFlask> list, bool isClient = false)
    {
        float offsetX = isClient ? .5f : 0;
        // Spawn flasks
        List<Flask> flasks = FlaskCreator.CreateFlasks(
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
        List<List<Color>> listColorFlasks = scenes[isClient ? multiplayerStore.clientLv.Value : multiplayerStore.hostLv.Value];
        // Fill client flasks
        FlaskCreator.RefillFlasks(flasks, listColorFlasks, contentHeight);
        list = flasks.ConvertAll(x => { return (NetworkFlask)x; });
        SpawnFlasks(list, isClient);
    }

    void SpawnFlasks(List<NetworkFlask> flasks, bool isClient = false)
    {
        // Spawn on server and Init Client flasks
        flasks.ForEach(flask =>
        {
            flask.GetComponent<NetworkObject>().Spawn();
            flask.InitFlaskClientRPC(flask.GetLayerContainer(), flask.GetMaxSize(), flask.GetColors().ToArray(), isClient);
            flask.isClientFlask = isClient;
        });
    }

    public bool SpillBottle(NetworkFlask giver, NetworkFlask receiver)
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

    public void AddToWaitingSpillList(NetworkFlask giver, NetworkFlask receiver)
    {
        listWaitingSpill.Add((giver, receiver));
    }

    List<List<List<Color>>> GetListScene()
    {
        List<List<List<Color>>> scenes = new List<List<List<Color>>>();
        for (int i = 0; i < maxLv; i++)
        {
            List<List<Color>> listColorFlasks = FlaskCreator.GetSolvedRandomFlasks(FlaskCreator.GetNbFlaskMultiplayer(i), nbContent, ref nbEmpty);
            scenes.Add(listColorFlasks);
        }
        return scenes;
    }

    void NextLevel(ref List<NetworkFlask> flasks, bool isClient)
    {
        // Make sure flask is clear before respawning client flasks
        flasks.ForEach(flask => flask.EmptyFlaskClientRPC());
        // Delete networked flasks
        FlaskCreator.DeleteFlasks(flasks);
        // Recreate and respawn flasks
        CreateFlasks(ref flasks, isClient);
    }

    void TryNextLevel(ref List<NetworkFlask> flasks, ref NetworkVariable<int> currentLv, bool isClient = false)
    {
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
                NextLevel(ref flasks, isClient);
            }
            else
            {
                if (isClient)
                {
                    endPanelClient.SetActive(true);
                    multiplayerStore.ClearedUIClientRPC(isClient);
                }
                else
                {
                    endPanelHost.SetActive(true);
                    multiplayerStore.ClearedUIClientRPC(isClient);
                }
            }
        }
    }

    public void RetryScene(bool isClient)
    {
        List<NetworkFlask> flasks = isClient ? clientFlasks : hostFlasks;
        int level = isClient ? multiplayerStore.clientLv.Value : multiplayerStore.hostLv.Value;

        // Refill flask on host
        FlaskCreator.RefillFlasks(flasks.ConvertAll(x => { return (Flask)x; }), scenes[level], contentHeight);
        // Refill flask on client
        flasks.ForEach(flask =>
        {
            flask.RefillFlaskClientRPC(flask.GetColors().ToArray());
        });
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
                NetworkFlask clickedFlask = hit.transform.gameObject.GetComponent<NetworkFlask>();
                bool spilled = false;
                bool canInteractFlask = false;
                // GameObject clicked is flask and is not moving
                if (clickedFlask != null && !clickedFlask.IsMoving())
                {
                    // Cannot interact with others host/client flask
                    canInteractFlask = NetworkManager.Singleton.IsHost ? !clickedFlask.isClientFlask : clickedFlask.isClientFlask;

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
                                TryNextLevel(ref hostFlasks, ref multiplayerStore.hostLv);
                                TryNextLevel(ref clientFlasks, ref multiplayerStore.clientLv, true);
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
                    TryNextLevel(ref hostFlasks, ref multiplayerStore.hostLv);
                    TryNextLevel(ref clientFlasks, ref multiplayerStore.clientLv, true);
                }
            }
        }
    }
}
