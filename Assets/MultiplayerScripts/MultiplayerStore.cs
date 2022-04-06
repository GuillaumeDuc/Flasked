using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MultiplayerStore : NetworkBehaviour
{
    public ServerManager serverManager;
    private bool serverManagerFound = false;
    private Color[] initColors;
    private int initNbContent, initNbFlask;
    public int nbPlayers, posClient;

    void UpdateLvClientChanged(int prevInt, int nextInt)
    {
        if (serverManagerFound)
        {
            serverManager.clientFlaskCurrentLvText.text = "" + (nextInt + 1);
        }
    }

    void UpdateLvHostChanged(int prevInt, int nextInt)
    {
        if (serverManagerFound)
        {
            serverManager.hostFlaskCurrentLvText.text = "" + (nextInt + 1);
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
        serverManager.AddToWaitingSpillList(posGiver, posReceiver, posPlayer);
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
    public void CreateFlasksClientRPC(Color[] colors, int flasksCount, int nbContent, int posPlayer)
    {
        if (IsOwner) return;
        if (serverManagerFound)
        {
            Player player = serverManager.players[posPlayer];
            FlaskCreator.DeleteFlasks(player.flasks);
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
        // Init Levels
        // serverManager.hostFlaskCurrentLvText.text = "" + (serverManager.players[posClient].level + 1);
        // serverManager.clientFlaskCurrentLvText.text = "" + (serverManager.players[posClient].level + 1);

        // Init buttons listener
        if (NetworkManager.Singleton.IsHost)
        {
            serverManager.RetryHostButton.onClick.AddListener(() => serverManager.RetryScene(NetworkManager.Singleton.LocalClientId));
            serverManager.RetryClientButton.gameObject.SetActive(false);
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            serverManager.RetryClientButton.onClick.AddListener(() => RetrySceneServerRPC(NetworkManager.Singleton.LocalClientId));
            serverManager.RetryHostButton.gameObject.SetActive(false);
        }
    }

    void InitFlasks()
    {
        if (!NetworkManager.Singleton.IsHost)
        {
            if (initColors != null)
            {
                for (int i = 0; i < nbPlayers; i++)
                {
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
                InitUI();
                InitFlasks();
            }
        }
    }
}
