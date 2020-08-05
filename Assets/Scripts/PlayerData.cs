using UnityEngine;

public struct PlayerData
{
    private readonly int playerID;
    public int PlayerID { get { return this.playerID; } } // From NetworkID - unique per connection
    public int PlayerIndex { get; set; }//based on join order - can be changed if others drop
    public Color PlayerColor { get; set; } // assigned on join

    public GameObject arrow;
    public GameObject characterCard;
    public string selectedDrifter; //this will probably not be a string, but-
    
    public PlayerData(int playerID) : this()
    {
        this.playerID = playerID;
    }
}
