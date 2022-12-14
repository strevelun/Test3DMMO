using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class PacketHandler : MonoBehaviour
{
	public static void S_EnterGameHandler(PacketSession session, IMessage packet)
	{
		S_EnterGame enterGamePacket = packet as S_EnterGame;
		Managers.Object.Add(enterGamePacket.Player, myPlayer: true);
	}

	public static void S_SpawnHandler(PacketSession session, IMessage packet)
	{
		S_Spawn spawnPacket = packet as S_Spawn;
		foreach (ObjectInfo obj in spawnPacket.Objects)
		{
			Managers.Object.Add(obj, myPlayer: false);
		}
	}

    public static void S_SkillHandler(PacketSession session, IMessage packet)
    {
        S_Skill skillPacket = packet as S_Skill;
        GameObject go = Managers.Object.FindById(skillPacket.Info.Attacker.ObjectId);
        PlayerController pc = go.GetComponent<PlayerController>();
        pc.State = CreatureState.Skill;
    }

    public static void S_MoveHandler(PacketSession session, IMessage packet)
    {
        S_Move movePacket = packet as S_Move;

        GameObject go = Managers.Object.FindById(movePacket.ObjectId);
        if (go == null)
            return;

        if (Managers.Object.MyPlayer.Id == movePacket.ObjectId)
            return;

      
        // State와 Blend 값 업데이트하고 UpdateAnimation

        GameObjectType objectType = ObjectManager.GetObjectTypeById(movePacket.ObjectId);

        if (objectType == GameObjectType.Player)
        {
            go.transform.position = new Vector3(movePacket.PosInfo.PosX, movePacket.PosInfo.PosY, movePacket.PosInfo.PosZ);
            go.transform.rotation = Quaternion.Euler(go.transform.rotation.x, movePacket.PosInfo.RotY, go.transform.rotation.z);

            PlayerController pc = go.GetComponent<PlayerController>();
            pc.State = movePacket.State;
            pc._animationBlend = movePacket.AnimationBlend;
            pc._inputMagnitude = movePacket.InputMagnitude;

        }
   //     else if (objectType == GameObjectType.Monster)
   //     {
   //         MonsterController mc = Util.GetOrAddComponent<MonsterController>(go);
   //         mc.State = movePacket.State;
			////mc.PosInfo.MergeFrom(movePacket.PosInfo);

			//mc.Move(new Vector3(movePacket.PosInfo.PosX, movePacket.PosInfo.PosY, movePacket.PosInfo.PosZ));
   //     }




        // TODO make these a property


    }

	public static void S_MonstermoveHandler(PacketSession session, IMessage packet)
    {
        S_Monstermove movePacket = packet as S_Monstermove;

        GameObject go = Managers.Object.FindById(movePacket.ObjectId);
		if (go == null)
			return;

        MonsterController mc = Util.GetOrAddComponent<MonsterController>(go);
		mc.Id = movePacket.ObjectId;
        mc.State = movePacket.State;
		mc.Stat.Speed = movePacket.Speed;

		// 서버로부터 오차 조정
		mc.WorldPos = new Vector3(movePacket.PosInfo.PosX, movePacket.PosInfo.PosY, movePacket.PosInfo.PosZ);

		mc.DestPos = new Vector3(movePacket.DestInfo.PosX, movePacket.DestInfo.PosY, movePacket.DestInfo.PosZ);
		Managers.UI.Log("현재좌표 : " + mc.WorldPos.ToString());
    }

    #region pended

    public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
	{
		S_LeaveGame leaveGameHandler = packet as S_LeaveGame;
		Managers.Object.Clear();
	}

	public static void S_DespawnHandler(PacketSession session, IMessage packet)
	{
		S_Despawn despawnPacket = packet as S_Despawn;
		foreach (int id in despawnPacket.ObjectIds)
		{
			Managers.Object.Remove(id);
		}
	}



	public static void S_ChangeHpHandler(PacketSession session, IMessage packet)
	{
		S_ChangeHp changePacket = packet as S_ChangeHp;

		GameObject go = Managers.Object.FindById(changePacket.ObjectId);
		if (go == null)
			return;

		//CreatureController cc = go.GetComponent<CreatureController>();
		//if (cc != null)
		//{
		//	cc.Hp = changePacket.Hp;
		//}
	}

	public static void S_DieHandler(PacketSession session, IMessage packet)
	{
		S_Die diePacket = packet as S_Die;

		GameObject go = Managers.Object.FindById(diePacket.ObjectId);
		if (go == null)
			return;

		//CreatureController cc = go.GetComponent<CreatureController>();
		//if (cc != null)
		//{
		//	cc.Hp = 0;
		//	cc.OnDead();
		//}
	}

	public static void S_ConnectedHandler(PacketSession session, IMessage packet)
	{
		Debug.Log("S_ConnectedHandler");
		C_Login loginPacket = new C_Login();

		string path = Application.dataPath;
		loginPacket.UniqueId = path.GetHashCode().ToString();
		Managers.Network.Send(loginPacket);
	}

	// 로그인 OK + 캐릭터 목록
	public static void S_LoginHandler(PacketSession session, IMessage packet)
	{
		S_Login loginPacket = (S_Login)packet;
		Debug.Log($"LoginOk({loginPacket.LoginOk})");

        // TODO : 로비 UI에서 캐릭터 보여주고, 선택할 수 있도록
        if (loginPacket.Players == null || loginPacket.Players.Count == 0)
        {
            C_CreatePlayer createPacket = new C_CreatePlayer();
            createPacket.Name = $"Player_{Random.Range(0, 10000).ToString("0000")}";
            Managers.Network.Send(createPacket);
        }
        else
        {
            // 무조건 첫번째 로그인
            LobbyPlayerInfo info = loginPacket.Players[0];
            C_EnterGame enterGamePacket = new C_EnterGame();
            enterGamePacket.Name = info.Name;
            Managers.Network.Send(enterGamePacket);
        }
    }

	public static void S_CreatePlayerHandler(PacketSession session, IMessage packet)
	{
		S_CreatePlayer createOkPacket = (S_CreatePlayer)packet;

        if (createOkPacket.Player == null)
        {
            C_CreatePlayer createPacket = new C_CreatePlayer();
            createPacket.Name = $"Player_{Random.Range(0, 10000).ToString("0000")}";
            Managers.Network.Send(createPacket);
        }
        else
        {
            C_EnterGame enterGamePacket = new C_EnterGame();
            enterGamePacket.Name = createOkPacket.Player.Name;
            Managers.Network.Send(enterGamePacket);
        }
    }

	public static void S_ItemListHandler(PacketSession session, IMessage packet)
	{
		S_ItemList itemList = (S_ItemList)packet;

		//Managers.Inven.Clear();

		//// 메모리에 아이템 정보 적용
		//foreach (ItemInfo itemInfo in itemList.Items)
		//{
		//	Item item = Item.MakeItem(itemInfo);
		//	Managers.Inven.Add(item);
		//}

		//if (Managers.Object.MyPlayer != null)
		//	Managers.Object.MyPlayer.RefreshAdditionalStat();
	}

	public static void S_AddItemHandler(PacketSession session, IMessage packet)
	{
		S_AddItem itemList = (S_AddItem)packet;

		//// 메모리에 아이템 정보 적용
		//foreach (ItemInfo itemInfo in itemList.Items)
		//{
		//	Item item = Item.MakeItem(itemInfo);
		//	Managers.Inven.Add(item);
		//}

		//Debug.Log("아이템을 획득했습니다!");

		//UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
		//gameSceneUI.InvenUI.RefreshUI();
		//gameSceneUI.StatUI.RefreshUI();

		//if (Managers.Object.MyPlayer != null)
		//	Managers.Object.MyPlayer.RefreshAdditionalStat();
	}

	public static void S_EquipItemHandler(PacketSession session, IMessage packet)
	{
		S_EquipItem equipItemOk = (S_EquipItem)packet;

		/*
		// 메모리에 아이템 정보 적용
		Item item = Managers.Inven.Get(equipItemOk.ItemDbId);
		if (item == null)
			return;

		item.Equipped = equipItemOk.Equipped;
		Debug.Log("아이템 착용 변경!");

		UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
		gameSceneUI.InvenUI.RefreshUI();
		gameSceneUI.StatUI.RefreshUI();

		if (Managers.Object.MyPlayer != null)
			Managers.Object.MyPlayer.RefreshAdditionalStat();
		*/
	}

	public static void S_ChangeStatHandler(PacketSession session, IMessage packet)
	{
		S_ChangeStat itemList = (S_ChangeStat)packet;

		// TODO
	}

	public static void S_PingHandler(PacketSession session, IMessage packet)
	{
		//C_Pong pongPacket = new C_Pong();
		//Debug.Log("[Server] PingCheck");
		//Managers.Network.Send(pongPacket);
	}
	
#endregion
}
