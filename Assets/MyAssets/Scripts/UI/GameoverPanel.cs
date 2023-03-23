using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace HDTWarrior
{
    public class GameoverPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI m_TxtResult;

        public void DisplayResult(bool isWin)
        {
            if (isWin)
            {
                m_TxtResult.text = "YOU WIN";
            }
            else
            {
                m_TxtResult.text = "YOU LOSE";
            }
        }

        public void BtnNextLevel_Pressed()
        {
            GamePlayManager.Instance.NextLevel();
        }

        public void BtnRestart_Pressed()
        {
            GamePlayManager.Instance.Restart();
        }

        public void Quit()
        {
            Application.Quit();
        }
        public void StartLevelBegin()
        {
            SceneManager.LoadScene("Select_Scene");
        }

        public void Start_Level_1()
        {
            SceneManager.LoadScene("Level_1");
        }

        public void Start_Level_2()
        {
            SceneManager.LoadScene("Level_2");
        }

        public void Start_Level_3()
        {
            SceneManager.LoadScene("Level_3");
        }

        public void Start_Level_4()
        {
            SceneManager.LoadScene("Level_4");
        }

        public void Start_Level_5()
        {
            SceneManager.LoadScene("Level_5");
        }
    }
}