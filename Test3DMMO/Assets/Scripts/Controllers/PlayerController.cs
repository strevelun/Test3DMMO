using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : CreatureController
{
    [Header("�÷��̾�")]
    public float _moveSpeed = 5.0f;
    public float _sprintSpeed = 10.0f;
    public float _rotationSmoothTime = 0.12f; // ĳ���� ȸ�� �ӵ�
    public float _speedChangeRate = 10.0f; // ����/����

    public AudioClip _landingAudioClip;
    public AudioClip[] _footstepAudioClips;
    [Range(0, 1)] public float _footstepAudioVolume = 0.5f;

    [Space(10)]
    public float _jumpHeight = 1.2f;
    public float _gravity = -15.0f; // ĳ���� ���� �߷�

    [Space(10)]
    public LayerMask _pushLayers;
    public bool _canPush;
    [Range(0.5f, 5f)] public float _strength = 1.1f;

    protected float _jumpTimeout = 0.7f; // ���� �ִϸ��̼� �ð�. (TODO ���� ���� �ִϸ��̼� ������ ����)
    protected float _fallTimeout = 0.15f; // fall ���� ���� ��� �ʿ� �ð�. ����� ������ �� ����. (�ּ� �� �ð��� ������ freeFall ���� ������� ��)

    [Header("�÷��̾� ���� üũ")]
    public bool _grounded = true;
    public float _groundedOffset = -0.14f; // ���������� ���鿡�� ����
    public float _groundedRadius = 0.33f; // grounded check�� ������. ĳ���� ��Ʈ�ѷ��� �������� ��ġ�ؾ� ��
    public LayerMask GroundLayers;

    [Header("�ó׸ӽ�")]
    public GameObject CinemachineCameraTarget;
    public float _topClamp = 70.0f; // ī�޶� �ִ밢
    public float _bottomClamp = -30.0f; // ī�޶� �ּҰ�



    protected override void Init()
    {
        base.Init();


    }

    void Start()
    {
        Init();
    }

    private void Update()
    {

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

        // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        Gizmos.DrawSphere(
            new Vector3(transform.position.x, transform.position.y - _groundedOffset, transform.position.z),
            _groundedRadius);
    }

    protected virtual void PushRigidBodies(ControllerColliderHit hit)
    {
        
    }
}