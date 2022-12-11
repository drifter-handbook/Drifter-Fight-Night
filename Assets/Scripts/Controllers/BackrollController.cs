using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HouraiTeahouse.Backroll;
using System;

public struct data
{
	string bits;
}

public class BackrollController : MonoBehaviour
{
   BackrollSession<data> br;
   BackrollErrorCode result;
   BackrollSessionCallbacks cb;


   unsafe void DoThing()
   {
   		/* fill in all callback functions */
   		cb.LoadGameState = load_state;
   		cb.SaveGameState = save_state;
   		cb.FreeBuffer = free_buffer;
   		cb.AdvanceFrame = advance_state;



   		// /* Start a new session */
   		// result = ggpo_start_session(&br,         // the new session object
     //                           &cb,           // our callbacks
     //                           "test_app",    // application name
     //                           2,             // 2 players
     //                           sizeof(int),   // size of an input packet
     //                           8001);         // our local udp port
	}

	Sync.SavedFrame framer()
	{
		return Sync.SavedFrame.Create();
	}


	void save_state(ref Sync.SavedFrame frame)
	{

	}

	unsafe void load_state(void* buffer, int len)
	{

	}

	void free_buffer(IntPtr input)
	{

	}

	void advance_state()
	{

	}
}
