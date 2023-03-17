using HDTWarrior;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private Sprite m_OpenDoorSprite;
    [SerializeField] private Sprite m_CloseDoorSprite;

    private Collider2D m_Collider;
    private SpriteRenderer m_SpriteRenderer;

    private void Start()
    {
        TryGetComponent(out m_Collider);
        TryGetComponent(out m_SpriteRenderer);

        GamePlayManager.Instance.onEnemyDied += OnEnemyDied;
    }

    private void OnEnemyDied()
    {
        StartCoroutine(EnemyDied());
    }

    private IEnumerator EnemyDied()
    {
        yield return null;
        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (allEnemies.Length > 0) CloseDoor();
        else OpenDoor();
    }

    private void OpenDoor()
    {
        m_Collider.enabled = true;
        m_SpriteRenderer.sprite = m_OpenDoorSprite;
    }

    private void CloseDoor()
    {
        m_Collider.enabled = false;
        m_SpriteRenderer.sprite = m_CloseDoorSprite;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GamePlayManager.Instance.Gameover(true);
        }
    }
}
