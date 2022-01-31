using UnityEngine;
using RPG.Movement;
using RPG.Core;
using RPG.Attributes;
using RPG.Saving;
using RPG.Stats;
using System;
using System.Collections.Generic;
using GameDevTV.Utils;

namespace RPG.Combat
{
    public class Fighter : MonoBehaviour, IAction, ISaveable, IModifierProvider
    {
        [SerializeField] float timeBetweenAttacks = 1f;
        [SerializeField] Transform rightHandTransform = null;
        [SerializeField] Transform leftHandTransform = null;
        [SerializeField] WeaponConfig defaultWeapon = null;



        float timeSinceLastAttack = Mathf.Infinity;
        Health target;
        Mover mover;
        ActionScheduler actionScheduler;
        Animator myAnimator;
        WeaponConfig currentWeaponConfig;
        LazyValue<Weapon> currentWeapon;

        private void Awake()
        {
            mover = GetComponent<Mover>();
            actionScheduler = GetComponent<ActionScheduler>();
            myAnimator = GetComponent<Animator>();
            currentWeaponConfig = defaultWeapon;
           currentWeapon=new LazyValue<Weapon>(SetUpDefaultWeapon);
        }



        private void Start()
        {
            currentWeapon.ForceInit();


        }


        private void Update()
        {
            timeSinceLastAttack += Time.deltaTime;
            if (target == null) return;

            if (target.IsDead()) return;




            if (!GetIsInRange(target.transform))
            {
                GetComponent<Mover>().MoveTo(target.transform.position, 1f);

            }
            else
            {
                mover.Cancel();
                AttackBehaviour();
            }

        }
        private Weapon SetUpDefaultWeapon()
        {
           return AttachWeapon(defaultWeapon);
            
        }
        public void EquipWeapon(WeaponConfig weapon)
        {
            currentWeaponConfig=weapon;
            currentWeapon.value = AttachWeapon(weapon);
            
        }

        private Weapon AttachWeapon(WeaponConfig weapon)
        {
            Animator animator = GetComponent<Animator>();
            return weapon.Spawn(rightHandTransform, leftHandTransform, animator);
            
        }

        public Health GetTarget()
        {
            return target;
        }

        public bool CanAttack(GameObject combatTarget)
        {
            if (combatTarget == null) { return false; }
            if (!GetComponent<Mover>().CanMoveTo(combatTarget.transform.position)&&!GetIsInRange(combatTarget.transform)) {return false;}
           
            Health targetToTest = combatTarget.GetComponent<Health>();

            return targetToTest != null && !targetToTest.IsDead();



        }

        private void AttackBehaviour()
        {

            transform.LookAt(target.transform);
            if (timeSinceLastAttack > timeBetweenAttacks)
            {
                TriggerAttack();
                timeSinceLastAttack = 0;

            }

        }

        private void TriggerAttack()
        {
            myAnimator.ResetTrigger("stopAttack");
            myAnimator.SetTrigger("attack");
        }

        private bool GetIsInRange(Transform targetTransform)
        {
            return Vector3.Distance(transform.position, targetTransform.transform.position) < currentWeaponConfig.GetRange();
        }

        public void Attack(GameObject combatTarget)
        {
            actionScheduler.StartAction(this);
            target = combatTarget.GetComponent<Health>();


        }
        public void Cancel()
        {
            StopAttack();
            target = null;
            mover.Cancel();

        }

        private void StopAttack()
        {
            myAnimator.ResetTrigger("attack");
            myAnimator.SetTrigger("stopAttack");
        }

        public IEnumerable<float> GetAdditiveModifiers(Stat stat)
        {
            if (stat == Stat.Damage)
            {
                yield return currentWeaponConfig.GetDamage();
            }
        }

        //Animation Event
        void Hit()
        {
            if (target == null) { return; }
            float damage = GetComponent<BaseStats>().GetStat(Stat.Damage);

            if(currentWeapon.value!=null)
            {
                currentWeapon.value.OnHit();
            }

            if (currentWeaponConfig.HasProjectile())
            {
                currentWeaponConfig.LaunchProjectile(rightHandTransform, leftHandTransform, target, gameObject, damage);
            }
            else
            {
                target.TakeDamage(gameObject, damage);
            }



        }



        void Shoot()
        {
            Hit();
        }

        public object CaptureState()
        {
            return currentWeaponConfig.name;
        }

        public void RestoreState(object state)
        {
            string weaponName = (string)state;
            WeaponConfig weapon = Resources.Load<WeaponConfig>(weaponName);
            EquipWeapon(weapon);


        }

        IEnumerable<float> IModifierProvider.GetPercentageModifiers(Stat stat)
        {
            if (stat == Stat.Damage)
            {
                yield return currentWeaponConfig.GetPercentageBonus();
            }
        }
    }
}

