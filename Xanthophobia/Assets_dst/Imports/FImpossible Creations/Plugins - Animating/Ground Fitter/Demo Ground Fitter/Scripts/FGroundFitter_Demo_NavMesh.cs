using FIMSpace.Basics;
using UnityEngine;
using UnityEngine.AI;

namespace FIMSpace.GroundFitter
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class FGroundFitter_Demo_NavMesh : MonoBehaviour
    {
        public FGroundFitter_Base TargetGroundFitter;
        [Range(0.5f, 50f)]
        public float RotationSpeed = 3f;
        [Range(0.0f, 1f)]
        [Tooltip("Moving Accordingly to rotation after acceleration")]
        public float DirectMovement = .8f;
        public float AnimationSpeedScale = 1f;

        private NavMeshAgent agent;
        private FAnimationClips animationClips;

        public bool moving { get; private set; }
        private bool reachedDestination;
        private Vector3 lastAgentPosition;
        private string movementClip;

        private float dirMov = 0f;
        private float sd_dirMov = 0f;

        private void Reset()
        {
            TargetGroundFitter = GetComponent<FGroundFitter_Base>();

            if (TargetGroundFitter) TargetGroundFitter.GlueToGround = false;

            agent = GetComponent<NavMeshAgent>();

            if (agent)
            {
                agent.acceleration = 1000;
                agent.angularSpeed = 100;
            }
        }


        protected virtual void Start()
        {
            if (TargetGroundFitter == null) TargetGroundFitter = GetComponent<FGroundFitter_Base>();

            if (TargetGroundFitter) TargetGroundFitter.GlueToGround = false;

            agent = GetComponent<NavMeshAgent>();

            agent.Warp(transform.position);
            agent.SetDestination(transform.position);
            moving = false;
            lastAgentPosition = transform.position;
            reachedDestination = true;

            animationClips = new FAnimationClips(GetComponentInChildren<Animator>());
            animationClips.AddClip("Idle");

            if (FAnimatorMethods.StateExists(animationClips.Animator, "Move") || FAnimatorMethods.StateExists(animationClips.Animator, "move"))
                movementClip = "Move";
            else
                movementClip = "Walk";

            animationClips.AddClip(movementClip);
        }


        protected virtual void Update()
        {
            // Animation Stuff
            animationClips.SetFloat("AnimationSpeed", agent.desiredVelocity.magnitude * AnimationSpeedScale, 8f);
            IsMovingCheck();

            // Direction stuff
            Vector3 velo = (agent.nextPosition - lastAgentPosition);
            float dist = agent.velocity.magnitude;
            Vector3 dir = velo.normalized;
            Debug.DrawRay(transform.position + Vector3.up * 0.2f, dir, Color.white);

            Vector3 newPos = agent.nextPosition;

            if (DirectMovement > 0f)
            {
                if (dist > 0f)
                {
                    Vector3 dirTranslate = lastAgentPosition + transform.forward * dist * Time.deltaTime;

                    float dur = 0.25f;
                    float tgt = 1f;

                    if (agent.remainingDistance <= agent.stoppingDistance * 1.1f + 0.1f)
                    {
                        dur = 0.1f;
                        tgt = 0f;
                    }

                    dirMov = Mathf.SmoothDamp(dirMov, tgt, ref sd_dirMov, dur, 1000f, Time.deltaTime);
                    newPos = Vector3.LerpUnclamped(newPos, dirTranslate, dirMov);
                }
                else
                {
                    dirMov = Mathf.SmoothDamp(dirMov, 0f, ref sd_dirMov, 0.1f, 1000f, Time.deltaTime);
                }
            }


            transform.position = newPos;

            if (moving)
            {
                // Calculating look rotation to target point
                Vector3 targetPoint = agent.nextPosition + agent.desiredVelocity;
                float yRotation = Quaternion.LookRotation(new Vector3(targetPoint.x, 0f, targetPoint.z) - transform.position).eulerAngles.y;

                // Setting top down rotation in ground fitter component with smoothing (lerp)
                //TargetGroundFitter.RotationYAxis = yRotation;
                TargetGroundFitter.UpAxisRotation = Mathf.LerpAngle(TargetGroundFitter.UpAxisRotation, yRotation, Time.deltaTime * RotationSpeed);
            }

            lastAgentPosition = newPos;
        }


        private bool IsMovingCheck()
        {
            bool preMov = moving;

            moving = true;

            if (!agent.pathPending)
                if (agent.remainingDistance <= agent.stoppingDistance)
                    if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                    {
                        if (!reachedDestination) OnReachDestination();
                        moving = false;
                    }

            if (preMov != moving) OnStartMoving();

            return moving;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (Application.isPlaying)
                if (moving)
                {
                    Gizmos.color = new Color(1f, 1f, 0.2f, 0.34f);
                    Gizmos.DrawLine(transform.position, agent.destination);
                    Gizmos.DrawWireSphere(transform.position + agent.velocity, 0.1f);
                    UnityEditor.Handles.Label(agent.nextPosition, "Remaining: " + agent.remainingDistance);
                }
        }
#endif

        protected virtual void OnReachDestination()
        {
            reachedDestination = true;
            animationClips.CrossFadeInFixedTime("Idle", 0.25f);
        }


        protected virtual void OnStartMoving()
        {
            reachedDestination = false;
            animationClips.CrossFadeInFixedTime(movementClip, 0.25f);
        }
    }
}