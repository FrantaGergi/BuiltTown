using UnityEngine;

/// <summary>
/// Unified animation controller for all NPC types.
/// Handles walking, idle, and role-specific work animations.
/// </summary>
[RequireComponent(typeof(Animator))]
public class NPCAnimationController : MonoBehaviour
{
    [Header("References")]
    private Animator animator;
    private BaseNPC baseNPC;

    [Header("Animation Parameters")]
    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
    private static readonly int IsMiningHash = Animator.StringToHash("IsMining");
    private static readonly int IsCarryingHash = Animator.StringToHash("IsCarrying");
    private static readonly int IsBuildingHash = Animator.StringToHash("IsBuilding");
    private static readonly int WorkSpeedHash = Animator.StringToHash("WorkSpeed");

    [Header("Movement Detection")]
    [SerializeField] private float movementThreshold = 0.1f;
    private Vector3 lastPosition;
    private float currentSpeed;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        baseNPC = GetComponent<BaseNPC>();

        if (animator == null)
            Debug.LogError($"NPCAnimationController: Animator not found on {gameObject.name}");

        lastPosition = transform.position;
    }

    private void Update()
    {
        UpdateMovementAnimation();
    }

    #region Movement Animation

    private void UpdateMovementAnimation()
    {
        // Calculate current movement speed
        Vector3 currentPosition = transform.position;
        currentSpeed = Vector3.Distance(currentPosition, lastPosition) / Time.deltaTime;
        lastPosition = currentPosition;

        // Update walking animation based on speed
        bool isWalking = currentSpeed > movementThreshold;
        SetWalking(isWalking);

        if (showDebugInfo && isWalking)
            Debug.Log($"{gameObject.name} walking at speed: {currentSpeed:F2}");
    }

    public void SetWalking(bool walking)
    {
        if (animator != null)
            animator.SetBool(IsWalkingHash, walking);
    }

    #endregion

    #region Role-Specific Animations

    /// <summary>
    /// Start mining animation (for Miner role)
    /// </summary>
    public void StartMining(float speed = 1f)
    {
        if (animator == null) return;

        animator.SetBool(IsMiningHash, true);
        animator.SetFloat(WorkSpeedHash, speed);

        if (showDebugInfo)
            Debug.Log($"{gameObject.name} started mining animation (speed: {speed})");
    }

    /// <summary>
    /// Stop mining animation
    /// </summary>
    public void StopMining()
    {
        if (animator == null) return;

        animator.SetBool(IsMiningHash, false);

        if (showDebugInfo)
            Debug.Log($"{gameObject.name} stopped mining animation");
    }

    /// <summary>
    /// Set carrying state (for Collector role)
    /// </summary>
    public void SetCarrying(bool carrying)
    {
        if (animator == null) return;

        animator.SetBool(IsCarryingHash, carrying);

        if (showDebugInfo)
            Debug.Log($"{gameObject.name} carrying: {carrying}");
    }

    /// <summary>
    /// Start building animation (for Builder role)
    /// </summary>
    public void StartBuilding(float speed = 1f)
    {
        if (animator == null) return;

        animator.SetBool(IsBuildingHash, true);
        animator.SetFloat(WorkSpeedHash, speed);

        if (showDebugInfo)
            Debug.Log($"{gameObject.name} started building animation (speed: {speed})");
    }

    /// <summary>
    /// Stop building animation
    /// </summary>
    public void StopBuilding()
    {
        if (animator == null) return;

        animator.SetBool(IsBuildingHash, false);

        if (showDebugInfo)
            Debug.Log($"{gameObject.name} stopped building animation");
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Reset all animation states to idle
    /// </summary>
    public void ResetAllStates()
    {
        if (animator == null) return;

        animator.SetBool(IsWalkingHash, false);
        animator.SetBool(IsMiningHash, false);
        animator.SetBool(IsCarryingHash, false);
        animator.SetBool(IsBuildingHash, false);
        animator.SetFloat(WorkSpeedHash, 1f);

        if (showDebugInfo)
            Debug.Log($"{gameObject.name} reset all animation states");
    }

    /// <summary>
    /// Get current animation state info
    /// </summary>
    public string GetCurrentStateName()
    {
        if (animator == null) return "No Animator";

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName("Idle") ? "Idle" :
               stateInfo.IsName("Walk") ? "Walk" :
               stateInfo.IsName("Mining") ? "Mining" :
               stateInfo.IsName("Carrying") ? "Carrying" :
               stateInfo.IsName("Building") ? "Building" : "Unknown";
    }

    /// <summary>
    /// Force immediate transition to idle
    /// </summary>
    public void ForceIdle()
    {
        ResetAllStates();
        if (animator != null)
            animator.Play("Idle", 0, 0f);
    }

    #endregion

    #region Debug Visualization

    private void OnDrawGizmos()
    {
        if (!showDebugInfo) return;

        // Draw speed indicator
        if (Application.isPlaying && currentSpeed > movementThreshold)
        {
            Gizmos.color = Color.cyan;
            Vector3 start = transform.position + Vector3.up * 2f;
            Vector3 end = start + transform.forward * currentSpeed;
            Gizmos.DrawLine(start, end);
            Gizmos.DrawWireSphere(end, 0.1f);
        }
    }

    #endregion
}