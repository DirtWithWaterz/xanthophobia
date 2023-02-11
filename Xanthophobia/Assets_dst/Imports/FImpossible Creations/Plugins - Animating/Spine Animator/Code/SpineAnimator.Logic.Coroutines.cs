using System.Collections;
using UnityEngine;

namespace FIMSpace.FSpine
{
    public partial class FSpineAnimator
    {
        /// <summary> Helper counter for start after t-pose feature </summary>
        int initAfterTPoseCounter = 0;

        /// <summary> Helper flag for basic animate physics mode </summary>
        bool fixedUpdated = false;

        // Supporting second solution for fixed animate physics mode
        private bool lateFixedIsRunning = false;
        private bool fixedAllow = true;
        private IEnumerator LateFixed()
        {
            WaitForFixedUpdate fixedWait = new WaitForFixedUpdate();
            lateFixedIsRunning = true;

            while (true)
            {
                yield return fixedWait;
                PreCalibrateBones();
                fixedAllow = true;
                if (lateFixedIsRunning == false) yield break;
            }
        }

    }
}