

using UnityEngine;
using UnityEngine.AI;
using RPG.Core;
using RPG.Attributes;
using RPG.Saving;
using RPG.Control;

namespace RPG.Movement
{
    public class Mover : MonoBehaviour, IAction, ISaveable
    {
        [SerializeField] Transform target;
        [SerializeField] float maxSpeed = 6f;
        [SerializeField] float maxNavPathLength = 40f;

        ActionScheduler actionScheduler;

        NavMeshAgent navMeshAgent;

        Health health;

        private void Awake()
        {
            health = GetComponent<Health>();
            actionScheduler = GetComponent<ActionScheduler>();
            navMeshAgent = GetComponent<NavMeshAgent>();
        }
        void Update()
        {
            navMeshAgent.enabled = !health.IsDead();
            UpdateAnimator();

        }
        public void StartMoveAction(Vector3 destination, float speedFraction)
        {
            actionScheduler.StartAction(this);

            MoveTo(destination, speedFraction);
        }

        public bool CanMoveTo(Vector3 destination)
        {
            
            NavMeshPath path = new NavMeshPath();
            bool hasPath = NavMesh.CalculatePath(transform.position, destination, NavMesh.AllAreas, path);
            if (!hasPath) return false;

            if (path.status != NavMeshPathStatus.PathComplete) return false;
            if (GetPathLength(path) > maxNavPathLength) return false;


            return true;
        }

        public void MoveTo(Vector3 destination, float speedFraction)
        {
            navMeshAgent.destination = destination;
            navMeshAgent.speed = maxSpeed * Mathf.Clamp01(speedFraction);
            navMeshAgent.isStopped = false;

        }
        public void Cancel()
        {
            navMeshAgent.isStopped = true;
        }


        private void UpdateAnimator()
        {
            Vector3 globalVel = navMeshAgent.velocity;
            Vector3 localVel = transform.InverseTransformDirection(globalVel);
            float speed = localVel.z;
            GetComponent<Animator>().SetFloat("forwardSpeed", speed);

        }
        private float GetPathLength(NavMeshPath path)
        {
            float count = 0;
            if (path.corners.Length < 2) return count;
            Vector3[] pathCorners = path.corners;
            for (int i = 0; i < pathCorners.Length - 1; i++)
            {
                count += Vector3.Distance(pathCorners[i + 1], pathCorners[i]);
            }

            return count;
        }
        [System.Serializable]
        struct MoverSaveData
        {
            public SerializableVector3 position;
            public SerializableVector3 rotation;

        }

        public object CaptureState()
        {
            MoverSaveData data = new MoverSaveData();
            data.position = new SerializableVector3(transform.position);
            data.rotation = new SerializableVector3(transform.eulerAngles);
            return data;
        }

        public void RestoreState(object state)
        {
            MoverSaveData data = (MoverSaveData)state;
            GetComponent<NavMeshAgent>().enabled = false;
            transform.position = data.position.ToVector();
            transform.eulerAngles = data.rotation.ToVector();

            GetComponent<NavMeshAgent>().enabled = true;

        }
    }

}
