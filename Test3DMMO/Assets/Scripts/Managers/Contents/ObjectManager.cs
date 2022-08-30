using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager
{
    public MyPlayerController MyPlayer { get; set; }
    Dictionary<int, GameObject> _objects = new Dictionary<int, GameObject>();

	public static GameObjectType GetObjectTypeById(int id)
	{
		int type = (id >> 24) & 0x7F;
		return (GameObjectType)type;
	}

	public void Add(ObjectInfo info, bool myPlayer = false)
	{
		if (MyPlayer != null && MyPlayer.Id == info.ObjectId)
			return;
		if (_objects.ContainsKey(info.ObjectId))
			return;

		GameObjectType objectType = GetObjectTypeById(info.ObjectId);

		if (objectType == GameObjectType.Player)
		{
			if (myPlayer)
			{
				GameObject root = Managers.Resource.Instantiate("Character/Player");
				GameObject go = Managers.Resource.Instantiate("Character/Beginner");
				go.transform.parent = root.transform;
				root.name = info.Name;
				_objects.Add(info.ObjectId, root);

				MyPlayer = Util.GetOrAddComponent<MyPlayerController>(root);
				MyPlayer.Id = info.ObjectId;
				MyPlayer.PosInfo = info.PosInfo;
			}
			else
			{
				GameObject go = Managers.Resource.Instantiate("Character/Beginner");
				go.name = info.Name;
				_objects.Add(info.ObjectId, go);

				PlayerController pc = Util.GetOrAddComponent<PlayerController>(go);
				pc.Id = info.ObjectId;
				pc.PosInfo = info.PosInfo;

				go.transform.position = new Vector3(info.PosInfo.PosX, info.PosInfo.PosY, info.PosInfo.PosZ);
			}
		}
	}

	public void Remove(int id)
	{
		if (MyPlayer != null && MyPlayer.Id == id)
			return;
		if (_objects.ContainsKey(id) == false)
			return;

		GameObject go = FindById(id);
		if (go == null)
			return;

		_objects.Remove(id);
		Managers.Resource.Destroy(go);
	}

	public GameObject FindById(int id)
	{
		GameObject go = null;
		_objects.TryGetValue(id, out go);
		return go;
	}

	public GameObject Find(Func<GameObject, bool> condition)
	{
		foreach (GameObject obj in _objects.Values)
		{
			if (condition.Invoke(obj))
				return obj;
		}

		return null;
	}

	public void Clear()
	{
		foreach (GameObject obj in _objects.Values)
			Managers.Resource.Destroy(obj);
		_objects.Clear();
		MyPlayer = null;
	}
}
