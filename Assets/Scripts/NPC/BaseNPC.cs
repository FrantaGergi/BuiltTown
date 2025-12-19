using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BaseNPC : MonoBehaviour, IBaseNPC
{
    public NavMeshAgent agent;
    public Animator animator;

    [Header("Movement")]
    public float stopDistance = 0.5f;

    // UI handle - pøiøadí NPCManager pøi registraci
    public UINPC.NPCRowHandle UiRow { get; set; }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null) agent = gameObject.AddComponent<NavMeshAgent>();

        // aby se pøedešlo problémùm s kolizemi mezi NPC
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;


    }



    public void MoveTo(Vector3 position)
    {
        if (agent == null) return;
        agent.isStopped = false;
        agent.SetDestination(position);
        animator?.SetBool("IsMoving", true);
    }

    public bool IsAtDestination(float tolerance = -1f)
    {
        if (tolerance < 0) tolerance = stopDistance;
        if (agent == null || !agent.hasPath) return false;
        return !agent.pathPending && agent.remainingDistance <= tolerance;
    }

    public void Stop()
    {
        if (agent == null) return;
        agent.isStopped = true;
        agent.ResetPath();
        animator?.SetBool("IsMoving", false);
    }

    public void LookAtTarget(Vector3 target)
    {
        Vector3 dir = target - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 10f * Time.deltaTime);
    }

    // Convenience: aktualizace UI pøímo z NPC / role
    public void SetDisplayName(string displayName)
    {
        UiRow?.SetDisplayName(displayName);
    }

    public void SetUiStatus(string status)
    {
        UiRow?.SetStatus(status);
    }

    public void SetUiDistrictSatus(string status)
    {
        UiRow?.SetDistrictStatus(status);
    }

    public void SetHighlightSection(int index)
    {
        UiRow?.SetHighlightSection(index);
    }

   
}