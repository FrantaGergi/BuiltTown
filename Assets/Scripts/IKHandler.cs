using UnityEngine;

public class IKHandler : MonoBehaviour
{
    public Animator animator;
    public Transform leftHandTarget;
    public Transform rightHandTarget;

    private bool useIK = false;
    private bool isTransitioningOff = false;

    private Vector3 rightHandStartPos, leftHandStartPos;
    private Quaternion rightHandStartRot, leftHandStartRot;
    private float transitionTime = 1f;
    private float rightHandLerp, leftHandLerp;
    private float ikWeight = 1f;

    public void SetTargets(Transform leftTarget, Transform rightTarget)
    {
        if (leftTarget == null || rightTarget == null)
        {
            // Zahájíme plynulý návrat
            if (useIK)
            {
                isTransitioningOff = true;
                ikWeight = 1f;
                rightHandLerp = 0f;
                leftHandLerp = 0f;
                rightHandStartPos = animator.GetIKPosition(AvatarIKGoal.RightHand);
                rightHandStartRot = animator.GetIKRotation(AvatarIKGoal.RightHand);
                leftHandStartPos = animator.GetIKPosition(AvatarIKGoal.LeftHand);
                leftHandStartRot = animator.GetIKRotation(AvatarIKGoal.LeftHand);
            }
            leftHandTarget = null;
            rightHandTarget = null;
            return;
        }

        // Zahájíme plynulý pøechod na target
        if (!useIK)
        {
            rightHandStartPos = animator.GetIKPosition(AvatarIKGoal.RightHand);
            rightHandStartRot = animator.GetIKRotation(AvatarIKGoal.RightHand);
            leftHandStartPos = animator.GetIKPosition(AvatarIKGoal.LeftHand);
            leftHandStartRot = animator.GetIKRotation(AvatarIKGoal.LeftHand);
            rightHandLerp = 0f;
            leftHandLerp = 0f;
        }

        useIK = true;
        isTransitioningOff = false;
        leftHandTarget = leftTarget;
        rightHandTarget = rightTarget;
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (animator == null)
            return;

        // Plynulý návrat IK OFF
        if (isTransitioningOff)
        {
            ikWeight -= Time.deltaTime / transitionTime;
            ikWeight = Mathf.Clamp01(ikWeight);

            // Interpolace pozice/rotace zpìt do animace
            Vector3 rightAnimPos = animator.GetIKPosition(AvatarIKGoal.RightHand);
            Quaternion rightAnimRot = animator.GetIKRotation(AvatarIKGoal.RightHand);
            Vector3 leftAnimPos = animator.GetIKPosition(AvatarIKGoal.LeftHand);
            Quaternion leftAnimRot = animator.GetIKRotation(AvatarIKGoal.LeftHand);

            rightHandLerp += Time.deltaTime / transitionTime;
            leftHandLerp += Time.deltaTime / transitionTime;

            Vector3 rightPos = Vector3.Lerp(rightHandStartPos, rightAnimPos, rightHandLerp);
            Quaternion rightRot = Quaternion.Slerp(rightHandStartRot, rightAnimRot, rightHandLerp);
            Vector3 leftPos = Vector3.Lerp(leftHandStartPos, leftAnimPos, leftHandLerp);
            Quaternion leftRot = Quaternion.Slerp(leftHandStartRot, leftAnimRot, leftHandLerp);

            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, ikWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, ikWeight);
            animator.SetIKPosition(AvatarIKGoal.RightHand, rightPos);
            animator.SetIKRotation(AvatarIKGoal.RightHand, rightRot);

            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, ikWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, ikWeight);
            animator.SetIKPosition(AvatarIKGoal.LeftHand, leftPos);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, leftRot);

            if (ikWeight <= 0f)
            {
                useIK = false;
                isTransitioningOff = false;
            }
            return;
        }

        if (!useIK)
            return;

        // Pravá ruka
        if (rightHandTarget != null)
        {
            if (rightHandLerp < 1f)
                rightHandLerp += Time.deltaTime / transitionTime;

            Vector3 pos = Vector3.Lerp(rightHandStartPos, rightHandTarget.position, rightHandLerp);
            Quaternion rot = Quaternion.Slerp(rightHandStartRot, rightHandTarget.rotation, rightHandLerp);

            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
            animator.SetIKPosition(AvatarIKGoal.RightHand, pos);
            animator.SetIKRotation(AvatarIKGoal.RightHand, rot);
        }

        // Levá ruka
        if (leftHandTarget != null)
        {
            if (leftHandLerp < 1f)
                leftHandLerp += Time.deltaTime / transitionTime;

            Vector3 pos = Vector3.Lerp(leftHandStartPos, leftHandTarget.position, leftHandLerp);
            Quaternion rot = Quaternion.Slerp(leftHandStartRot, leftHandTarget.rotation, leftHandLerp);

            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
            animator.SetIKPosition(AvatarIKGoal.LeftHand, pos);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, rot);
        }
    }
}
