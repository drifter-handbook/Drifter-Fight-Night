using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[Serializable]
public class CharacterSelectState : ICloneable {
	public int PeerID = -1;
	public int x = 7;
	public int y = 1;
	public int removalTimer = 0;
	public DrifterType PlayerType = DrifterType.None;
	public BattleStage StageType = BattleStage.None;
	public PlayerInputData prevInput;
	public int GameStandings = -1;
	public GameObject Cursor;

	public object Clone() {
		return new CharacterSelectState() {
			PeerID = PeerID,
			x = x,
			y = y,
			removalTimer = removalTimer,
			PlayerType = PlayerType,
			StageType = StageType,
			prevInput = prevInput,			
			GameStandings = GameStandings,
			Cursor = Cursor,
		};
	}

	public void Serialize(BinaryWriter bw) {
		bw.Write(PeerID);
		bw.Write(x);
		bw.Write(y);
		bw.Write(removalTimer);
		bw.Write((int)PlayerType);
		bw.Write((int)StageType);
		bw.Write(GameStandings);
		prevInput.Serialize(bw);
	}

	public void Deserialize(BinaryReader br) {
		PeerID = br.ReadInt32();
		x = br.ReadInt32();
		y = br.ReadInt32();
		removalTimer = br.ReadInt32();
		PlayerType = (DrifterType)br.ReadInt32();
		StageType = (BattleStage)br.ReadInt32();
		GameStandings = br.ReadInt32();
		prevInput.Deserialize(br);
	}


	public override String ToString(){
		return 
			"PeerID: " + PeerID + "; " +
			"Drifter: " + PlayerType.ToString() + "; " +
			"Standings" + GameStandings + "; " +
			"Matrix Position: [" + x + ", " + y + "]; "  +
			"Stage" + StageType.ToString();
	}

}
