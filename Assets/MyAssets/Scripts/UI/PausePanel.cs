using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HDTWarrior
{
    public class PausePanel : MonoBehaviour
    {
        public void BtnContinue_Pressed()
        {
            GamePlayManager.Instance.Continue();
        }

        public void BtnRestart_Pressed()
        {
            GamePlayManager.Instance.Restart();
        }

        public void BtnSelect_Pressed()
        {
            GamePlayManager.Instance.Select();
        }

        public void BtnExit_Pressed()
        {
            GamePlayManager.Instance.Exit();
        }
    }
}