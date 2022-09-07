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
            // lock

            if (ObjectManager.Instance.Players.Count > 0)
            {
                float min = 9999999f;

                // 일정 거리 내에 플레이어가 존재한다면 그 플레이어의 좌표로 직선 이동

                foreach (Player p in ObjectManager.Instance.Players.Values)
                {
                    float temp = Vector3.Distance(p.Pos, WorldPos);

                    if (temp < min && temp <= _sightRange)
                    {
                        min = temp;
                        _target = p;
                        DestPos = _target.Pos;
                    }
                }
            }

            if (Vector3.Distance(WorldPos, DestPos) < 0.7f)
            {
                if (_target == null)
                {
                    float a = (float)rand.Next(1, 50);
                    float b = (float)rand.Next(1, 50);
                    DestPos = new Vector3(a, 0f, b);

                    BroadcastMove();
                }
                return;
            }



            WorldPos = Vector3.MoveTowards(WorldPos, DestPos, Speed * 0.016f);
            Console.WriteLine($"{WorldPos.x},\t {WorldPos.z} \t({Vector3.Distance(WorldPos, DestPos)}) =======>\t {DestPos.x},\t {DestPos.z}");

        }

		protected virtual void UpdateSkill() { 
        }

		protected virtual void UpdateDead() {
        }

	}
}
