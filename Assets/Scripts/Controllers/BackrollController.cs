using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HouraiTeahouse.Backroll;
using HouraiTeahouse.Networking;
using System;
using HouraiTeahouse.Networking.Discord;
using System.Threading.Tasks;

public struct data
{
	string bits;
}

public class BackrollController : MonoBehaviour
{
   BackrollSession<data> br;
   BackrollErrorCode result;
   BackrollSessionCallbacks cb;
   BackrollSessionConfig config;


   SyncTestsBackrollSession<int> session;

   DiscordIntegrationClient discord = new DiscordIntegrationClient(897161969113628752);

   public unsafe void InitializeRollbackSession()
    {
        // Only local player
        LobbyMember[] players = new LobbyMember[1];
        List<LobbyMember> spectators = new List<LobbyMember>();

        // Assigning cb
        BackrollSessionCallbacks cb = new BackrollSessionCallbacks();
        cb.SaveGameState = SaveGameState;
        cb.LoadGameState = LoadGameState;
        cb.FreeBuffer = FreeBuffer;
        cb.AdvanceFrame = AdvanceFrame;

        // cb.OnPlayerSynchronized = OnPlayerSynchronized;
        // cb.OnConnected = OnConnected;
        // cb.OnConnectionInterrupted = OnConnectionInterrupted;
        // cb.OnConnectionResumed = OnConnectionResumed;
        // cb.OnPlayerSynchronizing = OnPlayerSynchronizing;
        // cb.OnReady = OnReady;
        // cb.OnTimeSync = OnTimeSync;

        BackrollSessionConfig config = new BackrollSessionConfig
        {
            Players = players,
            Spectators = spectators.ToArray(),
            Callbacks = cb,
        };

        session = new SyncTestsBackrollSession<int>(config, 1);

        //Backroll.StartSession<data>(config); 
    }

    public void makeLobby()
    {
    	Task<Lobby> task = discord.LobbyManager.CreateLobby(new LobbyCreateParams{Type = LobbyType.Private, Capacity = 2});
    }
	

    /*
    SavedFrame is just a size and a pointer
	You'll need to manually allocate a buffer (i.e. malloc)
	Then copy the saved state into it
	Then save the buffer's pointer and the size of the buffer into the SavedFrame struct
	FreeBuffer will be called when the saved frame must be manually deallocated
	This is fairly low level memory management
	So if you don't know your way around pointers and manual memory management, you may need to read up on that
    */
	void SaveGameState(ref Sync.SavedFrame frame)
	{
		// playerStruct p1 = fighters[0].saveBlob(frame.Frame);
  //       PlayerStruct p2 = fighters[1].saveBlob(frame.Frame);
  //       SaveBlob save = new SaveBlob();
  //       save.p1 = p1;
  //       save.p2 = p2;
  //       save.timer = this.time;

  //       int size = Marshal.SizeOf(save);
  //       frame.Size = size;
  //       IntPtr arrPtr = Marshal.AllocHGlobal(size);
  //       Marshal.StructureToPtr(save, arrPtr, true);


  //       frame.Buffer = (byte*)arrPtr;
	}

	unsafe void LoadGameState(void* buffer, int len)
	{

	}

	void FreeBuffer(IntPtr input)
	{

	}

	void AdvanceFrame()
	{

	}
}
