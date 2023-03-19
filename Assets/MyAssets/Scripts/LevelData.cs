using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HDTWarrior
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "HDTWarrior/Create Levels Data")]
    public class LevelData : ScriptableObject
    {
        [SerializeField] private string[] levels;
        public string GetNextLevel()
        {
            Scene curScene = SceneManager.GetActiveScene();
            for (int i = 0; i < levels.Length - 1; i++)
            {
                if (levels[i] == curScene.name) {
                    return levels[i + 1];
                }
                
            }
            return string.Empty;
        }

        public string GetFirstLevel()
        {
            return levels[0];
        }
    }
}
