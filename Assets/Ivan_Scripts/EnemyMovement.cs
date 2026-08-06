using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMovement : MonoBehaviour
{
    [Header("Rotation")]
    public float rotationSpeed = 8f;

    private NavMeshAgent agent;

    public bool IsPhysicallyMoving => agent.velocity.sqrMagnitude > 0.1f;
    public bool IsBlocked => agent.hasPath && agent.velocity.sqrMagnitude < 0.05f;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = 0.5f;
    }

    public void MoveTo(Vector3 destination)
    {
        agent.isStopped = false;
        agent.updateRotation = true;
        agent.SetDestination(destination);
    }

    public void Stop()
    {
        agent.isStopped = true;
        agent.updateRotation = false;
    }

    public bool HasReachedTarget(Vector3 targetPos, float threshold = 1.8f)
    {
        return Vector3.Distance(transform.position, targetPos) <= threshold;
    }

    public void RotateTowards(Vector3 targetPos)
    {
        Vector3 dir = Vector3.ProjectOnPlane(targetPos - transform.position, Vector3.up).normalized;
        if (dir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }

    public void SetRandomSpeed(float minSpeed, float maxSpeed)
    {
        agent.speed = Random.Range(minSpeed, maxSpeed);
    }
}