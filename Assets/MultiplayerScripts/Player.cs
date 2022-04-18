using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player
{
    public List<Flask> flasks;
    public ulong playerId;
    public int level;
    public GameObject UI;

    public Player(ulong playerId)
    {
        this.playerId = playerId;
        this.flasks = new List<Flask>();
    }
    public Player()
    {
        this.playerId = 0;
        this.flasks = new List<Flask>();
    }

    public void HideButton()
    {
        UI.GetComponentInChildren<Button>().gameObject.SetActive(false);
    }

    public void UpdateLevelUI()
    {
        UI.transform.GetChild(1).GetComponent<Text>().text = "" + (level + 1);
    }
}