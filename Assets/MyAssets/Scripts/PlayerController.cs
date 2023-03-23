using HDTWarrior;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public Action<int, int> onCurHpChanged;

    [SerializeField] private Rigidbody2D m_Rigidbody;
    [SerializeField] private Animator m_Animator;
    [SerializeField] private int m_MaxHp;

    //Walk
    [SerializeField] private float m_WalkingSpeed;

    //Jump
    [SerializeField] private float m_JumpForce;
    [SerializeField] private LayerMask m_GroundLayerMask;
    [SerializeField] private Transform m_Foot;
    [SerializeField] private Vector2 m_GroundCastSize;

    //Climb
    [SerializeField] private LayerMask m_ClimbableLayerMask;
    [SerializeField] private float m_ClimbSpeed;

    //Dash
    [SerializeField] private TrailRenderer tr;

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
    private int m_CurHp;
    private bool m_GetHit;
    private float m_GetHitTime;
    private bool m_Dead;
    private bool canDash = true;
    private bool isDashing;
    private float dashPower = 15f;
    private float dashTime = 0.2f;
    private float dashCooldown = 2f;
    private List<string> level = new List<string> {"Level_1", "Level_2", "Start_Scene", "End_Scene"};

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

            m_PlayerInput.Player.Skill.started += OnDash;
            m_PlayerInput.Player.Skill.performed += OnDash;
            m_PlayerInput.Player.Skill.canceled += OnDash;
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
        m_CurHp = m_MaxHp;

        if(onCurHpChanged != null)
        {
            onCurHpChanged(m_CurHp, m_MaxHp);
        }
    }

    private void Update()
    {
        var curScene = SceneManager.GetActiveScene().name;
        if (!level.Contains(curScene))
        {
            if (isDashing) return; 
        }

        if (m_GetHit)
        {
            m_GetHitTime -= Time.deltaTime;
            if (m_GetHitTime <= 0)
            {
                m_GetHit = false;
            }
        }
    }

    private void FixedUpdate()
    {
        var curScene = SceneManager.GetActiveScene().name;
        if (!level.Contains(curScene))
        {
            if (isDashing) return;
        }

        if (m_GetHit || m_Dead)
        {
            return;
        }

        m_OnGround = Physics2D.BoxCast(m_Foot.position, m_GroundCastSize, 0, Vector3.forward, 0, m_GroundLayerMask);
        if (m_OnGround) m_JumpCount = 0;

        CheckMovement();
        CheckClimb();
        CheckAnimation();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (m_GetHit || m_Dead)
        {
            return;
        }

        if (collision.CompareTag("Enemy"))
        {
            m_CurHp -= 20;
            m_GetHit = true;
            m_GetHitTime = 0.5f;

            if (onCurHpChanged != null)
            {
                onCurHpChanged(m_CurHp, m_MaxHp);
            }

            if (m_CurHp <= 0)
            {
                m_Dead = true;
                AudioManager.Instance.PlaySFX_PlayerDead();
                GamePlayManager.Instance.Gameover(false);
                PlayDyingAnim();
                return;
            }

            AudioManager.Instance.PlaySFX_PlayerGetHit();

            Vector2 knockbackDirection = transform.position - collision.transform.position;
            knockbackDirection = knockbackDirection.normalized;
            m_Rigidbody.AddForce(knockbackDirection * 10, ForceMode2D.Impulse);

            StartCoroutine(GetHitFX());
        }

        if (collision.gameObject.CompareTag("DeadLine"))
        {
            m_CurHp -= 100;
            m_Dead = true;
            AudioManager.Instance.PlaySFX_PlayerDead();
            GamePlayManager.Instance.Gameover(false);
            return;
        }

        if (collision.gameObject.CompareTag("Coin"))
        {
            if (m_CurHp == 100)
            {

            }
            else
            {
                m_CurHp += 20;
                if (onCurHpChanged != null)
                {
                    onCurHpChanged(m_CurHp, m_MaxHp);
                }
            }
        }
    }

    private IEnumerator GetHitFX()
    {
        SpriteRenderer spt;
        TryGetComponent(out spt);
        Color transparent = Color.white;
        transparent.a = 0.25f;
        int i = 0;
        while(m_GetHitTime > 0)
        {
            if(i % 2 == 0)
            {
                spt.color = Color.white;
            }
            else
            {
                spt.color = transparent;
            }
            i++;
            yield return null;
        }
        spt.color = Color.white;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("MoveablePlatform"))
        {
            transform.SetParent(collision.transform);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("MoveablePlatform"))
        {
            transform.SetParent(collision.transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("MoveablePlatform"))
        {
            transform.SetParent(null);
        }
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
        if (m_AttackInput || m_GetHit || m_Dead)
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

    private void OnAttack(InputAction.CallbackContext ctx)
    {
        if (ctx.started || ctx.performed)
            m_AttackInput = true;
        else
            m_AttackInput = false;
    }

    private void OnDash(InputAction.CallbackContext context)
    {
        var curScene = SceneManager.GetActiveScene().name;
        if (context.started || context.performed)
        {
            if (canDash && !level.Contains(curScene)) StartCoroutine(Dash());
        }
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        float originalGravity = m_Rigidbody.gravityScale;
        m_Rigidbody.gravityScale = 0;
        m_Rigidbody.velocity = new Vector2(transform.localScale.x * dashPower, 0f);
        tr.emitting = true;
        yield return new WaitForSeconds(dashTime);
        tr.emitting = false;
        m_Rigidbody.gravityScale = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void PlayAttackSFX()
    {
        AudioManager.Instance.PlaySFX_MeleeSplash();
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
