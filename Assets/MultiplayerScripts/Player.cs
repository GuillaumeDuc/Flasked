using System.Collections.Generic;

public class Player
{
    public List<Flask> flasks;
    public ulong playerId;

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
}