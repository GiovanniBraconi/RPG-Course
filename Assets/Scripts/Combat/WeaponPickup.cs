using System;
using System.Collections;
using System.Collections.Generic;
using RPG.Attributes;
using RPG.Control;
using UnityEngine;

namespace RPG.Combat
{
    public class WeaponPickup : MonoBehaviour, IRaycastable
    {
        [SerializeField] WeaponConfig weapon = null;
        [SerializeField] float healthtoRestore = 0;
        [SerializeField] float respownTime = 5f;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Pickup(other.gameObject);
            }

        }

        private void Pickup(GameObject subject)
        {
            if (weapon != null)
            {
            subject.GetComponent<Fighter>().EquipWeapon(weapon);

            }
            if(healthtoRestore>0)
            {
                subject.GetComponent<Health>().Heal(healthtoRestore);
            }
            StartCoroutine(HideForSeconds(respownTime));
        }

        private IEnumerator HideForSeconds(float seconds)
        {
            ShowPickup(false);
            yield return new WaitForSeconds(seconds);
            ShowPickup(true);
        }



        private void ShowPickup(bool shouldShow)
        {
            SphereCollider sphereCollider = GetComponent<SphereCollider>();
            if (shouldShow)
            {

                sphereCollider.enabled = true;
                foreach (Transform child in transform)
                {
                    child.gameObject.SetActive(true);
                }
            }
            else
            {
                sphereCollider.enabled = false;
                foreach (Transform child in transform)
                {
                    child.gameObject.SetActive(false);
                }

            }
        }

        public bool HandleRaycast(PlayerController callingController)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Pickup(callingController.gameObject);
            }
            return true;
        }

        public CursorType GetCursorType()
        {
            return CursorType.Pickup;
        }
    }
}
