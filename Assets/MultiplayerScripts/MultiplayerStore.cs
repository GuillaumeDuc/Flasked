using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class MultiplayerStore : NetworkBehaviour
{
    public ServerManager serverManager;
    private bool serverManagerFound = false;
    private Color[] initColors;
    private int initNbContent, initNbFlask;
    private float screenSize, yStep, maxHeight, lightOuterAngle, lightXOffset;
    public int nbPlayers, posClient;

    [ClientRpc]
    public void SetScreenConfigClientRPC(float screenSize, float yStep, float maxHeight, float lightOuterAngle, float lightXOffset)
    {
        if (IsHost) return;
        if (serverManagerFound)
        {
            serverManager.SetScreenConfig(screenSize, yStep, maxHeight, lightOuterAngle, lightXOffset);
        }
        else
        {
            this.screenSize = screenSize;
            this.yStep = yStep;
            this.maxHeight = maxHeight;
            this.lightOuterAngle = lightOuterAngle;
            this.lightXOffset = lightXOffset;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RetrySceneServerRPC(ulong clientId)
    {
        serverManager.RetryScene(clientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpillBottleServerRPC(int posGiver, int posReceiver, int posPlayer)
    {
        // Play on server
        serverManager.AddToWaitingSpillList(posGiver, posReceiver, posPlayer);
        // Play on all clients
        SpillBottleClientRPC(posGiver, posReceiver, posPlayer);
    }

    [ClientRpc]
    public void SpillBottleClientRPC(int posGiver, int posReceiver, int posPlayer)
    {
        if (IsOwner) return;
        serverManager.AddToWaitingSpillList(posGiver, posReceiver, posPlayer);
    }

    [ClientRpc]
    public void ClearedUIClientRPC(bool isClient)
    {
        if (IsOwner) return;

        if (isClient)
        {
            serverManager.endPanelClient.SetActive(true);
        }
        else
        {
            serverManager.endPanelHost.SetActive(true);
        }
    }

    [ClientRpc]
    public void NextLevelClientRPC(Color[] colors, int flasksCount, int nbContent, int posPlayer, int newLevel)
    {
        if (IsOwner) return;
        if (serverManagerFound)
        {
            Player player = serverManager.players[posPlayer];
            FlaskCreator.DeleteFlasks(player.flasks);
            player.level = newLevel;
            player.UpdateLevelUI();
            serverManager.CreateFlasks(ref player.flasks, FlaskCreator.UnflattenArray(colors, flasksCount, serverManager.nbContent), player);
        }
    }

    [ClientRpc]
    public void RefillFlasksClientRPC(Color[] colors, int pos, float contentHeight)
    {
        if (IsOwner) return;
        List<Flask> flasks = serverManager.players[pos].flasks;
        Color[][] newColors = FlaskCreator.UnflattenArray(colors, flasks.Count, serverManager.nbContent);
        // Refill flask on host
        FlaskCreator.RefillFlasks(flasks, newColors, contentHeight);
    }

    [ClientRpc]
    public void InitAllFlasksClientRPC(Color[] colors, int flasksCount, int nbContent, int posClient, int nbPlayers, ClientRpcParams clientRpcParams = default)
    {
        if (IsOwner) return;
        this.nbPlayers = nbPlayers;
        this.posClient = posClient;
        // Wait for server manager to be found
        initColors = new Color[colors.Length];
        initColors = colors;
        initNbContent = nbContent;
        initNbFlask = flasksCount;
    }

    void InitUI()
    {
        for (int i = 0; i < serverManager.players.Count; i++)
        {
            GameObject go = Instantiate(
                serverManager.LevelResetDisplay,
                Camera.main.ViewportToScreenPoint(new Vector3((serverManager.GetOffsetX(i) + .18f) / 2, -(serverManager.GetOffsetY(i) + .06f) / 2.4f, 0)),
                serverManager.LevelResetDisplay.transform.rotation
            );
            go.transform.SetParent(serverManager.Canvas.transform, false);
            serverManager.players[i].UI = go;

            // Update level UI
            serverManager.players[i].UpdateLevelUI();

            // Reset button listener on current player
            if (posClient == i)
            {
                if (NetworkManager.Singleton.IsHost)
                {
                    go.GetComponentInChildren<Button>().onClick.AddListener(() => serverManager.RetryScene(NetworkManager.Singleton.LocalClientId));
                }
                else if (NetworkManager.Singleton.IsClient)
                {
                    go.GetComponentInChildren<Button>().onClick.AddListener(() => RetrySceneServerRPC(NetworkManager.Singleton.LocalClientId));
                }
            }
            else // Hide reset button for other players
            {
                serverManager.players[i].HideButton();
            }
        }
    }

    void InitPlayers()
    {
        if (!NetworkManager.Singleton.IsHost)
        {
            // Set screensize if initialized
            if (screenSize != 0)
            {
                serverManager.SetScreenConfig(screenSize, yStep, maxHeight, lightOuterAngle, lightXOffset);
            }
            if (initColors != null)
            {
                for (int i = 0; i < nbPlayers; i++)
                {
                    // Init flasks
                    Player p = new Player();
                    serverManager.players.Add(p);
                    serverManager.CreateFlasks(ref p.flasks, FlaskCreator.UnflattenArray(initColors, initNbFlask, initNbContent), p);
                }
            }
        }
    }

    void Update()
    {
        // Wait for ServerManager to be initialized
        if (!serverManagerFound)
        {
            serverManager = FindObjectOfType<ServerManager>();
            if (serverManager)
            {
                serverManagerFound = true;
                serverManager.SetMultiplayerStore(this);
                InitPlayers();
                InitUI();
            }
        }
    }
}
