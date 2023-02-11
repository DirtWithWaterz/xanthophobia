using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FSpine
{
    public partial class FSpineAnimator
    {
        /// <summary>
        /// ! Object with SpineAnimator must be active !
        /// Calls coroutine which will smoothly change value of choosen parameter to target value
        /// </summary>
        /// <param name="to"> Depends on parameter but usually From 0 to 1 </param>
        /// <param name="transitionDuration"> Duration in seconds </param>
        /// <param name="executionDelay"> If you want to trigger this coroutine after some delay </param>
        public void User_ChangeParameter(EParamChange parameter, float to, float transitionDuration, float executionDelay = 0f)
        {
            if (transitionDuration <= 0f && executionDelay <= 0f)
            {
                SetValue(parameter, to);
                return;
            }

            StartCoroutine(IEChangeValue(parameter, to, transitionDuration, executionDelay));
        }

        /// <summary>
        /// ! Object with SpineAnimator must be active !
        /// Calls coroutine which will smoothly change value of choosen parameter to target value
        /// And restores back value after choosed duration
        /// </summary>
        /// <param name="to"> Depends on parameter but usually From 0 to 1 </param>
        /// <param name="transitionDuration"> Duration in seconds </param>
        /// <param name="restoreAfter"> Delay after reaching desired value, time in seconds </param>
        public void User_ChangeParameterAndRestore(EParamChange parameter, float to, float transitionDuration, float restoreAfter = 0f)
        {
            float startVal = GetValue(parameter);
            StartCoroutine(IEChangeValue(parameter, to, transitionDuration, 0f));
            StartCoroutine(IEChangeValue(parameter, startVal, transitionDuration, transitionDuration + restoreAfter));
        }


        /// <summary>
        /// Resetting bones pose to the original animator pose (one frame delay)
        /// </summary>
        public void User_ResetBones()
        {
            _ResetBones();
        }


        #region Handling Utilities

        public enum EParamChange
        {
            /// <summary> Value from 0 to 1 </summary>
            GoBackSpeed,
            /// <summary> Value from 0 to 1 </summary>
            SpineAnimatorAmount,
            /// <summary> Value from 1 to 91 </summary>
            AngleLimit,
            /// <summary> Value from 0 to 15 </summary>
            StraightenSpeed,
            /// <summary> Value from 0 to 1 </summary>
            PositionSmoother,
            /// <summary> Value from 0 to 1 </summary>
            RotationSmoother
        }

        private IEnumerator IEChangeValue(EParamChange param, float to, float duration, float executionDelay)
        {
            if (executionDelay > 0f)
            {
                yield return new WaitForSeconds(executionDelay);
            }

            if (duration > 0f)
            {
                float elapsed = 0f;
                float startVal = GetValue(param);

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float progress = elapsed / duration;
                    if (progress > 1f) progress = 1f;
                    SetValue(param, Mathf.LerpUnclamped(startVal, to, progress));
                    yield return null;
                }
            }

            SetValue(param, to);

            yield break;
        }




        float GetValue(EParamChange param)
        {
            switch (param)
            {
                case EParamChange.GoBackSpeed: return GoBackSpeed;
                case EParamChange.SpineAnimatorAmount: return SpineAnimatorAmount;
                case EParamChange.AngleLimit: return AngleLimit;
                case EParamChange.StraightenSpeed: return StraightenSpeed;
                case EParamChange.PositionSmoother: return PosSmoother;
                case EParamChange.RotationSmoother: return RotSmoother;
            }

            return 0f;
        }

        void SetValue(EParamChange param, float val)
        {
            switch (param)
            {
                case EParamChange.GoBackSpeed: GoBackSpeed = val; break;
                case EParamChange.SpineAnimatorAmount: SpineAnimatorAmount = val; break;
                case EParamChange.AngleLimit: AngleLimit = val; break;
                case EParamChange.StraightenSpeed: StraightenSpeed = val; break;
                case EParamChange.PositionSmoother: PosSmoother = val; break;
                case EParamChange.RotationSmoother: RotSmoother = val; break;
            }
        }


        void _ResetBones()
        {
            if (LastBoneLeading == false)
            {
                for (int i = SpineBones.Count - 1; i >= 0; i--)
                {
                    SpineBones[i].ProceduralPosition = SpineBones[i].ReferencePosition;
                    SpineBones[i].ProceduralRotation = SpineBones[i].ReferenceRotation;

                    SpineBones[i].PreviousPosition = SpineBones[i].ReferencePosition;

                    SpineBones[i].FinalPosition = SpineBones[i].ReferencePosition;
                    SpineBones[i].FinalRotation = SpineBones[i].ReferenceRotation;
                }
            }
            else
            {
                for (int i = 0; i < SpineBones.Count; i++)
                {
                    SpineBones[i].ProceduralPosition = SpineBones[i].ReferencePosition;
                    SpineBones[i].ProceduralRotation = SpineBones[i].ReferenceRotation;

                    SpineBones[i].PreviousPosition = SpineBones[i].ReferencePosition;

                    SpineBones[i].FinalPosition = SpineBones[i].ReferencePosition;
                    SpineBones[i].FinalRotation = SpineBones[i].ReferenceRotation;
                }
            }

            float preBack = GoBackSpeed;
            GoBackSpeed = 10f;
            Update();
            FixedUpdate();

            delta = .25f;
            LateUpdate();

            GoBackSpeed = preBack;
        }

        #endregion


    }
}