using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using Server.Game.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.Game
{
	public partial class GameRoom : JobSerializer
	{
		public int RoomId { get; set; }

		Dictionary<int, Player> _players = new Dictionary<int, Player>();
		Dictionary<int, Monster> _monsters = new Dictionary<int, Monster>();


		//public Map Map { get; private set; } = new Map();
		
		public void Init(int mapId)
		{
            //Map.LoadMap(mapId);

            //// TEMP
            for (int i = 0; i < 1; i++)
            {
                Monster monster = ObjectManager.Instance.Add<Monster>(); // 몬스터 고유 아이디 생성
                monster.Init(1);
                EnterGame(monster);
            }
        }
		
		// 누군가 주기적으로 호출해줘야 한다
		public void Update()
		{
			Flush();
		}

		Random _rand = new Random();

		public void EnterGame(GameObject gameObject)
		{
			if (gameObject == null)
				return;

			GameObjectType type = ObjectManager.GetObjectTypeById(gameObject.Id);

			Player player = gameObject as Player;

			if (type == GameObjectType.Player)
			{
				player = gameObject as Player;
				_players.Add(gameObject.Id, player);
				player.Room = this;

				player.RefreshAdditionalStat();

				// 본인한테 정보 전송
				{
					S_EnterGame enterPacket = new S_EnterGame();
					enterPacket.Player = player.Info;
					player.Session.Send(enterPacket);
				}

                S_Spawn spawn1 = new S_Spawn();
                foreach (Player p in _players.Values)
                    spawn1.Objects.Add(p.Info);
                player.Session.Send(spawn1);

                S_Spawn spawn2 = new S_Spawn();
                foreach (Monster m in _monsters.Values)
                    spawn2.Objects.Add(m.Info);
                player.Session.Send(spawn2);
            }
            else if (type == GameObjectType.Monster)
            {
                Monster monster = gameObject as Monster;
                _monsters.Add(gameObject.Id, monster);
                monster.Room = this;
                monster.WorldPos = new Vector3(3.2f, 0f, 3.7f);

                monster.Update();
            }

            // 새로 들어온 플레이어는 _players와 _monsters에 있는 정보를 전달받아야 한다.
            // 그리고 새로 들어온 플레이어의 정보를 이미 존재하는 방의 _players에게 뿌려야 한다.
            

            
            

            // 타인한테 정보 전송
            {
				S_Spawn spawnPacket = new S_Spawn();
				spawnPacket.Objects.Add(gameObject.Info);
				Broadcast(spawnPacket);
			}
		}

		public void LeaveGame(int objectId)
		{
			GameObjectType type = ObjectManager.GetObjectTypeById(objectId);

			if (type == GameObjectType.Player)
			{
				Player player = null;
				if (_players.Remove(objectId, out player) == false)
					return;

				player.OnLeaveGame();
				player.Room = null;

				// 본인한테 정보 전송
				{
					S_LeaveGame leavePacket = new S_LeaveGame();
					player.Session.Send(leavePacket);
				}
			}
			//else if (type == GameObjectType.Monster)
			//{
			//	Monster monster = null;
			//	if (_monsters.Remove(objectId, out monster) == false)
			//		return;

			//	cellPos = monster.CellPos;
			//	Map.ApplyLeave(monster);
			//	monster.Room = null;
			//}
			else
			{
				return;
			}

			// 타인한테 정보 전송
			{
				S_Despawn despawnPacket = new S_Despawn();
				despawnPacket.ObjectIds.Add(objectId);
				Broadcast(despawnPacket);
			}
		}

		public Player FindPlayer(Func<GameObject, bool> condition)
		{
			foreach (Player player in _players.Values)
			{
				if (condition.Invoke(player))
					return player;
			}

			return null;
		}

		public void Broadcast(IMessage packet)
		{
			foreach (Player p in _players.Values)
            {
                p.Session.Send(packet);
			}
		}
	}
}
