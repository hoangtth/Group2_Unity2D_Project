using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D m_Rigidbody;
    [SerializeField] private Animator m_Animator;
    [SerializeField] private float m_WalkingSpeed;

    //Jump
    [SerializeField] private float m_JumpForce;
    [SerializeField] private LayerMask m_GroundLayerMask;
    [SerializeField] private Transform m_Foot;
    [SerializeField] private Vector2 m_GroundCastSize;

    private int m_AttackHash;
    private int m_DyingHash;
    private int m_IdleHash;
    private int m_WalkingHash;
    private PlayerInputActions m_PlayerInput;
    private Vector2 m_Movementinput;
    private bool m_OnGround;
    private int m_JumpCount;

    private void OnEnable()
    {
        if (m_PlayerInput == null)
        {
            m_PlayerInput = new PlayerInputActions();
            m_PlayerInput.Player.Movement.started += OnMovement;
            m_PlayerInput.Player.Movement.performed += OnMovement;
            m_PlayerInput.Player.Movement.canceled += OnMovement;

            m_PlayerInput.Player.Jump.started += OnJump;
            m_PlayerInput.Player.Jump.performed += OnJump;
            m_PlayerInput.Player.Jump.canceled += OnJump;
        }
        m_PlayerInput.Enable();
    }

    private void OnDisable()
    {
        if (m_PlayerInput != null) m_PlayerInput.Disable();
    }

    private void Start()
    {
        m_AttackHash = Animator.StringToHash("Attack");
        m_DyingHash = Animator.StringToHash("Dying");
        m_IdleHash = Animator.StringToHash("Idle");
        m_WalkingHash = Animator.StringToHash("Walking");
    }

    private void Update()
    {
        
    }

    private void FixedUpdate()
    {
        m_OnGround = Physics2D.BoxCast(m_Foot.position, m_GroundCastSize, 0, Vector3.forward, 0, m_GroundLayerMask);
        if (m_OnGround) m_JumpCount = 0;

        CheckMovement();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(m_Foot.position, m_GroundCastSize);
    }

    private void CheckMovement()
    {
        m_Rigidbody.velocity = new Vector2(m_Movementinput.x * m_WalkingSpeed, m_Rigidbody.velocity.y);
        /* PROD 0001: Nhân v không quay đúng hướng sau khi kết thúc di chuyển
         * Rootcause: Chưa đặt lại hướng cho nhân vật
         * Assignee : HoangTTH
         * TODO     : Đặt lại hướng cho nhân vật sau khi kết thúc di chuyển
         */
        if (m_Rigidbody.velocity.x >= 0) transform.localScale = Vector3.one;
        else transform.localScale = new Vector3(-1,1,1);

        if (m_Rigidbody.velocity.x != 0) PlayWalkingAnim();
        else PlayIdleAnim();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (context.started || context.performed) 
        {
            if (m_OnGround || m_JumpCount < 2)
            {
                m_Rigidbody.velocity = new Vector2(m_Rigidbody.velocity.x, m_JumpForce);
                m_JumpCount++;
            }
        }

    }

    private void OnMovement(InputAction.CallbackContext context)
    {
        if (context.started || context.performed) m_Movementinput = context.ReadValue<Vector2>();
        else m_Movementinput = Vector2.zero;
    }

    [ContextMenu("Play Attack Anim")]
    private void PlayAttackAnim()
    {
        m_Animator.SetBool(m_AttackHash, true);
        m_Animator.SetBool(m_IdleHash, false);
        m_Animator.SetBool(m_WalkingHash, false);
    }

    [ContextMenu("Play Idle Anim")]
    private void PlayIdleAnim()
    {
        m_Animator.SetBool(m_AttackHash, false);
        m_Animator.SetBool(m_IdleHash, true);
        m_Animator.SetBool(m_WalkingHash, false);
    }

    [ContextMenu("Play Walking Anim")]
    private void PlayWalkingAnim()
    {
        m_Animator.SetBool(m_AttackHash, false);
        m_Animator.SetBool(m_IdleHash, false);
        m_Animator.SetBool(m_WalkingHash, true);
    }

    [ContextMenu("Play Dying Anim")]
    private void PlayDyingAnim()
    {
        m_Animator.SetBool(m_DyingHash, true);
    }

    [ContextMenu("Reset Anim")]
    private void ResetAnim()
    {
        m_Animator.SetBool(m_AttackHash, false);
        m_Animator.SetBool(m_IdleHash, true);
        m_Animator.SetBool(m_WalkingHash, false);
        m_Animator.SetBool(m_DyingHash, false);
    }
}
