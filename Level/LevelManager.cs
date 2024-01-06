using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Level
{
    [System.Serializable]
    public struct LevelRules /// add random spwantimer, add speed of cars
    {
        public GameObject[] AvailableCars;
        public CarSpawn[] CarSpawns;
        public float SpawnFreqency;

        [HideInInspector]
        public int levelIndex;

        public CarSpawn GetRandomSpawn() => CarSpawns[Random.Range(0, CarSpawns.Length)];

        public GameObject GetRandomCar() => AvailableCars[Random.Range(0, AvailableCars.Length)];

        public float GetNextSpawnTime()
        {
            return Random.Range(SpawnFreqency - 1f, SpawnFreqency + 1f); // this sets the entire random for all of the spawns. 
        }
    }
    
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private LevelRules rules;

        public int GetLevelIndex()
        {
            rules.levelIndex = transform.GetSiblingIndex();
            return rules.levelIndex;
        }

        public LevelRules GetRules() => rules;

        [ExecuteInEditMode]
        public void FindAllCarSpawns()
        {
            int childCount = transform.childCount;

            List<CarSpawn> carSpawns = new List<CarSpawn>();

            for (int i = 0; i < childCount; i++)
            {
                GameObject child = transform.GetChild(i).gameObject;
                if (child.GetComponent<CarSpawn>())
                    carSpawns.Add(child.GetComponent<CarSpawn>());
            }

            rules.CarSpawns = carSpawns.ToArray();
        }

        /*private void OnEnable() //for testing
        {
            rules.levelIndex = transform.GetSiblingIndex();
            GameManager.Instance.SetRules(rules);
        }*/
    }
}
