using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using Server.Game.Object;
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
			p.WorldPos = new Vector3(movePosInfo.PosX, movePosInfo.PosY, movePosInfo.PosZ); // Pos -> WorldPos
			p.RotY = movePacket.PosInfo.RotY;
			p.State = movePacket.State;
			p._animationBlend = movePacket.AnimationBlend;
			p._inputMagnitude = movePacket.InputMagnitude;

            // 게임룸 딕셔너리 좌표 수정
            _players.TryGetValue(player.Id, out p);
            p.WorldPos = new Vector3(movePosInfo.PosX, movePosInfo.PosY, movePosInfo.PosZ);
            p.RotY = movePacket.PosInfo.RotY;
            p.State = movePacket.State;
            p._animationBlend = movePacket.AnimationBlend;
            p._inputMagnitude = movePacket.InputMagnitude;

            // 브로드캐스팅
            S_Move resMovePacket = new S_Move();
			resMovePacket.ObjectId = player.Info.ObjectId;
			resMovePacket.PosInfo = movePacket.PosInfo;
			resMovePacket.PosInfo.RotY = movePacket.PosInfo.RotY;	
			resMovePacket.State = movePacket.State;
			resMovePacket.AnimationBlend = movePacket.AnimationBlend;	
			resMovePacket.InputMagnitude = movePacket.InputMagnitude;	

			Broadcast(resMovePacket);
			
			Console.WriteLine($"C_Move ({movePacket.PosInfo.PosX}, {movePacket.PosInfo.PosY}, {movePacket.PosInfo.PosZ}, {movePacket.PosInfo.RotY}, {movePacket.State}, {movePacket.AnimationBlend}, {movePacket.InputMagnitude})");
		}

        public void HandleMonsterMove(C_Monstermove movePacket)
        {
            // (클라이언트 여러개로부터) 목적지에 다왔어요 패킷이 도착하면 서버좌표로 조정한 후 PosInfo와 destPos를 한번만 새롭게 내려보냄

            Monster monster;

            if(!_monsters.TryGetValue(movePacket.ObjectId, out monster))
            {
                Console.WriteLine($"{movePacket.ObjectId}은 서버 데이터에 존재하지 않음");
                return;
            }

            monster.WorldPos = new Vector3(movePacket.PosInfo.PosX, movePacket.PosInfo.PosY, movePacket.PosInfo.PosZ);

            if(Vector3.Distance(monster.WorldPos, monster.DestPos) < 0.1f)
            {
                monster.WorldPos = monster.DestPos;
                monster.State = CreatureState.Idle;
            }
        }
		
		public void HandleSkill(Player player, C_Skill skillPacket)
		{
			if (player == null)
				return;

            Console.WriteLine($"C_Skill {skillPacket.Info.Attacker}");
            Console.WriteLine($"{skillPacket.Info.Victim}");

			// 공격자의 공격력만큼 피해자의 체력을 깎는다.
			// 이 때 0보다 같거나 작으면 Dead상태를 브로드캐스팅
			// 제 3자의 입장에서 공격자의 애니메이션과 몬스터의 애니메이션이 동시에 출력되어 보이도록

			Player p = null;

			if (_players.TryGetValue(player.Id, out p) == false)
            {
				Console.WriteLine("플레이어의 id가 게임룸 데이터에 없음");
				return;
            }

            if (skillPacket.Info.Victim != null)
            {
                Monster m = null;

                if (_monsters.TryGetValue(skillPacket.Info.Victim.ObjectId, out m) == false)
                {
                    Console.WriteLine("몬스터 id가 게임룸 데이터에 없음");
                    return;
                }



                m.Hp -= p.TotalAttack;

                Console.WriteLine($"현재 체력 : {m.Hp}");

                if (m.Hp <= 0)
                    m.State = CreatureState.Dead;

                // 플레이어 위치로 destPos
            }

            ObjectManager.Instance.Find(player.Id).State = CreatureState.Skill;
			p.State = CreatureState.Skill;
            
			S_Skill skill = new S_Skill();
            skill.Info = skillPacket.Info;
            
			Broadcast(skill);

			/*
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
						Vector3 skillPos = player.GetFrontCellPos(info.PosInfo.MoveDir);
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
			*/
		}
	}
}
