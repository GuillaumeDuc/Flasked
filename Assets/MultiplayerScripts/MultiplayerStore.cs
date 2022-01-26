using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MultiplayerStore : NetworkBehaviour
{
    public NetworkVariable<int> nbRetry,
        nbUndo = new NetworkVariable<int>(),
        host = new NetworkVariable<int>(),
        hostLv = new NetworkVariable<int>(0),
        clientLv = new NetworkVariable<int>(0);

    public ServerManager serverManager;
    private bool serverManagerFound = false;

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
    public void RetrySceneServerRPC()
    {
        serverManager.RetryScene(true);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpillBottleServerRPC(NetworkObjectReference nrGiver, NetworkObjectReference nrReceiver)
    {
        if (nrGiver.TryGet(out NetworkObject networkObjectFlaskGiver) && nrReceiver.TryGet(out NetworkObject networkObjectFlaskReceiver))
        {
            serverManager.AddToWaitingSpillList(networkObjectFlaskGiver.GetComponent<NetworkFlask>(), networkObjectFlaskReceiver.GetComponent<NetworkFlask>());
        }
    }

    [ClientRpc]
    public void SpillBottleClientRPC(NetworkObjectReference nrGiver, NetworkObjectReference nrReceiver)
    {
        if (IsOwner) return;

        if (nrGiver.TryGet(out NetworkObject networkObjectFlaskGiver) && nrReceiver.TryGet(out NetworkObject networkObjectFlaskReceiver))
        {
            serverManager.AddToWaitingSpillList(networkObjectFlaskGiver.GetComponent<NetworkFlask>(), networkObjectFlaskReceiver.GetComponent<NetworkFlask>());
        }
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

    void InitUI()
    {
        // Init Levels
        serverManager.hostFlaskCurrentLvText.text = "" + (hostLv.Value + 1);
        serverManager.clientFlaskCurrentLvText.text = "" + (clientLv.Value + 1);

        // Init buttons listener
        if (NetworkManager.Singleton.IsHost)
        {
            serverManager.RetryHostButton.onClick.AddListener(() => serverManager.RetryScene(false));
            serverManager.RetryClientButton.gameObject.SetActive(false);
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            serverManager.RetryClientButton.onClick.AddListener(() => RetrySceneServerRPC());
            serverManager.RetryHostButton.gameObject.SetActive(false);
        }
    }

    void Start()
    {
        // Listen for changes
        hostLv.OnValueChanged += UpdateLvHostChanged;
        clientLv.OnValueChanged += UpdateLvClientChanged;
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
            }
        }
    }
}
