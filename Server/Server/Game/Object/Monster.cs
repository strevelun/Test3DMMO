using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks.Dataflow;

namespace Server.Game.Object
{
    public class Monster : GameObject
	{
		public int TemplateId { get; private set; }
        public Vector3 DestPos { get; set; }
		
        Player _target;
		IJob _job;
        float _sightRange = 10.0f;
        bool _arrived = false;
        bool _nodeArrived = false;
        float _chaseCellDist = 5f;

        public Monster()
		{
			ObjectType = GameObjectType.Monster;
		}

		public void Init(int templateId)
		{
			TemplateId = templateId;

			MonsterData monsterData = null;
			DataManager.MonsterDict.TryGetValue(TemplateId, out monsterData);
			Stat.MergeFrom(monsterData.stat);
			State = CreatureState.Idle;

            // 60fps
            Speed = 5f;
            // 초기화할 때 스폰 패킷
		}

        void BroadcastMove()
        {
            S_Monstermove movePacket = new S_Monstermove();

            //if (Vector3.Distance(WorldPos, DestPos) < 0.01f)
            //    movePacket.State = CreatureState.Idle;
            //else
            //    movePacket.State = CreatureState.Moving;

            movePacket.ObjectId = Id;
            movePacket.PosInfo = PosInfo;
            movePacket.DestInfo = new PositionInfo();
            movePacket.DestInfo.PosX = DestPos.x;
            movePacket.DestInfo.PosY = DestPos.y;
            movePacket.DestInfo.PosZ = DestPos.z;
            movePacket.Speed = Speed;

            //WorldPos = new Vector3(PosInfo.PosX, PosInfo.PosY, PosInfo.PosZ);

            //Console.WriteLine(PosInfo);
            

            
            Room.Broadcast(movePacket);
        }

        // 1. 몬스터가 가만히 있는 상황에서 계속 주변 플레이어 탐색
        // 2. 몬스터가 이동 중 쫓는 플레이어가 없을 경우 주변 플레이어 탐색. 

        // 플레이어를 쫓다가 플레이어가 시야 밖으로 나갈경우 다시 새로운 destPos를 설정하고 순찰

        public override void Update()
		{

			switch (State)
			{
				case CreatureState.Idle:
					UpdateIdle();
					break;
				case CreatureState.Moving:
					UpdateMoving();
					break;
				case CreatureState.Skill:
					UpdateSkill();
					break;
				case CreatureState.Dead:
					UpdateDead();
					break;
			}

			if (Room != null)
				_job = Room.PushAfter(20, Update);
		}

		protected virtual void UpdateIdle() {
            State = CreatureState.Moving;
        }

        Random rand = new Random();

        protected virtual void UpdateMoving()
        {
            // 단순 목적지 직선이동 or 플레이어 위치로 길찾기
            if (ObjectManager.Instance.Players.Count > 0) // 플레이어 위치가 계속 변하기 떄문에 항상 주변 탐색
            {
                float min = 9999999f;

                bool flag = false;

                foreach (Player p in Room._players.Values)
                {
                    float temp = Vector3.Distance(p.WorldPos, WorldPos);

                    if (temp < min && temp <= _sightRange)
                    {
                        min = temp;
                        _target = p;
                        flag = true;
                        DestPos = _target.WorldPos;

                        Vector3 t = DestPos;
                        t.y = 0f;
                        DestPos = t;
                    }
                }

                if (!flag) // 주변에 조건에 맞는 플레이어가 없으면 null로 세팅
                    _target = null;
            }

            if (Vector3.Distance(WorldPos, DestPos) < 0.7f)
            {
                _arrived = true;
            }

            // 목적지에 도착했고 타겟이 없는 경우
            if ((_arrived && _target == null))
            {
                float a, b;
                do {
                    a = (float)rand.Next(1, 35);
                    b = (float)rand.Next(1, 20);
                } while (!Room.Map.CanGo(new Vector3(a, 0f, b)));

                DestPos = new Vector3(a, 0f, b);
                _arrived = false; // 목적지에 도착할 때까지 랜덤 좌표 생성x

                BroadcastMove();
                return;
            }



                // 1. DestPos가 정해졌으면 A*를 돌린다.
                // 2. 첫번째 노드에 도달할 때까지 MoveTowards. 동시에 실시간으로 주변 탐색


            List<Vector3> path = Room.Map.FindPath(WorldPos, DestPos, checkObjects: true);
            
            

            if (path.Count < 2 || path.Count > 100)
            {
                _arrived = true;
                Console.WriteLine("리턴");
                WorldPos = new Vector3(path[0].x, 0f, path[0].z);
                BroadcastMove();
                return;
            }

            //if (!_nodeArrived) 
            {
                if (Vector3.Distance(WorldPos, new Vector3(path[1].x, 0f, path[1].z)) < 0.1f)
                {
                    WorldPos = Vector3.MoveTowards(WorldPos, new Vector3(path[1].x, 0f, path[1].z), Speed * 0.016f);
                    BroadcastMove();
                    return;
                    //_nodeArrived = true;
                }

                // 도달할 때까지 첫번쨰 노드로 이동
                WorldPos = Vector3.MoveTowards(WorldPos, new Vector3(path[1].x, 0f, path[1].z), Speed * 0.016f);
                BroadcastMove();

                Console.WriteLine($"{WorldPos.x},\t {WorldPos.z} \t({Vector3.Distance(WorldPos, new Vector3(path[1].x, 0f, path[1].z))}) =======>\t {path[1].x},\t {path[1].z}");
            }













            //if (!_nodeArrived)
            //{
                
            //    if (Vector3.Distance(WorldPos, new Vector3(path[1].x, 0f, path[1].y)) > 0.1f)
            //    {
            //        WorldPos = Vector3.MoveTowards(WorldPos, new Vector3(path[1].x, 0f, path[1].y), Speed * 0.016f);
            //        Console.WriteLine($"{WorldPos.x}, {WorldPos.z}");
            //        BroadcastMove();
            //        _nodeArrived = false;
            //        return;
            //    }
            //    else
            //        _nodeArrived = true;

            //}

            //WorldPos = Vector3.MoveTowards(WorldPos, DestPos, Speed * 0.016f);
            //BroadcastMove();
            // Console.WriteLine($"{WorldPos.x},\t {WorldPos.z} \t({Vector3.Distance(WorldPos, DestPos)}) =======>\t {DestPos.x},\t {DestPos.z}");

        }

		protected virtual void UpdateSkill() { 
        }

		protected virtual void UpdateDead() {
        }

	}
}
