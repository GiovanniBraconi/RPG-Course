using System;

using UnityEngine;
using UnityEngine.UI;

namespace RPG.Attributes
{
    public class HealthDisplay : MonoBehaviour
    {
        Health health;
        Experience experience;

        private void Awake()
        {
            health = GameObject.FindWithTag("Player").GetComponent<Health>();
            experience = GameObject.FindWithTag("Player").GetComponent<Experience>();
        }
        private void Update()
        {


            GetComponent<Text>().text = String.Format("{0:0}/{1:0}", health.GetHealth(), health.GetMaXHealth());




        }
    }
}
