using FIMSpace.Basics;
using UnityEngine;

namespace FIMSpace.GroundFitter
{
    /// <summary>
    /// FM: Example component to use groud fitter for movement controller
    /// </summary>
    [RequireComponent(typeof(FGroundFitter))]
    public class FGroundFitter_Movement : MonoBehaviour
    {
        [Header("> Main Tweak Variables <")]
        public float BaseSpeed = 3f;
        public float RotateToTargetSpeed = 6f;
        public float SprintingSpeed = 10f;

        protected float ActiveSpeed = 0f;
        public float AccelerationSpeed = 10f;
        public float DecelerationSpeed = 10f;

        [Header("> Additional Options <")]
        // Physics variables
        public float JumpPower = 7f; // variable used in TriggerJump() in script FGroundFitter_InputBase
        public float gravity = 15f;
        public bool MultiplySprintAnimation = false;
        [Range(0f, 20f)]
        public float RotateBackInAir = 0f;
        [Tooltip("Protecting from going through walls when slope is very big and ground fitter is jumping into it")]
        public bool NotFallingThrough = false;

        [Tooltip("You need collider and rigidbody on object to make it work right - ALSO CHANGE YOUR CAMERA UPDATE CLOCK TO FIXED UPDATE AND USE TIME.fixedDeltaTime - ! For now it can cause errors when jumping, character can go through floor sometimes ! - Will be upgraded in future versions")]
        [Header("(experimental)")]
        public bool UsePhysics = false;

        [Tooltip("Disabling translating object from code and running animation without need to hold minimum movement speed")]
        public bool UseRootMotionTranslation = false;
        public bool UseRootMotionRotation = false;

        internal float YVelocity;
        protected bool inAir = false;
        protected float gravitUpOffset = 0f;

        // Motion Variables changed from FGroundFitter_Input script
        internal Vector3 lastNotZeroMoveVector = Vector3.zero;
        internal Vector3 MoveVector = Vector3.zero;
        internal bool Sprint = false;
        internal float RotationOffset = 0f;

        // Other variables
        protected string lastAnim = "";

        // Important references
        protected Animator animator;
        protected FGroundFitter fitter; // Using fitter for some transforming stuff
        protected Rigidbody rigb; // Using fitter for some transforming stuff

        // Using aniation speed property in animator to change speed of animations from code
        protected bool animatorHaveAnimationSpeedProp = false;
        protected float initialUpOffset;

        // Additional motion calculations
        protected Vector3 holdJumpPosition;
        protected float freezeJumpYPosition;
        protected float delta;
        protected Vector3 lastVelocity;
        protected Collider itsCollider;

        protected FAnimationClips clips;
        internal static int _hash_animSp = Animator.StringToHash("AnimationSpeed");
        private int _hash_IsGrounded = -1;
        private int _hash_IsMov = -1;

        private void Reset()
        {
            if (!gameObject.GetComponent<FGroundFitter_Input>()) gameObject.AddComponent<FGroundFitter_Input>();
        }

        /// <summary>
        /// Preparin initial stuff
        /// </summary>
        protected virtual void Start()
        {
            fitter = GetComponent<FGroundFitter>();
            animator = GetComponentInChildren<Animator>();
            rigb = GetComponent<Rigidbody>();
            itsCollider = GetComponentInChildren<Collider>();

            if (string.IsNullOrEmpty(SetIsGroundedParam) == false) _hash_IsGrounded = Animator.StringToHash(SetIsGroundedParam);
            if (string.IsNullOrEmpty(SetIsMovingParam) == false) _hash_IsMov = Animator.StringToHash(SetIsMovingParam);

            if (animator)
            {
                if (HasParameter(animator, "AnimationSpeed")) animatorHaveAnimationSpeedProp = true;
                animator.applyRootMotion = false;
            }

            fitter.UpAxisRotation = transform.rotation.eulerAngles.y;
            initialUpOffset = fitter.UpOffset;

            fitter.RefreshLastRaycast();

            clips = new FAnimationClips(animator);
            clips.AddClip("Idle");
            clips.AddClip("Walk");
            clips.AddClip("Run");
        }


        protected virtual void Update()
        {
            HandleBaseVariables();
            HandleGravity();
            HandleAnimations();
            HandleTransforming();

            if (UsePhysics) return;
            ApplyTransforming();
        }

        bool slidingAssigned = false;
        protected virtual void FixedUpdate()
        {
            if (rigb)
            {
                if (UsePhysics)
                {
                    rigb.useGravity = false;
                    rigb.isKinematic = false;

                    if ( !slidingAssigned)
                    {
                        Collider col = GetComponentInChildren<Collider>();
                        if ( col)
                        {
                            if (pm_Sliding == null)
                            {
                                pm_Sliding = new PhysicMaterial("Sliding");
                                pm_Sliding.bounciness = 0f;
                                pm_Sliding.frictionCombine = PhysicMaterialCombine.Minimum;
                                pm_Sliding.dynamicFriction = 0f;
                                pm_Sliding.staticFriction = 0f;
                            }

                            col.material = pm_Sliding;
                            slidingAssigned = true;
                        }
                    }
                }
                else
                {
                    rigb.isKinematic = true;
                }
            }

            if (!UsePhysics)
            {
                fitter.ApplyRotation = true;
                return;
            }

            //delta = Time.deltaTime;

            //HandleGravity();
            //HandleAnimations();
            //HandleTransforming();

            ApplyTransforming();

            //rigb.velocity = Vector3.zero;
            rigb.angularVelocity = Vector3.zero;
            rigb.freezeRotation = true;

            fitter.ApplyRotation = false;
            rigb.rotation = fitter.targetRotationToApply;
        }

        /// <summary>
        /// Calculating variables needed in further calculations
        /// </summary>
        protected virtual void HandleBaseVariables()
        {
            delta = Time.deltaTime;

            if (UseRootMotionTranslation)
            {
                fitter.HandleRootMotion = false;

                if (animator.gameObject != gameObject) if (!animator.applyRootMotion)
                        if (!animator.GetComponent<FGroundFitter_RootMotionHelper>()) animator.gameObject.AddComponent<FGroundFitter_RootMotionHelper>().MovementController = this;

                fitter.UpdateClock = EFUpdateClock.LateUpdate;
                animator.applyRootMotion = true;
            }
            else
            {
                animator.applyRootMotion = false;
            }
        }

        /// <summary>
        /// Calculating gravity stuff
        /// </summary>
        protected virtual void HandleGravity()
        {
            if (fitter.enabled)
            {
                if (fitter.UpOffset > initialUpOffset)
                    fitter.UpOffset += YVelocity * delta;
                else
                    fitter.UpOffset = initialUpOffset;
            }
            else
                fitter.UpOffset += YVelocity * delta;

            if (inAir)
            {
                YVelocity -= gravity * delta;
                fitter.RefreshDelta();
                fitter.RotateBack(RotateBackInAir);
            }

            if (fitter.enabled)
            {
                if (!fitter.LastRaycast.transform)
                {
                    if (!inAir)
                    {
                        inAir = true;
                        holdJumpPosition = transform.position;
                        freezeJumpYPosition = holdJumpPosition.y;
                        YVelocity = -1f;
                        fitter.enabled = false;
                    }
                }
                else
                    if (YVelocity > 0f)
                {
                    inAir = true;
                }
            }

            if (inAir)
            {
                if (fitter.enabled) fitter.enabled = false;

                if (YVelocity < 0f)
                {
                    RaycastHit hit = fitter.CastRay();

                    if (hit.transform)
                    {
                        //if (transform.position.y + (YVelocity * delta) <= hit.point.y + initialUpOffset + 0.05f)
                        if (transform.position.y + (YVelocity * delta) <= hit.point.y + initialUpOffset + 0.05f)
                        {
                            fitter.UpOffset -= (hit.point.y - freezeJumpYPosition);
                            HitGround();
                        }
                    }
                }
                else
                {
                    RaycastHit hit = fitter.CastRay();

                    if (hit.transform)
                    {
                        if (hit.point.y - 0.1f > transform.position.y)
                        {
                            fitter.UpOffset = initialUpOffset;
                            YVelocity = -1f;
                            HitGround();
                        }
                    }
                }

                if (NotFallingThrough)
                    if (inAir)
                    {
                        Vector3 dir = fitter.transform.forward;
                        float scale = fitter.RaycastCheckRange;
                        //if( itsCollider) scale = itsCollider.bounds.extents.magnitude;

                        if (Physics.Raycast(fitter.GetRaycastOrigin() - dir * scale * 0.1f, dir, scale * 1.11f, fitter.GroundLayerMask, QueryTriggerInteraction.Ignore))
                        {
                            float pre = fitter.RaycastCheckRange;
                            fitter.RaycastCheckRange *= 100;

                            fitter.UpOffset = initialUpOffset;
                            YVelocity = -1f;
                            HitGround();

                            fitter.RaycastCheckRange = pre;
                        }
                    }
            }
        }

        /// <summary>
        /// Handling switching animation clips of Animator
        /// </summary>
        protected virtual void HandleAnimations()
        {
            if (ActiveSpeed > 0.15f)
            {
                if (Sprint)
                    CrossfadeTo("Run", 0.25f);
                else
                    CrossfadeTo("Walk", 0.25f);
            }
            else
            {
                CrossfadeTo("Idle", 0.25f);
            }

            // If object is in air we just slowing animation speed to zero
            if (animatorHaveAnimationSpeedProp)
                if (inAir) FAnimatorMethods.LerpFloatValue(animator, "AnimationSpeed", 0f);
                else
                    FAnimatorMethods.LerpFloatValue(animator, "AnimationSpeed", MultiplySprintAnimation ? (ActiveSpeed / BaseSpeed) : Mathf.Min(1f, (ActiveSpeed / BaseSpeed)));

            if (animator)
            {
                if (_hash_IsGrounded != -1) animator.SetBool(_hash_IsGrounded, !inAir);
                if (_hash_IsMov != -1) animator.SetBool(_hash_IsMov, MoveVector != Vector3.zero);
            }

        }


        /// <summary>
        /// Refreshing some switching to new landing position varibles, useful in custom coding
        /// </summary>
        protected void RefreshHitGroundVars(RaycastHit hit)
        {
            holdJumpPosition = hit.point;
            freezeJumpYPosition = hit.point.y;
            fitter.UpOffset = Mathf.Abs(hit.point.y - transform.position.y);
        }


        /// <summary>
        /// Calculating changes for transform
        /// </summary>
        protected virtual void HandleTransforming()
        {
            if (!UseRootMotionTranslation)
            {
                lastVelocity = transform.TransformDirection(lastNotZeroMoveVector) * ActiveSpeed;
            }

            if (fitter.enabled)
            {
                if (fitter.LastRaycast.transform)
                {
                    Vector3 nPos = fitter.LastRaycast.point + fitter.UpOffset * Vector3.up;

                    if (!UsePhysics)
                        transform.position = nPos;
                    else
                    {
                        if (rigb)
                        {
                            yAdjustPos = nPos.y;
                        }
                    }

                    holdJumpPosition = transform.position;
                    freezeJumpYPosition = holdJumpPosition.y;
                }
                else
                {
                    inAir = true;

                }
            }
            else
            {
                holdJumpPosition.y = freezeJumpYPosition + fitter.UpOffset;
            }

            //if (inAir)
            //{
            //    if (UsePhysics)
            //        if (rigb)
            //        {
            //            RaycastHit hit;
            //            //itsCollider.enabled = false;

            //            if (Physics.BoxCast(itsCollider.bounds.center, itsCollider.bounds.extents * 0.6f, fitter.transform.forward, out hit, Quaternion.identity, itsCollider.bounds.max.magnitude * 0.01f, fitter.GroundLayerMask, QueryTriggerInteraction.Ignore))
            //            {
            //                //inAir = false;
            //                //fitter.BackRaycast();
            //                //fitter.enabled = true;

            //                float pre = fitter.RaycastCheckRange;
            //                fitter.RaycastCheckRange *= 100;
            //                fitter.CastRay();
            //                fitter.RaycastCheckRange = pre;
            //            }
            //            //itsCollider.enabled = true;
            //        }
            //}

            if (MoveVector != Vector3.zero)
            {
                if (!UseRootMotionRotation)
                {
                    if (!fitter.enabled)
                    {
                        fitter.UpAxisRotation = Mathf.LerpAngle(fitter.UpAxisRotation, Camera.main.transform.eulerAngles.y + RotationOffset, delta * RotateToTargetSpeed * 0.15f);
                        fitter.RotationCalculations();
                    }
                    else
                        fitter.UpAxisRotation = Mathf.LerpAngle(fitter.UpAxisRotation, Camera.main.transform.eulerAngles.y + RotationOffset, delta * RotateToTargetSpeed);
                }

                if (!Sprint)
                    ActiveSpeed = Mathf.Lerp(ActiveSpeed, BaseSpeed, delta * AccelerationSpeed);
                else
                    ActiveSpeed = Mathf.Lerp(ActiveSpeed, SprintingSpeed, delta * AccelerationSpeed);
            }
            else
            {
                if (ActiveSpeed > 0f)
                    ActiveSpeed = Mathf.Lerp(ActiveSpeed, -0.01f, delta * DecelerationSpeed);
                else ActiveSpeed = 0f;
            }

            holdJumpPosition += lastVelocity * delta;
            //if (!UsePositionRootMotion)
            //{
            //    holdJumpPosition += ((transform.forward * ActiveSpeed)) * delta;
            //}
            //else
            //{
            //    if (inAir)
            //        holdJumpPosition += ((transform.forward * ActiveSpeed)) * delta;
            //}

            if (MoveVector != Vector3.zero) lastNotZeroMoveVector = MoveVector;
        }


        float? yAdjustPos = null;
        void ApplyTransforming()
        {
            if (UsePhysics && rigb)
            {
                float yVelo = YVelocity;

                if ( !inAir)
                if (yAdjustPos != null)
                {
                    yVelo = (yAdjustPos.Value - rigb.position.y) / Time.fixedDeltaTime;
                    //yAdjustPos = null;
                }

                rigb.velocity = new Vector3(lastVelocity.x, yVelo, lastVelocity.z);
            }
            else
                transform.position = holdJumpPosition;
        }


        /// <summary>
        /// Supporting Root Motion movement
        /// </summary>
        internal virtual void OnAnimatorMove()
        {
            if (UseRootMotionTranslation)
            {
                // Movement Support
                if (!inAir) lastVelocity = animator.velocity;
                animator.rootPosition = transform.position;

                animator.rootRotation = fitter.LastRotation;
            }

            if (UseRootMotionRotation)
            {
                // Rotation Support
                animator.rootRotation = fitter.LastRotation;

                float angleInDegrees; Vector3 rotationAxis;
                animator.deltaRotation.ToAngleAxis(out angleInDegrees, out rotationAxis);

                float rotationValue = (rotationAxis * angleInDegrees * Mathf.Deg2Rad).y;
                fitter.UpAxisRotation += rotationValue / Time.deltaTime;
            }
        }

        /// <summary>
        /// Method executed when object is landing on ground from beeing in air lately
        /// </summary>
        protected virtual void HitGround()
        {
            fitter.RefreshLastRaycast();
            fitter.enabled = true;
            inAir = false;
            freezeJumpYPosition = 0f;
        }

        /// <summary>
        /// Trigger this method so object will jump
        /// </summary>
        public virtual void Jump()
        {
            YVelocity = JumpPower;
            fitter.UpOffset += JumpPower * Time.deltaTime / 2f;
        }

        /// <summary>
        /// Crossfading to target animation with protection of playing same animation over again
        /// </summary>
        protected virtual void CrossfadeTo(string animation, float transitionTime = 0.25f)
        {
            if (!clips.ContainsKey(animation))
            {
                // Preventing holding shift for sprint and starting walking freeze on idle  
                if (animation == "Run") animation = "Walk"; else return;
            }

            if (lastAnim != animation)
            {
                animator.CrossFadeInFixedTime(clips[animation], transitionTime);
                lastAnim = animation;
            }
        }

        /// <summary>
        /// Checking if animator have parameter with choosed name
        /// </summary>
        public static bool HasParameter(Animator animator, string paramName)
        {
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == paramName)
                    return true;
            }
            return false;
        }

        [Tooltip("If you want to set some animator parameter during being grounded")]
        [HideInInspector] public string SetIsGroundedParam = "";
        [Tooltip("If you want to set some animator parameter during accelerating moving")]
        [HideInInspector] public string SetIsMovingParam = "";
        [Tooltip("If using physical move with collider, assigning to the collider sliding material")]
        [HideInInspector] public bool UseSlidingMat = true;

        private static PhysicMaterial pm_Sliding = null;
    }


#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(FGroundFitter_Movement))]
    public class FGroundFitter_MovementEditor : UnityEditor.Editor
    {
        public FGroundFitter_Movement Get { get { if (_get == null) _get = (FGroundFitter_Movement)target; return _get; } }
        private FGroundFitter_Movement _get;

        UnityEditor.SerializedProperty sp_SetIsGroundedParam;
        bool displayParams = false;

        private void OnEnable()
        {
            sp_SetIsGroundedParam = serializedObject.FindProperty("SetIsGroundedParam");
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            GUILayout.Space(4f);

            displayParams = UnityEditor.EditorGUILayout.Foldout(displayParams, " Extra Animator Params", true);

            if (displayParams)
            {
                GUILayout.Space(4);

                UnityEditor.SerializedProperty sp = sp_SetIsGroundedParam.Copy();
                UnityEditor.EditorGUILayout.PropertyField(sp); sp.Next(false);
                UnityEditor.EditorGUILayout.PropertyField(sp); sp.Next(false);
                UnityEditor.EditorGUILayout.PropertyField(sp);
                serializedObject.ApplyModifiedProperties();

                GUILayout.Space(2);
                Animator anim = Get.GetComponentInChildren<Animator>();

                if (anim)
                {
                    if (!AnimatorHasParam(anim, FGroundFitter_Movement._hash_animSp))
                        if (GUILayout.Button("Add 'AnimationSpeed' parameter to the Animator"))
                        {
                            UnityEditor.Animations.AnimatorController aContr = (UnityEditor.Animations.AnimatorController)anim.runtimeAnimatorController;
                            aContr.AddParameter(new AnimatorControllerParameter() { defaultFloat = 1f, name = "AnimationSpeed", type = AnimatorControllerParameterType.Float });
                        }
                }
                GUILayout.Space(4);
            }

        }

        public static bool AnimatorHasParam(Animator anim, int hash)
        {
            UnityEditor.Animations.AnimatorController aContr = (UnityEditor.Animations.AnimatorController)anim.runtimeAnimatorController;
            foreach (AnimatorControllerParameter p in aContr.parameters)
                if (p.nameHash == hash) return true;

            return false;
        }
    }
#endif

}