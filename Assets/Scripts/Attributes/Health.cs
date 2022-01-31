using UnityEngine;
using RPG.Core;
using RPG.Saving;
using RPG.Stats;
using System;
using GameDevTV.Utils;
using UnityEngine.Events;

namespace RPG.Attributes
{
    public class Health : MonoBehaviour, ISaveable
    {
        Animator myAnimator;
        bool isDead = false;

        BaseStats baseStats;
        [SerializeField] float regenerationPercentage = 70;
        [SerializeField] UnityEvent<float> takeDamage;
        [SerializeField] UnityEvent onDie;




        LazyValue<float> healthPoints;


        private void Awake()
        {
            healthPoints = new LazyValue<float>(GetInitialHealth);
            myAnimator = GetComponent<Animator>();
            // if(healthPoints.value<0)
            // {
            // healthPoints.value = GetComponent<BaseStats>().GetStat(Stat.Health);
            // }
            baseStats = GetComponent<BaseStats>();


        }

      

        private void Start()
        {
            healthPoints.ForceInit();

        }

        private float GetInitialHealth()
        {
            return GetComponent<BaseStats>().GetStat(Stat.Health);
        }

        private void OnEnable()
        {
            baseStats.onLevelUp += HealthOnLevelUp;
        }
        private void OnDisable()
        {
            baseStats.onLevelUp -= HealthOnLevelUp;
        }

        private void HealthOnLevelUp()
        {
            float regenHealthPoints = GetComponent<BaseStats>().GetStat(Stat.Health) * (regenerationPercentage / 100);
            healthPoints.value = Mathf.Max(healthPoints.value, regenHealthPoints);
        }

        public bool IsDead()
        {
            return isDead;
        }


        public void TakeDamage(GameObject instigator, float damage)
        {

            print(gameObject.name + "took damage: " + damage);
            healthPoints.value = Mathf.Max(healthPoints.value - damage, 0);

            if (healthPoints.value == 0)
            {
                onDie.Invoke();
                Die();
                AwardExperience(instigator);
            }
            else
            {
                takeDamage.Invoke(damage);
            }

        }


        public void Heal(float healthtoRestore)
        {
            
            healthPoints.value = Mathf.Min(healthPoints.value + healthtoRestore,GetMaXHealth());
        }


        public float GetHealth()
        {
            return healthPoints.value;
        }

        public float GetMaXHealth()
        {
            return GetComponent<BaseStats>().GetStat(Stat.Health);
        }



        public float GetPercentage()
        {
            return 100 * GetFraction();

        }
        public float GetFraction()
        {
            return (healthPoints.value / GetComponent<BaseStats>().GetStat(Stat.Health));
        }

        private void Die()
        {
            if (!isDead)
            {
                myAnimator.SetTrigger("die");
                GetComponent<ActionScheduler>().CancelCurrentAction();
            }
            isDead = true;
        }

        private void AwardExperience(GameObject instigator)
        {
            Experience experience = instigator.GetComponent<Experience>();
            if (experience == null) return;

            experience.GainExperience(GetComponent<BaseStats>().GetStat(Stat.ExperienceReward));
        }
        public object CaptureState()
        {
            return healthPoints.value;
        }

        public void RestoreState(object state)
        {

            healthPoints.value = (float)state;
            if (healthPoints.value == 0)
            {
                Die();
            }
        }


    }
}