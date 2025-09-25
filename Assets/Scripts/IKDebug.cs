using UnityEngine;

public class IKDebug : MonoBehaviour
{
    public Animator animator;
    public Transform leftHandTarget;
    public Transform rightHandTarget;
    public bool useIK = false;
    public float ikWeight = 1f;

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (animator == null) Debug.LogError("IKDebug: Animator je null! Pøipoj skript na GameObject s Animator.");
        else Debug.Log($"IKDebug: animator.isHuman = {animator.isHuman}");
    }

   

    void OnAnimatorIK(int layerIndex)
    {
        Debug.Log($"OnAnimatorIK called (layer {layerIndex}) useIK={useIK}");

        if (animator == null) return;

        if (!useIK)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
            return;
        }

        if (!animator.isHuman)
        {
            Debug.LogWarning("Animator není Humanoid — AvatarIKGoal nebude fungovat. Pøepni model na Humanoid nebo použij Animation Rigging.");
            return;
        }

        if (rightHandTarget != null)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, ikWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, ikWeight);
            animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);
            animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);
        }

        if (leftHandTarget != null)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, ikWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, ikWeight);
            animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
        }
    }
}
