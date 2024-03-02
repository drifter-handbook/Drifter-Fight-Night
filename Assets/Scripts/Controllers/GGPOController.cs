// using SharedGame;
// using UnityGGPO;
// using System;
// using System.IO;
// using Unity.Collections;
// using UnityEngine;

// namespace DFN {

//     public class DNFGameManager : GameManager {

//         public override void StartLocalGame() {
//             StartGame(new LocalRunner(new DFNGame(2)));
//         }

//         public override void StartGGPOGame(IPerfUpdate perfPanel, IList<Connections> connections, int playerIndex) {
//             var game = new GGPORunner("DFN", new DFNGame(connections.Count), perfPanel);
//             game.Init(connections, playerIndex);
//             StartGame(game);
//         }
//     }

//     [Serializable]
//     public struct DFNGame : IGame {
//         public int Framenumber { get; private set; }

//         public int Checksum => GetHashCode();

//         public Drifter[] _drifters;

//         //public static Rect _bounds = new Rect(0, 0, 640, 480);

//         // public void Serialize(BinaryWriter bw) {
//         //     bw.Write(Framenumber);
//         //     bw.Write(_drifters.Length);
//         //     for (int i = 0; i < _drifters.Length; ++i) {
//         //         _drifters[i].Serialize(bw);
//         //     }
//         // }

//         // public void Deserialize(BinaryReader br) {
//         //     Framenumber = br.ReadInt32();
//         //     int length = br.ReadInt32();
//         //     if (length != _ships.Length) {
//         //         _ships = new Ship[length];
//         //     }
//         //     for (int i = 0; i < _ships.Length; ++i) {
//         //         _ships[i].Deserialize(br);
//         //     }
//         // }

//         public GaneRollbackFrame SerializeFrame(){

//         	DrifterFrames = new DrifterRollbackFrame[_drifters.Length];
//         	for(int i = 0; i < _drifters.Length; i++){
//         		DrifterFrames = _drifters[i].SerializeFrame();
//         	}

//         	return new GaneRollbackFrame() {
//         		Framenumber = Framenumber,
//         		Drifters = DrifterFrames,
//         	};

//         }

//         public void DeserializeFrame(GaneRollbackFrame g_frame){
//         	Framenumber = g_frame.Framenumber;
// 			for(int i = 0; i < _drifters.Length; i++){
//         		 _drifters[i] = g_frame.Drifters[i].DeserializeFrame();
//         	}

//         }

//         // public NativeArray<byte> ToBytes() {
//         //     using (var memoryStream = new MemoryStream()) {
//         //         using (var writer = new BinaryWriter(memoryStream)) {
//         //             Serialize(writer);
//         //         }
//         //         return new NativeArray<byte>(memoryStream.ToArray(), Allocator.Persistent);
//         //     }
//         // }

//         // public void FromBytes(NativeArray<byte> bytes) {
//         //     using (var memoryStream = new MemoryStream(bytes.ToArray())) {
//         //         using (var reader = new BinaryReader(memoryStream)) {
//         //             Deserialize(reader);
//         //         }
//         //     }
//         // }

//         /*
//          * InitGameState --
//          *
//          * Initialize our game state.
//          */

//         public DFNGame(int num_players, Drifter[] drifter) {
//             Framenumber = 0;
//             _drifters = drifters;
//         }

       

//         // public void ParseShipInputs(long inputs, int i, out float heading, out float thrust, out int fire) {
//         //     var ship = _ships[i];

//         //     GGPORunner.LogGame($"parsing ship {i} inputs: {inputs}.");

//         //     if ((inputs & INPUT_ROTATE_RIGHT) != 0) {
//         //         heading = (ship.heading - ROTATE_INCREMENT) % 360;
//         //     }
//         //     else if ((inputs & INPUT_ROTATE_LEFT) != 0) {
//         //         heading = (ship.heading + ROTATE_INCREMENT + 360) % 360;
//         //     }
//         //     else {
//         //         heading = ship.heading;
//         //     }

//         //     if ((inputs & INPUT_THRUST) != 0) {
//         //         thrust = SHIP_THRUST;
//         //     }
//         //     else if ((inputs & INPUT_BREAK) != 0) {
//         //         thrust = -SHIP_THRUST;
//         //     }
//         //     else {
//         //         thrust = 0;
//         //     }
//         //     fire = (int)(inputs & INPUT_FIRE);
//         // }

//         // public void MoveShip(int index, float heading, float thrust, int fire) {
//         //     var ship = _ships[index];

//         //     GGPORunner.LogGame($"calculation of new ship coordinates: (thrust:{thrust} heading:{heading}).");

//         //     ship.heading = heading;

//         //     if (ship.cooldown == 0) {
//         //         if (fire != 0) {
//         //             GGPORunner.LogGame("firing bullet.");
//         //             for (int i = 0; i < ship.bullets.Length; i++) {
//         //                 float dx = Mathf.Cos(DegToRad(ship.heading));
//         //                 float dy = Mathf.Sin(DegToRad(ship.heading));
//         //                 if (!ship.bullets[i].active) {
//         //                     ship.bullets[i].active = true;
//         //                     ship.bullets[i].position.x = ship.position.x + (ship.radius * dx);
//         //                     ship.bullets[i].position.y = ship.position.y + (ship.radius * dy);
//         //                     ship.bullets[i].velocity.x = ship.velocity.x + (BULLET_SPEED * dx);
//         //                     ship.bullets[i].velocity.y = ship.velocity.y + (BULLET_SPEED * dy);
//         //                     ship.cooldown = BULLET_COOLDOWN;
//         //                     break;
//         //                 }
//         //             }
//         //         }
//         //     }

//         //     if (thrust != 0) {
//         //         float dx = thrust * Mathf.Cos(DegToRad(heading));
//         //         float dy = thrust * Mathf.Sin(DegToRad(heading));

//         //         ship.velocity.x += dx;
//         //         ship.velocity.y += dy;
//         //         float mag = Mathf.Sqrt(ship.velocity.x * ship.velocity.x +
//         //                          ship.velocity.y * ship.velocity.y);
//         //         if (mag > SHIP_MAX_THRUST) {
//         //             ship.velocity.x = (ship.velocity.x * SHIP_MAX_THRUST) / mag;
//         //             ship.velocity.y = (ship.velocity.y * SHIP_MAX_THRUST) / mag;
//         //         }
//         //     }
//         //     GGPORunner.LogGame($"new ship velocity: (dx:{ship.velocity.x} dy:{ship.velocity.y}).");

//         //     ship.position.x += ship.velocity.x;
//         //     ship.position.y += ship.velocity.y;
//         //     GGPORunner.LogGame($"new ship position: (dx:{ship.position.x} dy:{ship.position.y}).");

//         //     if (ship.position.x - ship.radius < _bounds.xMin ||
//         //         ship.position.x + ship.radius > _bounds.xMax) {
//         //         ship.velocity.x *= -1;
//         //         ship.position.x += (ship.velocity.x * 2);
//         //     }
//         //     if (ship.position.y - ship.radius < _bounds.yMin ||
//         //         ship.position.y + ship.radius > _bounds.yMax) {
//         //         ship.velocity.y *= -1;
//         //         ship.position.y += (ship.velocity.y * 2);
//         //     }
//         //     for (int i = 0; i < ship.bullets.Length; i++) {
//         //         if (ship.bullets[i].active) {
//         //             ship.bullets[i].position.x += ship.bullets[i].velocity.x;
//         //             ship.bullets[i].position.y += ship.bullets[i].velocity.y;
//         //             if (ship.bullets[i].position.x < _bounds.xMin ||
//         //                 ship.bullets[i].position.y < _bounds.yMin ||
//         //                 ship.bullets[i].position.x > _bounds.xMax ||
//         //                 ship.bullets[i].position.y > _bounds.yMax) {
//         //                 ship.bullets[i].active = false;
//         //             }
//         //             else {
//         //                 for (int j = 0; j < _ships.Length; j++) {
//         //                     var other = _ships[j];
//         //                     if (Distance(ship.bullets[i].position, other.position) < other.radius) {
//         //                         ship.score++;
//         //                         other.health -= BULLET_DAMAGE;
//         //                         ship.bullets[i].active = false;
//         //                         break;
//         //                     }
//         //                 }
//         //             }
//         //         }
//         //     }
//         // }

//         // public void LogInfo(string filename) {
//         //     string fp = "";
//         //     fp += "GameState object.\n";
//         //     fp += string.Format("  bounds: {0},{1} x {2},{3}.\n", _bounds.xMin, _bounds.yMin, _bounds.xMax, _bounds.yMax);
//         //     fp += string.Format("  num_ships: {0}.\n", _ships.Length);
//         //     for (int i = 0; i < _ships.Length; i++) {
//         //         var ship = _ships[i];
//         //         fp += string.Format("  ship {0} position:  %.4f, %.4f\n", i, ship.position.x, ship.position.y);
//         //         fp += string.Format("  ship {0} velocity:  %.4f, %.4f\n", i, ship.velocity.x, ship.velocity.y);
//         //         fp += string.Format("  ship {0} radius:    %d.\n", i, ship.radius);
//         //         fp += string.Format("  ship {0} heading:   %d.\n", i, ship.heading);
//         //         fp += string.Format("  ship {0} health:    %d.\n", i, ship.health);
//         //         fp += string.Format("  ship {0} cooldown:  %d.\n", i, ship.cooldown);
//         //         fp += string.Format("  ship {0} score:     {1}.\n", i, ship.score);
//         //         for (int j = 0; j < ship.bullets.Length; j++) {
//         //             fp += string.Format("  ship {0} bullet {1}: {2} {3} -> {4} {5}.\n", i, j,
//         //                     ship.bullets[j].position.x, ship.bullets[j].position.y,
//         //                     ship.bullets[j].velocity.x, ship.bullets[j].velocity.y);
//         //         }
//         //     }
//         //     File.WriteAllText(filename, fp);
//         // }

//         public void Update(long[] inputs, int disconnect_flags) {
//             Framenumber++;
//             for (int i = 0; i < _ships.Length; i++) {
//                 float thrust, heading;
//                 int fire;

//                 if ((disconnect_flags & (1 << i)) != 0) {
//                     GetShipAI(i, out heading, out thrust, out fire);
//                 }
//                 else {
//                     ParseShipInputs(inputs[i], i, out heading, out thrust, out fire);
//                 }
//                 MoveShip(i, heading, thrust, fire);

//                 if (_ships[i].cooldown != 0) {
//                     _ships[i].cooldown--;
//                 }
//             }
//         }

//         public long ReadInputs(int id) {
//             long input = 0;

//             if (id == 0) {
//                 if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.UpArrow)) {
//                     input |= INPUT_THRUST;
//                 }
//                 if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.DownArrow)) {
//                     input |= INPUT_BREAK;
//                 }
//                 if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.LeftArrow)) {
//                     input |= INPUT_ROTATE_LEFT;
//                 }
//                 if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.RightArrow)) {
//                     input |= INPUT_ROTATE_RIGHT;
//                 }
//                 if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.RightControl)) {
//                     input |= INPUT_FIRE;
//                 }
//                 if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.RightShift)) {
//                     input |= INPUT_BOMB;
//                 }
//             }
//             else if (id == 1) {
//                 if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.W)) {
//                     input |= INPUT_THRUST;
//                 }
//                 if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.S)) {
//                     input |= INPUT_BREAK;
//                 }
//                 if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.A)) {
//                     input |= INPUT_ROTATE_LEFT;
//                 }
//                 if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.D)) {
//                     input |= INPUT_ROTATE_RIGHT;
//                 }
//                 if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.F)) {
//                     input |= INPUT_FIRE;
//                 }
//                 if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.G)) {
//                     input |= INPUT_BOMB;
//                 }
//             }

//             return input;
//         }

//         // public void FreeBytes(NativeArray<byte> data) {
//         //     if (data.IsCreated) {
//         //         data.Dispose();
//         //     }
//         // }

//         // public override int GetHashCode() {
//         //     int hashCode = -1214587014;
//         //     hashCode = hashCode * -1521134295 + Framenumber.GetHashCode();
//         //     foreach (var ship in _ships) {
//         //         hashCode = hashCode * -1521134295 + ship.GetHashCode();
//         //     }
//         //     return hashCode;
//         // }
//     }

//     public class GaneRollbackFrame: INetworkData {
// 		public string Type { get; set; }
// 		public int Framenumber;
// 		public DrifterRollbackFrame[] Drifters;
// 	}
// }