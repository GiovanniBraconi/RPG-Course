
using UnityEngine;
using RPG.Combat;
using RPG.Movement;
using RPG.Core;
using RPG.Attributes;
using UnityEngine.AI;
using GameDevTV.Utils;
using System;

namespace RPG.Control
{
    public class AIController : MonoBehaviour
    {
        [SerializeField] float chaseDistance = 5f;
        [SerializeField] float suspicionTime = 3f;
        [SerializeField] float waypointDwellTime = 3f;
        [SerializeField] PatrolPath patrolPath;
        [SerializeField] float waypointTolerance = 1f;
        [SerializeField] float aggroTime = 3f;
        [SerializeField] float shoutDistance = 5f;

        [Range(0, 1)]
        [SerializeField] float patrolSpeedFraction = 0.3f;

        Fighter fighter;
        Health health;
        GameObject player;
        Mover mover;
        ActionScheduler actionScheduler;
        NavMeshAgent navMeshAgent;


        LazyValue<Vector3> guardPosition;
        float timeSinceLastSawPlayer = Mathf.Infinity;
        float timeSinceLastPlayerAttack = Mathf.Infinity;
        float timeSinceArrivedAtWaypoint = Mathf.Infinity;

        int currentWaypointIndex = 0;


        private void Awake()
        {
            player = GameObject.FindWithTag("Player");
            health = GetComponent<Health>();
            fighter = GetComponent<Fighter>();
            mover = GetComponent<Mover>();
            navMeshAgent = GetComponent<NavMeshAgent>();
            actionScheduler = GetComponent<ActionScheduler>();
            guardPosition = new LazyValue<Vector3>(GetGuardPosition);

        }


        private void Start()
        {
            guardPosition.ForceInit();
        }

        private Vector3 GetGuardPosition()
        {
            return transform.position;
        }
        private void Update()
        {
            if (health.IsDead()) { return; }

            if (IsAggrevated() && fighter.CanAttack(player))
            {

                timeSinceLastSawPlayer = 0;
                AttackBehaviour();
            }
            else if (timeSinceLastSawPlayer < suspicionTime)
            {
                SuspicionBehaviour();
            }
            else
            {


                PatrolBehaviour();




            }
            UpdateTimers();

        }

        public void Aggrevate()
        {
            timeSinceLastPlayerAttack = 0;
        }

        private void UpdateTimers()
        {
            timeSinceLastSawPlayer += Time.deltaTime;
            timeSinceArrivedAtWaypoint += Time.deltaTime;
            timeSinceLastPlayerAttack += Time.deltaTime;
        }

        private void PatrolBehaviour()
        {

            Vector3 nextPosition = guardPosition.value;
            if (patrolPath != null)
            {

                if (AtWaypoint())
                {
                    timeSinceArrivedAtWaypoint = 0;
                    CycleWaypoint();

                }
                nextPosition = GetCurrentWaypoint();
            }
            if (timeSinceArrivedAtWaypoint > waypointDwellTime)
            {
                mover.StartMoveAction(nextPosition, patrolSpeedFraction);
            }






        }

        private Vector3 GetCurrentWaypoint()
        {
            return patrolPath.GetWaypoint(currentWaypointIndex);
        }

        private void CycleWaypoint()
        {
            currentWaypointIndex = patrolPath.GetNextIndex(currentWaypointIndex);
        }

        private bool AtWaypoint()
        {
            float distanceToWaypoint = Vector3.Distance(transform.position, GetCurrentWaypoint());
            return distanceToWaypoint < waypointTolerance;
        }

        private void SuspicionBehaviour()
        {
            actionScheduler.CancelCurrentAction();
        }

        private void AttackBehaviour()
        {

            fighter.Attack(player);

            AggrevateNearbyEnemies();
        }

        private void AggrevateNearbyEnemies()
        {
            RaycastHit[] hits = Physics.SphereCastAll(transform.position, shoutDistance, Vector3.up, 0);
            foreach (RaycastHit hit in hits)
            {
                AIController aiController = hit.collider.GetComponent<AIController>();
                if (aiController == null) continue;

                aiController.Aggrevate();


            }


        }

        private bool IsAggrevated()
        {

            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);


            return distanceToPlayer < chaseDistance || timeSinceLastPlayerAttack < aggroTime;

        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, chaseDistance);
        }
    }
}
