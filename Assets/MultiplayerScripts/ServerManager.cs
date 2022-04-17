using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.Rendering.Universal;
using System.Linq;

public class ServerManager : MonoBehaviour
{
    public int nbContent = 4;
    public float contentHeight = 1;
    public int nbEmpty = 1;
    public int maxLv = 5;
    public GameObject flaskPrefab;
    public GameObject MultiplayerStorePrefab;
    public Text hostFlaskCurrentLvText;
    public Text clientFlaskCurrentLvText;
    public GameObject endPanelHost;
    public GameObject endPanelClient;
    public Button RetryHostButton;
    public Button RetryClientButton;
    private Flask selectedFlask;
    private List<Color[][]> scenes = new List<Color[][]>();
    public Light2D light1P, light2P;
    [HideInInspector]
    public List<Player> players = new List<Player>();
    private MultiplayerStore multiplayerStore;
    private List<(int, int, int)> listWaitingSpill = new List<(int, int, int)>();
    private bool clientClear = false, hostClear = false;
    float minX = .05f;
    float maxX = .48f;
    float xStep = .055f;
    float yStep = .2f;
    float maxHeight = .75f;
    float spillingYOffset = 2;
    float spillingXOffset = 1.75f;

    float lightXOffset2P = 7;
    float lightOuterAngle2P = 6;
    float orthographicSize2P = 7;
    float yStep2P = .4f;
    float maxHeight2P = .65f;

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
        // Change screen config depending on nb players
        SetScreenConfigServer(NetworkManager.Singleton.ConnectedClientsIds.Count);
        // Init players
        for (int i = 0; i < NetworkManager.Singleton.ConnectedClientsIds.Count; i++)
        {
            Player p = new Player(NetworkManager.Singleton.ConnectedClientsIds[i]);
            players.Add(p);
            CreateFlasks(ref p.flasks, scenes[p.level], p);

            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { NetworkManager.Singleton.ConnectedClientsIds[i] }
                }
            };

            multiplayerStore.InitAllFlasksClientRPC(
                FlaskCreator.FlattenArray(scenes[p.level]),
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
        // Host pos is always first
        multiplayerStore.posClient = 0;
    }

    public void CreateFlasks(ref List<Flask> flasks, Color[][] colors, Player p)
    {
        int pos = players.FindIndex(player => player == p);
        float offsetX = GetOffsetX(pos);
        float offsetY = GetOffsetY(pos);

        // Spawn flasks
        flasks = FlaskCreator.CreateFlasks(
            flaskPrefab,
            FlaskCreator.GetNbFlaskMultiplayer(p.level),
            nbContent,
            nbEmpty,
            contentHeight,
            offsetX + minX,
            offsetX + maxX,
            offsetY,
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

    float GetOffsetX(int posPlayer)
    {
        return posPlayer % 2 == 0 ? 0 : .5f;
    }

    float GetOffsetY(int posPlayer)
    {
        if (posPlayer % 2 != 0)
        {
            posPlayer -= 1;
        }
        return (float)posPlayer / 4.7f;
    }

    void SetScreenConfigServer(int nbPlayers)
    {
        if (nbPlayers < 3)
        {
            SetScreenConfig(orthographicSize2P, yStep2P, maxHeight2P, lightOuterAngle2P, lightXOffset2P);
            multiplayerStore.SetScreenConfigClientRPC(orthographicSize2P, yStep2P, maxHeight2P, lightOuterAngle2P, lightXOffset2P);
        }
    }

    public void ChangeLights(float lightOuterAngle, float lightXOffset)
    {
        light1P.pointLightOuterRadius = lightOuterAngle;
        light1P.transform.position = new Vector3(-lightXOffset, light1P.transform.position.y, light1P.transform.position.z);
        light2P.pointLightOuterRadius = lightOuterAngle;
        light2P.transform.position = new Vector3(lightXOffset, light2P.transform.position.y, light2P.transform.position.z);
    }

    public void SetScreenConfig(float screenSize, float yStep, float maxHeight, float lightOuterAngle, float lightXOffset)
    {
        Camera.main.orthographicSize = screenSize;
        this.yStep = yStep;
        this.maxHeight = maxHeight;
        ChangeLights(lightOuterAngle, lightXOffset);
    }

    public bool SpillBottle(Flask giver, Flask receiver)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            bool spilled = (giver != null) ? giver.SpillTo(receiver) : false;
            multiplayerStore.SpillBottleClientRPC(
                GetPosFlaskServer(giver),
                GetPosFlaskServer(receiver),
                GetPosPlayerServer(NetworkManager.Singleton.LocalClientId)
            );
            return spilled;
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            if (giver != null && giver.CanSpill(receiver))
            {
                multiplayerStore.SpillBottleServerRPC(
                    GetPosFlaskClient(giver),
                    GetPosFlaskClient(receiver),
                    multiplayerStore.posClient
                );
                return true;
            }
            return false;
        }
        return false;
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

    void NextLevelServer(Player p)
    {
        // Delete flasks
        FlaskCreator.DeleteFlasks(p.flasks);
        // Recreate and respawn flasks
        Color[][] colorsScene = scenes[p.level];
        CreateFlasks(ref p.flasks, colorsScene, p);
        int nbFlasks = p.flasks.Count;
        multiplayerStore.NextLevelClientRPC(FlaskCreator.FlattenArray(colorsScene), nbFlasks, nbContent, players.FindIndex(player => player == p), p.level);
    }

    void TryNextLevelServer(Player p)
    {
        List<Flask> flasks = p.flasks;
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
            p.level += 1;
            if (p.level < scenes.Count)
            {
                selectedFlask = null;
                NextLevelServer(p);
            }
            else
            {
                // UI
                /*
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
                */
            }
        }
    }

    public void RetryScene(ulong playerID)
    {
        int posPlayer = GetPosPlayerServer(playerID);
        Player player = players[posPlayer];
        List<Flask> flasks = player.flasks;
        int level = player.level;

        // Refill flask on host
        FlaskCreator.RefillFlasks(flasks, scenes[level], contentHeight);
        // Refill flask on client
        multiplayerStore.RefillFlasksClientRPC(FlaskCreator.FlattenArray(scenes[level]), posPlayer, contentHeight);
        // Reset selected flask
        selectedFlask = null;
    }

    public void SetMultiplayerStore(MultiplayerStore multiplayerStore)
    {
        this.multiplayerStore = multiplayerStore;
    }

    public Player GetPlayerFromFlask(Flask flask)
    {
        return players.Find(player => player.flasks.Contains(flask));
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
                                TryNextLevelServer(GetPlayerFromFlask(clickedFlask));
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
                    TryNextLevelServer(GetPlayerFromFlask(giver));
                }
                listWaitingSpill.RemoveAt(listWaitingSpill.Count - 1);
            }
        }
    }
}
