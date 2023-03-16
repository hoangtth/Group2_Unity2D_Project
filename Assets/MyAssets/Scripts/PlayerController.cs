using System;
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

    //Climb
    [SerializeField] private LayerMask m_ClimbableLayerMask;
    [SerializeField] private float m_ClimbSpeed;

    private int m_AttackHash;
    private int m_DyingHash;
    private int m_IdleHash;
    private int m_WalkingHash;
    private PlayerInputActions m_PlayerInput;
    private Vector2 m_Movementinput;
    private bool m_OnGround;
    private int m_JumpCount;
    private bool m_AttackInput;
    private Collider2D m_collider2D;

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

            m_PlayerInput.Player.Attack.started += OnAttack;
            m_PlayerInput.Player.Attack.performed += OnAttack;
            m_PlayerInput.Player.Attack.canceled += OnAttack;
        }
        m_PlayerInput.Enable();
    }

    private void OnDisable()
    {
        if (m_PlayerInput != null) m_PlayerInput.Disable();
    }

    private void Start()
    {
        TryGetComponent(out m_collider2D);
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
        CheckClimb();
        CheckAnimation();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(m_Foot.position, m_GroundCastSize);
    }

    private void CheckMovement()
    {
        //If attacking, don't move
        if (m_AttackInput)
            return;

        m_Rigidbody.velocity = new Vector2(m_Movementinput.x * m_WalkingSpeed, m_Rigidbody.velocity.y);
    }

    private void CheckClimb()
    {
        if (m_collider2D.IsTouchingLayers(m_ClimbableLayerMask))
        {
            Vector2 velocity = m_Rigidbody.velocity;
            velocity.y = m_ClimbSpeed * m_Movementinput.y;
            m_Rigidbody.velocity = velocity;
            m_Rigidbody.gravityScale = 0;
        }
        else
        {
            m_Rigidbody.gravityScale = 2f;
        }
    }

    private void CheckAnimation()
    {
        m_Animator.SetBool(m_AttackHash, m_AttackInput);
        m_Animator.SetBool(m_IdleHash, m_Rigidbody.velocity.x == 0 && !m_AttackInput);
        m_Animator.SetBool(m_WalkingHash, m_Rigidbody.velocity.x != 0 && !m_AttackInput);
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        //If attacking, don't jump
        if (m_AttackInput)
            return;

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
        if (context.started || context.performed)
        {
            m_Movementinput = context.ReadValue<Vector2>();
            transform.localScale = new Vector3(m_Movementinput.x >= 0 ? 1 : -1, 1, 1);
        }
        else
        {
            m_Movementinput = Vector2.zero;
        }
    }

    //Handle on attack event
    private void OnAttack(InputAction.CallbackContext ctx)
    {
        if (ctx.started || ctx.performed)
            m_AttackInput = true;
        else
            m_AttackInput = false;
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
