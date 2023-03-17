using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

namespace HDTWarrior
{
    public class GameplayPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI m_TxtHP;
        [SerializeField] private Image m_ImgHpBar;

        private PlayerController m_Player;

        private void OnEnable()
        {
            m_Player = FindObjectOfType<PlayerController>();
            m_Player.onCurHpChanged += OnHpChanged;
        }

        private void OnDisable()
        {
            m_Player.onCurHpChanged -= OnHpChanged;
        }

        private void OnHpChanged(int curHp, int maxHp)
        {
            m_TxtHP.text = $"{curHp}/{maxHp}";
            m_ImgHpBar.fillAmount = curHp * 1f / maxHp;
        }

        public void BtnPause_Pressed()
        {
            GamePlayManager.Instance.Pause();
        }
    }
}