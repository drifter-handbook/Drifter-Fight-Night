using UnityEngine;

public class LocalPlayer {
    readonly int ConnectionID;
    public int PlayerNumber;
    Color Color;

    public LocalPlayer(int connectionID, int playerNumber) {
        new LocalPlayer(connectionID, playerNumber, new Color());
    }

    public LocalPlayer(int connectionID, int playerNumber, Color color) {
        ConnectionID = connectionID;
        PlayerNumber = playerNumber;
        Color = color;
    }

    
}