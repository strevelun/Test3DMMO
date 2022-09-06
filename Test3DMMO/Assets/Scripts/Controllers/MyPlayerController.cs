using Cinemachine;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MyPlayerController : PlayerController
{
    private CinemachineVirtualCamera _playerFollowCamera;

    private PlayerInput _playerInput;

    private CharacterController _controller;
    private PlayerInputSystem _input;
    private GameObject _mainCamera;

    private const float _threshold = 0.01f;

    // cinemachine
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;

    public float _cameraAngleOverride = 0.0f; // lock 상태일 때 카메라 위치 미세조정할 때 유용
    public bool _lockCameraPosition = false; // 카메라 모든 축 잠금

    // player
    private float _speed;
    private float _targetRotation = 0.0f;
    private float _rotationVelocity;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;

    // timeout deltatime
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    

    private bool IsCurrentDeviceMouse
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            return _playerInput.currentControlScheme == "KeyboardMouse";
#else
            return false;
#endif
        }
    }

    private void Awake()
    {
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
    }

    private void Update()
    {

        JumpAndGravity();
        GroundedCheck();
        Move();
        //Debug.Log(_grounded);
        Attack();
        CheckDead();




        Debug.DrawRay(transform.position + Vector3.up, Vector3.forward * 2, Color.red);
    }

    private void CheckDead()
    {
        // Dead 모션 테스트
        //State = CreatureState.Dead;
    }

    protected override void Init()
    {
        base.Init();

        _playerFollowCamera = FindObjectOfType<CinemachineVirtualCamera>();
        _playerFollowCamera.GetComponent<CinemachineVirtualCamera>().Follow = transform.Find("PlayerCameraRoot").transform;

        _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
        _controller = GetComponent<CharacterController>();
        _input = GetComponent<PlayerInputSystem>();
#if ENABLE_INPUT_SYSTEM
        _playerInput = GetComponent<PlayerInput>();
#endif

        _jumpTimeoutDelta = _jumpTimeout;
        _fallTimeoutDelta = _fallTimeout;

        WorldPos = transform.position;

        Name = "테스트 이름";
        Stat.Attack = 5;
        Stat.Hp = 10;
        Stat.MaxHp = 10;
        Stat.Level = 1;
        Stat.Speed = 5;

    }

    void Attack()
    {
        RaycastHit hit;
        LayerMask mask =  1 << (int)Define.Layers.Monster;

        MonsterController monsterController = null;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 vec = transform.position;
            vec.y = vec.y + 0.3f;

            GameScene.Log("마우스 눌렀음");

            if (Physics.Raycast(vec, transform.forward, out hit, 10f, mask))
            {
                if (!hit.transform.TryGetComponent(out monsterController))
                {
                    Debug.Log($"{hit.transform.name}에 MonsterController가 존재하지 않습니다!");
                }

                Debug.Log("히트!!!!!!");
            }

            SendSkillPacket(monsterController);
            
            //if (_hasAnimator)
            //    State = CreatureState.Skill;
           
            //_input.attack = false;
        }
      

        // 이펙트

        // 사운드
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    private void CameraRotation()
    {
        // if there is an input and camera position is not fixed
        if (_input.look.sqrMagnitude >= _threshold && !_lockCameraPosition)
        {
            //Don't multiply mouse input by Time.deltaTime;
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
            _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
        }

        // clamp our rotations so our values are limited 360 degrees
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, _bottomClamp, _topClamp);

        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + _cameraAngleOverride,
            _cinemachineTargetYaw, 0.0f);
    }

    protected override void Move()
    {
        float targetSpeed = _input.sprint ? _sprintSpeed : MoveSpeed;

        if (_input.move == Vector2.zero)
        {
            targetSpeed = 0.0f; // 입력 없을 경우
        }

        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

        float speedOffset = 0.1f;
        _inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

        if (currentHorizontalSpeed < targetSpeed - speedOffset ||
            currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * _inputMagnitude,
                Time.deltaTime * _speedChangeRate);

            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }

        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * _speedChangeRate);

        if (_animationBlend < 0.01f) 
            _animationBlend = 0f;

        Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

        if (_input.move != Vector2.zero)
        {
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                              _mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                _rotationSmoothTime);

            // rotate to face input direction relative to camera position
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);

            PosInfo.RotY = rotation;
        }

        Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

        Vector3 result = targetDirection.normalized * (_speed * Time.deltaTime) +
                         new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime;

        _controller.Move(result);

        WorldPos = transform.position;

        // 점프키 누르고 찰나의 순간 패킷 보내는 갭 줄이기
        if (State == CreatureState.Jump)
            SendMovePacket();

        if (!_grounded || State == CreatureState.Jump || State == CreatureState.Inair)
            return;

        if (_input.move == Vector2.zero)
        {
            /*
            if(_isIdle == true)
            {
                Vector3 temp = transform.position;
                temp.y = temp.y - _groundedOffset;
                transform.position = temp;
            }
            */
            State = CreatureState.Idle;
            SendMovePacket();
            _isIdle = true;
        }
        else
        {
            State = CreatureState.Moving;
            _isIdle = false;
            SendMovePacket();
        }
    }

    private void JumpAndGravity()
    {
        if (_grounded)
        {
            _fallTimeoutDelta = _fallTimeout;


            // stop our velocity dropping infinitely when grounded
            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

            
                
            if (_input.jump && _jumpTimeoutDelta <= 0.0f)
            {
                // the square root of H * -2 * G = how much velocity needed to reach desired height
                _verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravity);

                if (_hasAnimator)
                {
                    State = CreatureState.Jump;
                    _isIdle = false;
                    SendMovePacket();
                }
            }


            if (_jumpTimeoutDelta > 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            _jumpTimeoutDelta = _jumpTimeout;

            if (_fallTimeoutDelta > 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                if (_hasAnimator)
                {
                    State = CreatureState.Inair;
                    _isIdle = false;
                    SendMovePacket();
                }
            }

            _input.jump = false;
        }

        // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        if (_verticalVelocity < _terminalVelocity) // 떨어질 때 최고 속도보다 작으면 
        {
            _verticalVelocity += _gravity * Time.deltaTime;
        }
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            AudioSource.PlayClipAtPoint(_landingAudioClip, transform.TransformPoint(_controller.center), _footstepAudioVolume);
        }
    }

    private void GroundedCheck()
    { 
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - _groundedOffset,
            transform.position.z);
        _grounded = Physics.CheckSphere(spherePosition, _groundedRadius, GroundLayers,
            QueryTriggerInteraction.Ignore);
        
   
        if (State == CreatureState.Jump)
            return;

        if (_hasAnimator)
        {
            if (!_grounded)
                State = CreatureState.Inair;
            else
                State = CreatureState.Idle;
        }
    }

    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            if (_footstepAudioClips.Length > 0)
            {
                var index = Random.Range(0, _footstepAudioClips.Length);
                AudioSource.PlayClipAtPoint(_footstepAudioClips[index], transform.TransformPoint(_controller.center), _footstepAudioVolume);
            }
        }
    }

    protected override void PushRigidBodies(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;
        if (body == null || body.isKinematic) return;

        var bodyLayerMask = 1 << body.gameObject.layer;
        if ((bodyLayerMask & _pushLayers.value) == 0) return;

        if (hit.moveDirection.y < -0.3f) return;

        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0.0f, hit.moveDirection.z);

        body.AddForce(pushDir * _strength, ForceMode.Impulse);
    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (_canPush) PushRigidBodies(hit);
    }

    private void SendMovePacket()
    {
        if (_isIdle && _animationBlend <= 0f)
            return;

       // if (State != CreatureState.Inair && !_grounded)
       //     return;

        C_Move movePacket = new C_Move();
        movePacket.PosInfo = PosInfo;
        movePacket.State = State;
        movePacket.AnimationBlend = _animationBlend;
        movePacket.InputMagnitude = _inputMagnitude;
        Managers.Network.Send(movePacket);
    }

    private void SendSkillPacket(CreatureController controller = null)
    {
        /*
        ObjectInfo info = new ObjectInfo();
        info.ObjectId = Id;
        info.Name = Name;
        info.Stat = Stat;
        info.PosInfo = PosInfo;

        ObjectInfo attackedInfo = new ObjectInfo();
        info.ObjectId = controller.Id;
        info.Name = controller.Name;    
        info.Stat = controller.Stat;
        info.PosInfo = controller.PosInfo;
        */
        
        C_Skill skillPacket = new C_Skill();

        SkillInfo skillInfo = new SkillInfo();
        skillInfo.Attacker = new ObjectInfo();
        skillInfo.Attacker.Name = Name;
        skillInfo.Attacker.Stat = Stat;
        skillInfo.Attacker.PosInfo = PosInfo;
        skillInfo.Attacker.ObjectId = Id;

        if (controller != null)
        {
            skillInfo.Victim = new ObjectInfo();
            skillInfo.Victim.Name = controller.Name;
            skillInfo.Victim.Stat = controller.Stat;
            skillInfo.Victim.PosInfo = controller.PosInfo;
            skillInfo.Victim.ObjectId = controller.Id;
        }

        skillPacket.Info = skillInfo;

        skillPacket.Info.SkillId = 1; // SkillData.json
        Managers.Network.Send(skillPacket);
    }
}
