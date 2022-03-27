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
    public List<Player> players = new List<Player>();
    private MultiplayerStore multiplayerStore;
    private List<(int, int, int)> listWaitingSpill = new List<(int, int, int)>();
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
        // Init store
        InitMultiplayerStore();
        // Init players
        for (int i = 0; i < NetworkManager.Singleton.ConnectedClientsIds.Count; i++)
        {
            Player p = new Player(NetworkManager.Singleton.ConnectedClientsIds[i]);
            players.Add(p);
            CreateFlasks(ref p.flasks, scenes[multiplayerStore.hostLv.Value], i);

            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { NetworkManager.Singleton.ConnectedClientsIds[i] }
                }
            };

            multiplayerStore.InitAllFlasksClientRPC(
                FlaskCreator.FlattenArray(scenes[multiplayerStore.hostLv.Value]),
                p.flasks.Count,
                nbContent,
                i,
                NetworkManager.Singleton.ConnectedClientsIds.Count,
                clientRpcParams
            );
        }
    }

    void InitMultiplayerStore()
    {
        GameObject go = Instantiate(MultiplayerStorePrefab);
        go.GetComponent<NetworkObject>().Spawn();
        multiplayerStore = go.GetComponent<MultiplayerStore>();
    }

    public void CreateFlasks(ref List<Flask> flasks, Color[][] colors, int nbCLient)
    {
        float offsetX = .5f * nbCLient;
        // Spawn flasks
        flasks = FlaskCreator.CreateFlasks(
            flaskPrefab,
            FlaskCreator.GetNbFlaskMultiplayer(nbCLient != 0 ? multiplayerStore.clientLv.Value : multiplayerStore.hostLv.Value),
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

    public int GetPosFlaskServer(Flask flask)
    {
        ulong playerId = NetworkManager.Singleton.LocalClientId;
        Player p = players.Find(player => player.playerId == playerId);
        return p.flasks.FindIndex(curr => curr == flask);
    }

    public int GetPosFlaskClient(Flask flask)
    {
        Player p = players[multiplayerStore.posClient];
        return p.flasks.FindIndex(curr => curr == flask);
    }

    public int GetPosPlayerServer(ulong playerId)
    {
        return players.FindIndex(player => player.playerId == playerId);
    }

    public bool SpillBottle(Flask giver, Flask receiver)
    {
        bool spilled = (giver != null) ? giver.SpillTo(receiver) : false;
        if (spilled)
        {
            if (NetworkManager.Singleton.IsHost)
            {
                multiplayerStore.SpillBottleClientRPC(
                    GetPosFlaskServer(giver),
                    GetPosFlaskServer(receiver),
                    GetPosPlayerServer(NetworkManager.Singleton.LocalClientId)
                );
            }
            else if (NetworkManager.Singleton.IsClient)
            {
                multiplayerStore.SpillBottleServerRPC(
                    GetPosFlaskClient(giver),
                    GetPosFlaskClient(receiver),
                    multiplayerStore.posClient
                );
            }
        }

        return spilled;
    }

    public void AddToWaitingSpillList(int giver, int receiver, int posPlayer)
    {
        listWaitingSpill.Add((giver, receiver, posPlayer));
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

    void NextLevel(List<Flask> flasks, int posPlayer)
    {
        // Delete networked flasks
        FlaskCreator.DeleteFlasks(flasks);
        // Recreate and respawn flasks
        Color[][] colorsScene = posPlayer != 0 ? scenes[multiplayerStore.clientLv.Value] : scenes[multiplayerStore.hostLv.Value];
        CreateFlasks(ref flasks, colorsScene, posPlayer);
        int nbFlasks = players[posPlayer].flasks.Count;
        multiplayerStore.CreateFlasksClientRPC(FlaskCreator.FlattenArray(colorsScene), nbFlasks, nbContent, posPlayer);
    }

    void TryNextLevel(ref NetworkVariable<int> currentLv, int posPlayer)
    {
        List<Flask> flasks = players[posPlayer].flasks;
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
                NextLevel(flasks, posPlayer);
            }
            else
            {
                if (posPlayer != 0)
                {
                    clientClear = true;

                    levelClient.SetActive(false);
                    clientFlaskCurrentLvText.gameObject.SetActive(false);
                    endPanelClient.SetActive(true);
                    multiplayerStore.ClearedUIClientRPC(posPlayer != 0);
                }
                else
                {
                    hostClear = true;

                    levelHost.SetActive(false);
                    hostFlaskCurrentLvText.gameObject.SetActive(false);
                    endPanelHost.SetActive(true);
                    multiplayerStore.ClearedUIClientRPC(posPlayer != 0);
                }
            }
        }
    }

    public void RetryScene(ulong playerID)
    {
        List<Flask> flasks = players.Find(player => player.playerId == playerID).flasks;
        int level = players.FindIndex(player => player.playerId == playerID);

        // Refill flask on host
        FlaskCreator.RefillFlasks(flasks, scenes[level], contentHeight);
        // Refill flask on client
        multiplayerStore.CreateFlasksClientRPC(FlaskCreator.FlattenArray(scenes[level]), flasks.Count, nbContent, level);
        // Reset selected flask
        selectedFlask = null;
    }

    public void SetMultiplayerStore(MultiplayerStore multiplayerStore)
    {
        this.multiplayerStore = multiplayerStore;
    }

    public int GetPosPlayerFromFlask(Flask flask)
    {
        return players.FindIndex(player => player.flasks.Contains(flask));
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
                    clickedFlask.GetColors().ForEach(color =>
                    {
                        Debug.Log(color);
                    });
                    // Cannot interact with others host/client flask
                    if (NetworkManager.Singleton.IsHost)
                    {
                        canInteractFlask = players.Find(player => player.flasks.Contains(clickedFlask)).playerId == NetworkManager.Singleton.LocalClientId;
                    }
                    else
                    {
                        canInteractFlask = players[multiplayerStore.posClient].flasks.Contains(clickedFlask);
                    }

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
                                    TryNextLevel(ref multiplayerStore.hostLv, GetPosPlayerFromFlask(clickedFlask));
                                }
                                if (!clientClear)
                                {
                                    TryNextLevel(ref multiplayerStore.clientLv, GetPosPlayerFromFlask(clickedFlask));
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
            List<Flask> spillFlaskList = players[listWaitingSpill[listWaitingSpill.Count - 1].Item3].flasks;
            Flask giver = spillFlaskList[listWaitingSpill[listWaitingSpill.Count - 1].Item1];
            Flask receiver = spillFlaskList[listWaitingSpill[listWaitingSpill.Count - 1].Item2];

            bool spilled = false;
            // Try to spill
            if (giver.CanSpill(receiver))
            {
                spilled = giver.SpillTo(receiver);
            }

            if (spilled)
            {
                if (NetworkManager.Singleton.IsHost)
                {
                    // Spilled, try to go to next scene
                    if (!hostClear)
                    {
                        TryNextLevel(ref multiplayerStore.hostLv, listWaitingSpill[listWaitingSpill.Count - 1].Item3);
                    }
                    if (!clientClear)
                    {
                        TryNextLevel(ref multiplayerStore.clientLv, listWaitingSpill[listWaitingSpill.Count - 1].Item3);
                    }
                }
                listWaitingSpill.RemoveAt(listWaitingSpill.Count - 1);
            }
        }
    }
}
