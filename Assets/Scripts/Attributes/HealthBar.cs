using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Attributes
{

    public class HealthBar : MonoBehaviour
    {

        [SerializeField] RectTransform greenBar = null;
        [SerializeField] Health health = null;
        [SerializeField] Canvas canvas = null;


        void Update()
        {
            if (Mathf.Approximately(health.GetFraction(), 0))
            {
                canvas.enabled = false;
                return;
            }if (Mathf.Approximately(health.GetFraction(), 1))
            {
                canvas.enabled = false;
                return;
            }
            canvas.enabled = true;
            greenBar.localScale = new Vector3(health.GetFraction(), 1, 1);


        }
    }
}