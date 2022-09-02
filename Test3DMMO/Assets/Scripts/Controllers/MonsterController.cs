using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterController : CreatureController
{
    protected override void Init()
    {
        Data.MonsterData data = null;
        Managers.Data.MonsterDict.TryGetValue(1, out data);

        if (data == null)
        {
            Debug.Log("존재하지 않는 몬스터 id입니다.");
            return;
        }

        Id = data.id;
        Name = data.name;
        MoveSpeed = data.stat.Speed;
        Hp = data.stat.MaxHp;
        TotalExp = data.stat.TotalExp;
        Damage = data.stat.Attack;
        MaxHp = data.stat.MaxHp;

        WorldPos = transform.position;
    }

    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        if (State == CreatureState.Dead)
            Debug.Log($"체력 : {Hp}... 사망");
    }
}
