using UnityEngine;

namespace FIMSpace.GroundFitter
{
    public class FGroundFitter_Input : FGroundFitter_InputBase
    {
        protected virtual void Update()
        {

            if (Input.GetKeyDown(KeyCode.Space)) TriggerJump();

            Vector3 dir = Vector3.zero;

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
            {
                if (Input.GetKey(KeyCode.LeftShift)) Sprint = true; else Sprint = false;

                if (Input.GetKey(KeyCode.W)) dir.z += 1f;
                if (Input.GetKey(KeyCode.A)) dir.x -= 1f;
                if (Input.GetKey(KeyCode.D)) dir.x += 1f;
                if (Input.GetKey(KeyCode.S)) dir.z -= 1f;

                dir.Normalize();

                RotationOffset = Quaternion.LookRotation(dir).eulerAngles.y;

                MoveVector = Vector3.forward;
            }
            else
            {
                Sprint = false;
                MoveVector = Vector3.zero;
            }

            if (Input.GetKey(KeyCode.X)) MoveVector -= Vector3.forward;
            if (Input.GetKey(KeyCode.Q)) MoveVector += Vector3.left;
            if (Input.GetKey(KeyCode.E)) MoveVector += Vector3.right;

            MoveVector.Normalize();

            controller.Sprint = Sprint;
            controller.MoveVector = MoveVector;
            controller.RotationOffset = RotationOffset;
        }
    }
}