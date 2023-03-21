using HDTWarrior;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    public enum State
    {
        Coin
    }

    [SerializeField] private Animator m_Animator;
    [SerializeField] private Rigidbody2D m_Rigidbody2D;

    private int m_StateParamHash;
    private State m_CurrentState;

    void Start()
    {
        m_StateParamHash = Animator.StringToHash("State");
        m_Animator.SetInteger(m_StateParamHash, 1);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Destroy(gameObject);
            GamePlayManager.Instance.EnemyDied();
            return;
        }
    }
}
