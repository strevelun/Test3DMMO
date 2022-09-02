using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Server.Game
{
	public class GameObject
	{
		public GameObjectType ObjectType { get; protected set; } = GameObjectType.None;
		public int Id
		{
			get { return Info.ObjectId; }
			set { Info.ObjectId = value; }
		}

		public GameRoom Room { get; set; }

		public ObjectInfo Info { get; set; } = new ObjectInfo();
		public PositionInfo PosInfo { get; private set; } = new PositionInfo();
		public StatInfo Stat { get; private set; } = new StatInfo();

		public virtual int TotalAttack { get { return Stat.Attack; } }
		public virtual int TotalDefence { get { return 0; } }

		public float _animationBlend;
		public float _inputMagnitude;

		public float Speed
		{
			get { return Stat.Speed; }
			set { Stat.Speed = value; }
		}

		public int Hp
		{
			get { return Stat.Hp; }
			set { Stat.Hp = Math.Clamp(value, 0, Stat.MaxHp); }
		}

		public CreatureState State
		{
			get;
			set;
		}

        public Vector3 WorldPos
        {
            get { return new Vector3(PosInfo.PosX, PosInfo.PosY, PosInfo.PosZ); }
            set
            {
                PosInfo.PosX = value.x;
                PosInfo.PosY = value.y;
                PosInfo.PosZ = value.z;
            }
        }


        public GameObject()
		{
			Info.PosInfo = PosInfo;
			Info.Stat = Stat;
		}

		public virtual void Update()
		{

		}

		public virtual void OnDamaged(GameObject attacker, int damage)
		{
			if (Room == null)
				return;

			damage = Math.Max(damage - TotalDefence, 0);
			Stat.Hp = Math.Max(Stat.Hp - damage, 0);

			S_ChangeHp changePacket = new S_ChangeHp();
			changePacket.ObjectId = Id;
			changePacket.Hp = Stat.Hp;
			Room.Broadcast(changePacket);

			if (Stat.Hp <= 0)
			{
				OnDead(attacker);
			}
		}

		public virtual void OnDead(GameObject attacker)
		{
			if (Room == null)
				return;

			S_Die diePacket = new S_Die();
			diePacket.ObjectId = Id;
			diePacket.AttackerId = attacker.Id;
			Room.Broadcast(diePacket);

			GameRoom room = Room;
			room.LeaveGame(Id);

			Stat.Hp = Stat.MaxHp;
			State = CreatureState.Idle;

			room.EnterGame(this);
		}

		public virtual GameObject GetOwner()
		{
			return this;
		}
	}
}
