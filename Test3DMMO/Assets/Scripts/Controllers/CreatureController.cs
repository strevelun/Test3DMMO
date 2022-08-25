using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureController : MonoBehaviour
{
    public int Id { get; set; }

    PositionInfo _positionInfo = new PositionInfo();
    public PositionInfo PosInfo
    {
        get { return _positionInfo; }
        set
        {
            if (_positionInfo.Equals(value))
                return;

            WorldPos = new Vector3Int(value.PosX, value.PosY, value.PosZ);
            // State = value.State;
        }
    }

    public Vector3Int WorldPos
    {
        get
        {
            return new Vector3Int(PosInfo.PosX, PosInfo.PosY, PosInfo.PosZ);
        }

        set
        {
            if (PosInfo.PosX == value.x && PosInfo.PosY == value.y)
                return;

            PosInfo.PosX = value.x;
            PosInfo.PosY = value.y;
            PosInfo.PosZ = value.z;
            //_updated = true;
        }
    }

    // animation IDs
    protected int _animIDSpeed;
    protected int _animIDGrounded;
    protected int _animIDJump;
    protected int _animIDFreeFall;
    protected int _animIDMotionSpeed;
    protected int _animIDAttack;

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        _animIDAttack = Animator.StringToHash("Attack");
    }

    protected bool _hasAnimator;
    protected Animator _animator;

    protected virtual void Init()
    {
        _hasAnimator = transform.GetChild(0).TryGetComponent(out _animator);
        AssignAnimationIDs();
    }

    void Start()
    {
        Init();
    }

    void Update()
    {

        _hasAnimator = transform.GetChild(0).TryGetComponent(out _animator);
    }
}
