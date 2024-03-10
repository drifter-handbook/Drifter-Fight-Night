using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class NeoMegurinMasterHit : MasterHit {

	//Takes a snapshot of the current frame to rollback to
	public override void Serialize(BinaryWriter bw) {
		base.Serialize(bw);
	}

	//Rolls back the entity to a given frame state
	public override void Deserialize(BinaryReader br) {
		base.Deserialize(br);
	}

}


