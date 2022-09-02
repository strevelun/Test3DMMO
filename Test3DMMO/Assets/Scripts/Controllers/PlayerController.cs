using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : CreatureController
{
    [Header("플레이어")]
    public float _sprintSpeed = 10.0f;
    public float _rotationSmoothTime = 0.12f; // 캐릭터 회전 속도
    public float _speedChangeRate = 10.0f; // 가속/감속

    public AudioClip _landingAudioClip;
    public AudioClip[] _footstepAudioClips;
    [Range(0, 1)] public float _footstepAudioVolume = 0.5f;

    [Space(10)]
    public float _jumpHeight = 1.2f;
    public float _gravity = -15.0f; // 캐릭터 고유 중력

    [Space(10)]
    public LayerMask _pushLayers;
    public bool _canPush;
    [Range(0.5f, 5f)] public float _strength = 1.1f;

    protected float _jumpTimeout = 0.7f; // 착지 애니메이션 시간. (TODO 추후 착지 애니메이션 빠르게 수정)
    protected float _fallTimeout = 0.15f; // fall 상태 이전 통과 필요 시간. 계단을 내려갈 때 유용. (최소 이 시간이 지나야 freeFall 상태 모션으로 들어감)

    [Header("플레이어 지면 체크")]
    public bool _grounded = true;
    public float _groundedOffset = -0.28f; // 울퉁불퉁한 지면에서 유용
    public float _groundedRadius = 0.33f; // grounded check의 반지름. 캐릭터 컨트롤러의 반지름과 일치해야 함
    public LayerMask GroundLayers;

    [Header("시네머신")]
    public GameObject CinemachineCameraTarget;
    public float _topClamp = 70.0f; // 카메라 최대각
    public float _bottomClamp = -30.0f; // 카메라 최소각


    protected Coroutine _coSkill;
    protected float _lastYPos;



    protected override void Init()
    {
        base.Init();

        if (!_hasAnimator)
            _hasAnimator = transform.TryGetComponent(out _animator);
        UpdateAnimation();
    }

    void Start()
    {
        Init();
    }

    private void Update()
    {
        UpdateAnimation();
    }

    protected virtual void Move()
    {
        
    }

    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (_grounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;

        Gizmos.DrawSphere(
            new Vector3(transform.position.x, transform.position.y - _groundedOffset, transform.position.z),
            _groundedRadius);
    }

    protected virtual void PushRigidBodies(ControllerColliderHit hit)
    {
        
    }

    protected override void UpdateAnimation()
    {
        if (_animator == null)
            return;

        //Debug.Log(_grounded);

        _animator.SetFloat(_animIDSpeed, _animationBlend);
        _animator.SetFloat(_animIDMotionSpeed, _inputMagnitude);


        if (State == CreatureState.Inair)
        {
            _animator.SetBool(_animIDFreeFall, true);
            _animator.SetBool(_animIDJump, false);
            _animator.SetBool(_animIDGrounded, false);
        }
        else if (State == CreatureState.Jump)
        {
            _animator.SetBool(_animIDJump, true);
            _animator.SetBool(_animIDGrounded, false);
        }
        else if (State == CreatureState.Idle)
        {
            // idle일때 애니메이션 재생 x
            _animator.SetBool(_animIDGrounded, true);
            _animator.SetBool(_animIDJump, false);
            _animator.SetBool(_animIDFreeFall, false);
        }
        else if (State == CreatureState.Moving)
        {
            if (_grounded)
            {
                _animator.SetBool(_animIDJump, false);
                _animator.SetBool(_animIDFreeFall, false);
                _animator.SetBool(_animIDGrounded, true);
            }
        }
        else if (State == CreatureState.Skill)
        {
            _animator.SetTrigger(_animIDAttack);
        }
        else if (State == CreatureState.Dead)
        {
            _animator.SetBool(_animIDDead, true);
        }
        
        
    }
}
