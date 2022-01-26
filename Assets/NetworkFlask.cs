using UnityEngine;
using Unity.Netcode;

public class NetworkFlask : Flask
{
    public bool isClientFlask = false;

    [ClientRpc]
    public void InitFlaskClientRPC(int layerFlaskContainer, int maxSize, Color[] colorsServer, bool isClientFlask)
    {
        if (IsOwner) return;

        this.isClientFlask = isClientFlask;
        this.maxSize = maxSize;
        animFlask = gameObject.GetComponent<AnimFlask>();
        GetComponentInChildren<Container>().gameObject.layer = layerFlaskContainer;
        for (int i = 0; i < colorsServer.Length; i++)
        {
            AddColor(colorsServer[i], contentHeight);
        }
    }

    [ClientRpc]
    public void EmptyFlaskClientRPC()
    {
        if (IsOwner) return;

        GetComponentInChildren<Container>().ClearContents();
    }

    [ClientRpc]
    public void RefillFlaskClientRPC(Color[] colorsServer)
    {
        if (IsOwner) return;

        Clear();

        for (int i = 0; i < colorsServer.Length; i++)
        {
            AddColor(colorsServer[i], contentHeight);
        }
    }
}
