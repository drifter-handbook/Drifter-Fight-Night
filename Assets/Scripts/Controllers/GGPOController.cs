using SharedGame;
using UnityGGPO;
using System;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DFN {

	public class DNFGameManager : GameManager {

		public override void StartLocalGame() {
			StartGame(new LocalRunner(new DFNGame(2)));
		}

		public override void StartGGPOGame(IPerfUpdate perfPanel, IList<Connections> connections, int playerIndex) {
			var game = new GGPORunner("DFN", new DFNGame(connections.Count), perfPanel);
			game.Init(connections, playerIndex);
			StartGame(game);
		}
	}

	[Serializable]
	public struct DFNGame : IGame {

		public const int INPUT_JUMP 	= (1 << 0);
    	public const int INPUT_LIGHT 	= (1 << 1);
    	public const int INPUT_SPECIAL 	= (1 << 2);
    	public const int INPUT_SUPER 	= (1 << 3);
    	public const int INPUT_GUARD 	= (1 << 4);
    	public const int INPUT_GRAB 	= (1 << 5);
    	public const int INPUT_DASH 	= (1 << 6);
    	public const int INPUT_PAUSE 	= (1 << 7);
    	public const int INPUT_MENU		= (1 << 8);
    	public const int INPUT_LEFT 	= (1 << 9);
    	public const int INPUT_RIGHT	= (1 << 10);
    	public const int INPUT_UP 		= (1 << 11);
    	public const int INPUT_DOWN		= (1 << 12);

		public int Framenumber { get; private set; }

		public int Checksum => GetHashCode();

		public Drifter[] _drifters;

		//public static Rect _bounds = new Rect(0, 0, 640, 480);

		public void Serialize(BinaryWriter bw) {
		    bw.Write(Framenumber);
		    bw.Write(_drifters.Length);
		    for (int i = 0; i < _drifters.Length; ++i) {
		        _drifters[i].Serialize(bw);
		    }
		}

		public void Deserialize(BinaryReader br) {
		    Framenumber = br.ReadInt32();
		    int length = br.ReadInt32();
		    if (length != _drifters.Length) {
		        _drifters = new Drifter[length];
		    }
		    for (int i = 0; i < _drifters.Length; ++i) {
		        _drifters[i].Deserialize(br);
		    }
		}

		public NativeArray<byte> ToBytes() {
		    using (var memoryStream = new MemoryStream()) {
		        using (var writer = new BinaryWriter(memoryStream)) {
		            Serialize(writer);
		        }
		        return new NativeArray<byte>(memoryStream.ToArray(), Allocator.Persistent);
		    }
		}

		public void FromBytes(NativeArray<byte> bytes) {
		    using (var memoryStream = new MemoryStream(bytes.ToArray())) {
		        using (var reader = new BinaryReader(memoryStream)) {
		            Deserialize(reader);
		        }
		    }
		}

		/*
		 * InitGameState --
		 *
		 * Initialize our game state.
		 */

		public DFNGame(int num_players) {

			Drifter[] drifters;
			//Get drifters from network players?
			Framenumber = 0;
			_drifters = new Drifter[num_players];
		}

		// public void MoveShip(int index, float heading, float thrust, int fire) {
		//     var ship = _ships[index];

		//     GGPORunner.LogGame($"calculation of new ship coordinates: (thrust:{thrust} heading:{heading}).");

		//     ship.heading = heading;

		//     if (ship.cooldown == 0) {
		//         if (fire != 0) {
		//             GGPORunner.LogGame("firing bullet.");
		//             for (int i = 0; i < ship.bullets.Length; i++) {
		//                 float dx = Mathf.Cos(DegToRad(ship.heading));
		//                 float dy = Mathf.Sin(DegToRad(ship.heading));
		//                 if (!ship.bullets[i].active) {
		//                     ship.bullets[i].active = true;
		//                     ship.bullets[i].position.x = ship.position.x + (ship.radius * dx);
		//                     ship.bullets[i].position.y = ship.position.y + (ship.radius * dy);
		//                     ship.bullets[i].velocity.x = ship.velocity.x + (BULLET_SPEED * dx);
		//                     ship.bullets[i].velocity.y = ship.velocity.y + (BULLET_SPEED * dy);
		//                     ship.cooldown = BULLET_COOLDOWN;
		//                     break;
		//                 }
		//             }
		//         }
		//     }

		//     if (thrust != 0) {
		//         float dx = thrust * Mathf.Cos(DegToRad(heading));
		//         float dy = thrust * Mathf.Sin(DegToRad(heading));

		//         ship.velocity.x += dx;
		//         ship.velocity.y += dy;
		//         float mag = Mathf.Sqrt(ship.velocity.x * ship.velocity.x +
		//                          ship.velocity.y * ship.velocity.y);
		//         if (mag > SHIP_MAX_THRUST) {
		//             ship.velocity.x = (ship.velocity.x * SHIP_MAX_THRUST) / mag;
		//             ship.velocity.y = (ship.velocity.y * SHIP_MAX_THRUST) / mag;
		//         }
		//     }
		//     GGPORunner.LogGame($"new ship velocity: (dx:{ship.velocity.x} dy:{ship.velocity.y}).");

		//     ship.position.x += ship.velocity.x;
		//     ship.position.y += ship.velocity.y;
		//     GGPORunner.LogGame($"new ship position: (dx:{ship.position.x} dy:{ship.position.y}).");

		//     if (ship.position.x - ship.radius < _bounds.xMin ||
		//         ship.position.x + ship.radius > _bounds.xMax) {
		//         ship.velocity.x *= -1;
		//         ship.position.x += (ship.velocity.x * 2);
		//     }
		//     if (ship.position.y - ship.radius < _bounds.yMin ||
		//         ship.position.y + ship.radius > _bounds.yMax) {
		//         ship.velocity.y *= -1;
		//         ship.position.y += (ship.velocity.y * 2);
		//     }
		//     for (int i = 0; i < ship.bullets.Length; i++) {
		//         if (ship.bullets[i].active) {
		//             ship.bullets[i].position.x += ship.bullets[i].velocity.x;
		//             ship.bullets[i].position.y += ship.bullets[i].velocity.y;
		//             if (ship.bullets[i].position.x < _bounds.xMin ||
		//                 ship.bullets[i].position.y < _bounds.yMin ||
		//                 ship.bullets[i].position.x > _bounds.xMax ||
		//                 ship.bullets[i].position.y > _bounds.yMax) {
		//                 ship.bullets[i].active = false;
		//             }
		//             else {
		//                 for (int j = 0; j < _ships.Length; j++) {
		//                     var other = _ships[j];
		//                     if (Distance(ship.bullets[i].position, other.position) < other.radius) {
		//                         ship.score++;
		//                         other.health -= BULLET_DAMAGE;
		//                         ship.bullets[i].active = false;
		//                         break;
		//                     }
		//                 }
		//             }
		//         }
		//     }
		// }

		// public void LogInfo(string filename) {
		//     string fp = "";
		//     fp += "GameState object.\n";
		//     fp += string.Format("  bounds: {0},{1} x {2},{3}.\n", _bounds.xMin, _bounds.yMin, _bounds.xMax, _bounds.yMax);
		//     fp += string.Format("  num_ships: {0}.\n", _ships.Length);
		//     for (int i = 0; i < _ships.Length; i++) {
		//         var ship = _ships[i];
		//         fp += string.Format("  ship {0} position:  %.4f, %.4f\n", i, ship.position.x, ship.position.y);
		//         fp += string.Format("  ship {0} velocity:  %.4f, %.4f\n", i, ship.velocity.x, ship.velocity.y);
		//         fp += string.Format("  ship {0} radius:    %d.\n", i, ship.radius);
		//         fp += string.Format("  ship {0} heading:   %d.\n", i, ship.heading);
		//         fp += string.Format("  ship {0} health:    %d.\n", i, ship.health);
		//         fp += string.Format("  ship {0} cooldown:  %d.\n", i, ship.cooldown);
		//         fp += string.Format("  ship {0} score:     {1}.\n", i, ship.score);
		//         for (int j = 0; j < ship.bullets.Length; j++) {
		//             fp += string.Format("  ship {0} bullet {1}: {2} {3} -> {4} {5}.\n", i, j,
		//                     ship.bullets[j].position.x, ship.bullets[j].position.y,
		//                     ship.bullets[j].velocity.x, ship.bullets[j].velocity.y);
		//         }
		//     }
		//     File.WriteAllText(filename, fp);
		// }

		public void Update(long[] inputs, int disconnect_flags) {
			Framenumber++;
			for (int i = 0; i < _drifters.Length; i++) {
			    if ((disconnect_flags & (1 << i)) != 0) {
			        //GetShipAI(i);
			    }
			    else {
			        ParseDrifterInputs(inputs[i], i);
			    }
			}
		}

		public PlayerInputData ParseDrifterInputs(long inputs, int index) {

		    PlayerInputData input = new PlayerInputData();

		    GGPORunner.LogGame($"parsing ship {index} inputs: {inputs}.");


		    input.Jump = ((inputs & INPUT_JUMP) !=0);
			input.Light = ((inputs & INPUT_LIGHT) !=0);
			input.Special = ((inputs & INPUT_SPECIAL) !=0);
			input.Super = ((inputs & INPUT_SUPER) !=0);
			input.Guard = ((inputs & INPUT_GUARD) !=0);
			input.Grab = ((inputs & INPUT_GRAB) !=0);
			input.Dash = ((inputs & INPUT_DASH) !=0);
			input.Pause = ((inputs & INPUT_PAUSE) !=0);
			input.Menu = ((inputs & INPUT_MENU) !=0);


			if((inputs & INPUT_RIGHT) != 0)
				input.MoveX = 1;
			else if((inputs & INPUT_LEFT) != 0)
				input.MoveX = -1;

			if((inputs & INPUT_UP) !=0)
				input.MoveY = 1;
			else if((inputs & INPUT_DOWN) != 0)
				input.MoveY = -1;

		    return input;
		}

		public long ReadInputs(int id) {
			long input = 0;

			InputActionMap playerInputAction = GameController.Instance.controls[id].currentActionMap;

			//Binary Buttons
			if(playerInputAction.FindAction("Jump").ReadValue<float>() > 0)
				input |= INPUT_JUMP;
			if(playerInputAction.FindAction("Light").ReadValue<float>() > 0)
				input |= INPUT_LIGHT;
			if(playerInputAction.FindAction("Special").ReadValue<float>() > 0)
				input |= INPUT_SPECIAL;
			if(playerInputAction.FindAction("Super").ReadValue<float>() > 0)
				input |= INPUT_SUPER;
			if(playerInputAction.FindAction("Guard 1").ReadValue<float>() > 0)
				input |= INPUT_GUARD;
			if(playerInputAction.FindAction("Grab").ReadValue<float>() > 0)
				input |= INPUT_GRAB;
			if(playerInputAction.FindAction("Dash").ReadValue<float>() > 0)
				input |= INPUT_DASH;
			if(playerInputAction.FindAction("Start").ReadValue<float>() > 0)
				input |= INPUT_PAUSE;
			if(playerInputAction.FindAction("Menu").ReadValue<float>() > 0)
				input |= INPUT_MENU;

			//Directional Movement
			if((int)playerInputAction.FindAction("Horizontal").ReadValue<float>() > 0)
				input |= INPUT_RIGHT;
			if((int)playerInputAction.FindAction("Horizontal").ReadValue<float>() < 0)
				input |= INPUT_LEFT;

			if((int)playerInputAction.FindAction("Vertical").ReadValue<float>() > 0)
				input |= INPUT_UP;
			if((int)playerInputAction.FindAction("Vertical").ReadValue<float>() < 0)
				input |= INPUT_DOWN;


			return input;
		}

		public void LogInfo(string str){

		}

		public void FreeBytes(NativeArray<byte> data) {
		    if (data.IsCreated) {
		        data.Dispose();
		    }
		}

		// public override int GetHashCode() {
		//     int hashCode = -1214587014;
		//     hashCode = hashCode * -1521134295 + Framenumber.GetHashCode();
		//     foreach (var ship in _ships) {
		//         hashCode = hashCode * -1521134295 + ship.GetHashCode();
		//     }
		//     return hashCode;
		// }
	}
}