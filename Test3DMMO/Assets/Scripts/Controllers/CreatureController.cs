using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureController : MonoBehaviour
{
    public int Id { get; set; }
    public string Name { get; set; }

    PositionInfo _positionInfo = new PositionInfo();
    public PositionInfo PosInfo
    {
        get { return _positionInfo; }
        set
        {
            if (_positionInfo.Equals(value))
                return;

            WorldPos = new Vector3(value.PosX, value.PosY, value.PosZ);
            // State = value.State;
        }
    }

    public float RotY { get; protected set; }

    public Vector3 WorldPos
    {
        get
        {
            return new Vector3(PosInfo.PosX, PosInfo.PosY, PosInfo.PosZ);
        }

        set
        {
            if (PosInfo.PosX == value.x && PosInfo.PosY == value.y && PosInfo.PosZ == value.z)
                return;

            PosInfo.PosX = value.x;
            PosInfo.PosY = value.y;
            PosInfo.PosZ = value.z;
            //_updated = true;
        }
    }

    StatInfo _stat = new StatInfo();
    public virtual StatInfo Stat
    {
        get { return _stat; }
        set
        {
            if (_stat.Equals(value))
                return;

            _stat.MergeFrom(value);
        }
    }

    protected float MoveSpeed
    {
        get { return Stat.Speed; }
        set { Stat.Speed = value; }
    }

    public virtual int TotalExp
    {
        get { return Stat.TotalExp; }
        set { Stat.TotalExp = value; }
    }

    public virtual int Level
    {
        get { return Stat.Level; }
        set { Stat.Level = value; } 
    }

    public int Hp
    {
        get { return Stat.Hp; }
        set
        {
            Stat.Hp = value;
        }
    }

    public int MaxHp { get; set; }

    public int Damage { get; set; }

    CreatureState _creatureState;   
    public virtual CreatureState State
    {
        get { return _creatureState; }  
        set
        {
            _creatureState = value;
            
            UpdateAnimation();
        }
    }

    protected int _animIDSpeed;
    protected int _animIDGrounded;
    protected int _animIDJump;
    protected int _animIDFreeFall;
    protected int _animIDMotionSpeed;
    protected int _animIDAttack;
    protected int _animIDDead;

    public float _animationBlend;
    public float _inputMagnitude;

    protected bool _isIdle;

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        _animIDAttack = Animator.StringToHash("Attack");
        _animIDDead = Animator.StringToHash("Dead");
    }

    protected bool _hasAnimator;
    protected Animator _animator;

    protected virtual void Init()
    {
        _hasAnimator = transform.GetChild(1).TryGetComponent(out _animator);

        AssignAnimationIDs();
    }

    void Start()
    {
        Init();
    }

    void Update()
    {
        _hasAnimator = transform.GetChild(1).TryGetComponent(out _animator);
    }

    protected virtual void UpdateAnimation()
    {
        if (_animator == null)
            return;

        Debug.Log("CreatureController?????? ??????");
        /*
        _animator.SetFloat(_animIDSpeed, _animationBlend);
        _animator.SetFloat(_animIDMotionSpeed, _inputMagnitude);

        if (State == CreatureState.Idle)
        {
            _animator.Play(_animIDGrounded);
        }
        else if (State == CreatureState.Skill)
        {

        }
        else if (State == CreatureState.Dead)
        {

        }
        */
    }
}
