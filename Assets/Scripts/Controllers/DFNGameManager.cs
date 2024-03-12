using SharedGame;
using System.Collections.Generic;
using UnityGGPO;

public class DFNGameManager : GameManager {

	public override void StartLocalGame() {
		StartGame(new LocalRunner(new DFNGame(GameController.Instance.maxPlayerCount)));
	}

	public void StopGame(){
		Shutdown();
	}

	public void DisconnectGGPOGame(){
		//Disconnect each player from the game, then shutdown
		//DisconnectPlayer(1);
		Shutdown();
	}

	public override void StartGGPOGame(IPerfUpdate perfPanel, IList<Connections> connections, int playerIndex) {
		var game = new GGPORunner("DFN", new DFNGame(connections.Count), perfPanel);
		game.Init(connections, playerIndex);
		StartGame(game);
	}
}