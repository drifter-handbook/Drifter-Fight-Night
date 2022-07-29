using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericMasterHit : MasterHit
{
	//Takes a snapshot of the current frame to rollback to
    public override MasterhitRollbackFrame SerializeFrame()
    {
    	MasterhitRollbackFrame baseFrame = SerializeBaseFrame();
        return baseFrame;
    }

    //Rolls back the entity to a given frame state
    public override void DeserializeFrame(MasterhitRollbackFrame p_frame)
    {
    	DeserializeBaseFrame(p_frame);
    }

}
