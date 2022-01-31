using System;
using RPG.Stats;
using UnityEngine;
using UnityEngine.UI;


namespace RPG.Attributes
{
    public class LevelDisplay : MonoBehaviour
    {
        BaseStats stats;
         

        private void Awake()
        {

             stats = GameObject.FindWithTag("Player").GetComponent<BaseStats>();
        }
        private void Update()
        {

            GetComponent<Text>().text = String.Format("{0:0}", stats.GetLevel());
        }




    }
}
