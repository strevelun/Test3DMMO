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
		Player _target;
		IJob _job;
        public Vector3 DestPos { get; private set; }

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

            DestPos = new Vector3(100f, 0f, 100f);

            // 60fps
            Speed = 5f;
            // 초기화할 때 스폰 패킷
		}

        void BroadcastMove()
        {
            S_Monstermove movePacket = new S_Monstermove();

            if (Vector3.Distance(WorldPos, DestPos) < 1f)
            {
                movePacket.State = CreatureState.Idle;
            }
            else
                movePacket.State = CreatureState.Moving;

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

			// 30 fps
			if (Room != null)
				_job = Room.PushAfter(20, Update);
		}

		protected virtual void UpdateIdle() {
            State = CreatureState.Moving;
        }

		protected virtual void UpdateMoving() {

            BroadcastMove();
        }

		protected virtual void UpdateSkill() { 
        }

		protected virtual void UpdateDead() { 

        }

	}
}
