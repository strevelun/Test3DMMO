using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game.Object
{
    public class Monster : GameObject
	{
		public int TemplateId { get; private set; }
		Player _target;
		IJob _job;

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

            // 초기화할 때 스폰 패킷
		}

        void BroadcastMove()
        {
            // 다른 플레이어한테도 알려준다
            S_Move movePacket = new S_Move();
            movePacket.ObjectId = Id;
            movePacket.PosInfo = PosInfo;

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

			// 5프레임 (0.2초마다 한번씩 Update)
			if (Room != null)
				_job = Room.PushAfter(200, Update);
		}

		protected virtual void UpdateIdle() {
            BroadcastMove();
        }

		protected virtual void UpdateMoving() {
            
        }

		protected virtual void UpdateSkill() { 
        }

		protected virtual void UpdateDead() { 

        }

	}
}
