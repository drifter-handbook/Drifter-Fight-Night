using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectState
{
    public int PeerID = -1;
    public DrifterType PlayerType = DrifterType.None;
    public BattleStage StageType = BattleStage.None;
    public int x = 7;
    public int y = 1;
    public GameObject Cursor;
    public PlayerInputData prevInput;
}
