using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BaseNPC : MonoBehaviour, IBaseNPC
{
    public NavMeshAgent agent;
    public Animator animator;

    [Header("Movement")]
    public float stopDistance = 0.5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null) agent = gameObject.AddComponent<NavMeshAgent>();
    }

    // Update is called once per frame
   

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
}
