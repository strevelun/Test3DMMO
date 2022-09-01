using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
	public partial class GameRoom : JobSerializer
	{

		public void HandleMove(Player player, C_Move movePacket)
		{
			if (player == null)
				return;

			// TODO : 검증... 현재 플레이어의 위치가 잘못된 곳에 있을 경우?
			PositionInfo movePosInfo = movePacket.PosInfo;

			ObjectInfo info = player.Info;

			// 서버에서 플레이어 좌표 수정
			Player p = ObjectManager.Instance.Find(info.ObjectId);
			p.Pos = new Vector3(movePosInfo.PosX, movePosInfo.PosY, movePosInfo.PosZ);
			p.RotY = movePacket.RotY;
			p.State = movePacket.State;
			p._animationBlend = movePacket.AnimationBlend;
			p._inputMagnitude = movePacket.InputMagnitude;

			// 브로드캐스팅
			S_Move resMovePacket = new S_Move();
			resMovePacket.ObjectId = player.Info.ObjectId;
			resMovePacket.PosInfo = movePacket.PosInfo;
			resMovePacket.RotY = movePacket.RotY;	
			resMovePacket.State = movePacket.State;
			resMovePacket.AnimationBlend = movePacket.AnimationBlend;	
			resMovePacket.InputMagnitude = movePacket.InputMagnitude;	

			Broadcast(resMovePacket);
			
			Console.WriteLine($"C_Move ({movePacket.PosInfo.PosX}, {movePacket.PosInfo.PosY}, {movePacket.PosInfo.PosZ}, {movePacket.RotY}, {movePacket.State}, {movePacket.AnimationBlend}, {movePacket.InputMagnitude})");
		}
		/*
		public void HandleSkill(Player player, C_Skill skillPacket)
		{
			if (player == null)
				return;

			ObjectInfo info = player.Info;
			if (info.PosInfo.State != CreatureState.Idle)
				return;

			// TODO : 스킬 사용 가능 여부 체크
			info.PosInfo.State = CreatureState.Skill;
			S_Skill skill = new S_Skill() { Info = new SkillInfo() };
			skill.ObjectId = info.ObjectId;
			skill.Info.SkillId = skillPacket.Info.SkillId;
			Broadcast(player.CellPos, skill);

			Data.Skill skillData = null;
			if (DataManager.SkillDict.TryGetValue(skillPacket.Info.SkillId, out skillData) == false)
				return;

			switch (skillData.skillType)
			{
				case SkillType.SkillAuto:
					{
						Vector2Int skillPos = player.GetFrontCellPos(info.PosInfo.MoveDir);
						GameObject target = Map.Find(skillPos);
						if (target != null)
						{
							Console.WriteLine("Hit GameObject !");
						}
					}
					break;
				case SkillType.SkillProjectile:
					{
						Arrow arrow = ObjectManager.Instance.Add<Arrow>();
						if (arrow == null)
							return;

						arrow.Owner = player;
						arrow.Data = skillData;
						arrow.PosInfo.State = CreatureState.Moving;
						arrow.PosInfo.MoveDir = player.PosInfo.MoveDir;
						arrow.PosInfo.PosX = player.PosInfo.PosX;
						arrow.PosInfo.PosY = player.PosInfo.PosY;
						arrow.Speed = skillData.projectile.speed;
						Push(EnterGame, arrow, false);
					}
					break;
			}
		}
		*/
	}
}
