using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[Serializable]
public class PlayerInputData : ICloneable, IEquatable<PlayerInputData> {
	public int MoveX;
	public int MoveY;
	public bool Jump;
	public bool Light;
	public bool Special;
	public bool Super;
	public bool Guard;
	public bool Pause;
	public bool Dash;
	public bool Grab;
	public bool Menu;

	public object Clone() {
		return new PlayerInputData() {
			MoveX = MoveX,
			MoveY = MoveY,
			Jump = Jump,
			Light = Light,
			Special = Special,
			Super = Super,
			Guard = Guard,
			Dash = Dash,
			Pause = Pause,
			Grab = Grab,
			Menu = Menu,
		};
	}

	public bool Equals(PlayerInputData other){
		if(other == null) return false;
		if( ReferenceEquals(this, other)) return true;

		return(
			MoveX == other.MoveX &&
			MoveY == other.MoveY && 
			Jump == other.Jump &&
			Light == other.Light &&
			Special == other.Special &&
			Super == other.Super &&
			Guard == other.Guard &&
			Grab == other.Grab &&
			Dash == other.Dash
			);
	   
	}

	public bool isEmpty(){
		return(
			MoveX == 0 &&
			MoveY == 0 && 
			Jump == false &&
			Light == false &&
			Special == false &&
			Super == false &&
			Guard == false &&
			Grab == false &&
			Dash == false
			);
	}

	public override String ToString(){
		return 
			MoveX 					+ "," +
			MoveY 					+ "," + 
			(Jump		? "1":"0") 	+ "," +
			(Light		? "1":"0") 	+ "," +
			(Special	? "1":"0") 	+ "," +
			(Super		? "1":"0") 	+ "," +
			(Guard		? "1":"0") 	+ "," +
			(Grab		? "1":"0")+ "," +
			(Dash		? "1":"0");

	}

	public static PlayerInputData FromString(String data){
		string[] buttons = data.Split(',');
		if(buttons.Length <8) return new PlayerInputData();

		return new PlayerInputData{
			MoveX 		= Int32.Parse(buttons[0]),
			MoveY 		= Int32.Parse(buttons[1]),
			Jump		= buttons[2].Equals("1"),	
			Light		= buttons[3].Equals("1"),
			Special		= buttons[4].Equals("1"),
			Super		= buttons[5].Equals("1"),
			Guard		= buttons[6].Equals("1"),
			Grab		= buttons[7].Equals("1"),
			Dash		= buttons[8].Equals("1")
		};
	}


	public void Serialize(BinaryWriter bw){
		bw.Write(MoveX);
		bw.Write(MoveY);
		bw.Write(Jump);
		bw.Write(Light);
		bw.Write(Special);
		bw.Write(Super);
		bw.Write(Guard);
		bw.Write(Grab);
		bw.Write(Dash);
		bw.Write(Pause);
		bw.Write(Menu);
	}

	 public void Deserialize(BinaryReader br) {  
		MoveX = br.ReadInt32();
		MoveY = br.ReadInt32();
		Jump = br.ReadBoolean();
		Light = br.ReadBoolean();
		Special = br.ReadBoolean();
		Super = br.ReadBoolean();
		Guard = br.ReadBoolean();
		Grab = br.ReadBoolean();
		Dash = br.ReadBoolean();
		Pause = br.ReadBoolean();
		Menu = br.ReadBoolean();	
	}

	public void CopyFrom(PlayerInputData data) {
		MoveX = data.MoveX;
		MoveY = data.MoveY;
		Jump = data.Jump;
		Light = data.Light;
		Special = data.Special;
		Super = data.Super;
		Guard = data.Guard;
		Dash = data.Dash;
		Pause = data.Pause;
		Grab = data.Grab;
		Menu = data.Menu;
	}
}
