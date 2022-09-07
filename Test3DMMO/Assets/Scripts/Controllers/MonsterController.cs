using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterController : CreatureController
{
    public Vector3 DestPos { get; set; }


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

        DestPos = WorldPos;
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

        
        UpdateMove();

        //WorldPos = transform.position;
    }

    void UpdateMove()
    {

        if(State == CreatureState.Idle)
        {
            
        }
        else if (State == CreatureState.Moving)
        {
            Move();
        }
    }

    public void Move()
    {
        C_Monstermove move = new C_Monstermove();

        move.ObjectId = Id;
        move.PosInfo = PosInfo;

        if (Vector3.Distance(WorldPos, DestPos) < 0.1f)
        {
            move.State = CreatureState.Idle;
            Managers.Network.Send(move);
            State = CreatureState.Idle;
            return;
        }

        move.State = CreatureState.Moving;
        Managers.Network.Send(move);

        transform.position = Vector3.MoveTowards(transform.position, DestPos, Stat.Speed * Time.deltaTime);
        WorldPos = transform.position;
        Managers.UI.Log(WorldPos.ToString());
    }

    void SendMonsterMovePacket()
    {

    }
}
