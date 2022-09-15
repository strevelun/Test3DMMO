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
        private List<Vector3> _path;
        private int _curPathPos = 0;
		
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

        Vector3 GetRandomPos()
        {
            Random rand = new Random();
            float a, b;

            do
            {
                a = (float)rand.Next(1, 35);
                b = (float)rand.Next(1, 35);
            } while (!Room.Map.CanGo(new Vector3(a, 0f, b)));

            return new Vector3(a, 0f, b);
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

        Vector3 prevPos;
        bool flag1 = true;

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
                        _path = Room.Map.FindPath(WorldPos, DestPos, checkObjects: true);
                        _curPathPos = 1;
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
            if (_arrived && _target == null)
            {
                DestPos = GetRandomPos();
                _arrived = false; // 목적지에 도착할 때까지 랜덤 좌표 생성x
                _path = Room.Map.FindPath(WorldPos, DestPos, checkObjects: true);
                _curPathPos = 1;
                BroadcastMove();
                return;
            }

            if (_target != null)
            {
                // 타겟이 계속 움직인다면

                if (_path.Count < 2 || _path.Count > 100)
                {
                    _arrived = true;
                    Console.WriteLine("플레이어의 위치에 도착함");
                    //WorldPos = new Vector3(path[0].x, 0f, path[0].z);
                    _curPathPos = 1;
                    BroadcastMove();
                    return;
                }


                if (flag1 && _target == null)
                {
                    prevPos = _path[0];
                    flag1 = false;
                }
                else if (!flag1 && _target == null)
                {
                    if (prevPos == _path[1])
                    {
                        DestPos = GetRandomPos();
                        flag1 = true;
                        Console.WriteLine("플래그");
                        return;
                    }
                    flag1 = true;
                }
            }


            if (Vector3.Distance(WorldPos, new Vector3(_path[_curPathPos].x, 0f, _path[_curPathPos].z)) < 0.01f)
            {
                //WorldPos = new Vector3(path[1].x, 0f, path[1].z);
                WorldPos = Vector3.MoveTowards(WorldPos, new Vector3(_path[_curPathPos].x, 0f, _path[_curPathPos].z), Speed * 0.016f);
                _curPathPos++; // 각각의 노드에 도착하면 그 다음 노드로 이동

                if (_curPathPos >= _path.Count)
                {
                    _arrived = true;
                    //_curPathPos = 1;
                }

                BroadcastMove();
                return;
            }


            // 도달할 때까지 첫번쨰 노드로 이동
            WorldPos = Vector3.MoveTowards(WorldPos, new Vector3(_path[_curPathPos].x, 0f, _path[_curPathPos].z), Speed * 0.016f);


            BroadcastMove();

            Console.WriteLine($"패킷보냄 : {WorldPos.x},\t {WorldPos.z}");
        }

		protected virtual void UpdateSkill() { 
        }

		protected virtual void UpdateDead() {
        }

	}
}
