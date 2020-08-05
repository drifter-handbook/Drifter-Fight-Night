using UnityEngine;

public enum PlayerColor
{
    RED,
    GOLD,
    GREEN,
    BLUE,
    PURPLE,
    MAGENTA,
    ORANGE,
    CYAN
}

public struct PlayerData
{
    private readonly int playerID;
    public int PlayerID { get { return this.playerID; } } // From NetworkID - unique per connection
    public int PlayerIndex { get; set; }//based on join order - can be changed if others drop
    public PlayerColor PlayerColor { get; set; } // assigned on join

    public GameObject arrow;
    public GameObject characterCard;
    public string selectedDrifter; //this will probably not be a string, but-

    public PlayerData(int playerID) : this()
    {
        this.playerID = playerID;
    }

    public Color getColorFromEnum()
    {
        switch (this.PlayerColor)
        {
            case PlayerColor.RED:
                return new Color(1.0f, 0f, 0f);
            case PlayerColor.GOLD:
                return new Color(1.0f, 0.8f, 0f);
            case PlayerColor.GREEN:
                return new Color(0.124f, 0.866f, 0.118f);
            case PlayerColor.BLUE:
                return new Color(0.075f, 0.702f, 0.906f);
            case PlayerColor.PURPLE:
                return new Color(0.725f, 0.063f, 1.0f);
            case PlayerColor.MAGENTA:
                return new Color(1.0f, 0.063f, 0.565f);
            case PlayerColor.ORANGE:
                return new Color(1.0f, 0.55f, 0.165f);
            case PlayerColor.CYAN:
                return new Color(0.0f, 1.0f, 0.702f);

            default:
                return Color.white;
        }
    }
}

