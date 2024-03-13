using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class CharacterSelectState
{
	public int PeerID = -1;
	public int x = 7;
	public int y = 1;
	public int removalTimer = 0;

	public DrifterType PlayerType = DrifterType.None;
	public BattleStage StageType = BattleStage.None;

	public PlayerInputData prevInput;

	//Store this elsewhere?
	public GameObject Cursor;

	public void Serialize(BinaryWriter bw) {
		bw.Write(PeerID);
		bw.Write(x);
		bw.Write(y);
		bw.Write(removalTimer);
		bw.Write((int)PlayerType);
		bw.Write((int)StageType);
		prevInput.Serialize(bw);
	}

	public void Deserialize(BinaryReader br) {
		PeerID = br.ReadInt32();
		x = br.ReadInt32();
		y = br.ReadInt32();
		removalTimer = br.ReadInt32();
		PlayerType = (DrifterType)br.ReadInt32();
		StageType = (BattleStage)br.ReadInt32();
		prevInput.Deserialize(br);
	}

}
