using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class CowardAI : MonoBehaviour
{
    public Transform player;
    public Transform[] patrolPoints;
    public float waypointTolerance = 0.3f;

    public float viewDistance = 12f;
    [Range(1f, 180f)] public float viewAngle = 120f;

    public float patrolSpeed = 2.0f;
    public float fleeSpeed = 3.6f;
    public float fleeDistance = 6f;
    public float fleeDuration = 2.0f;

    NavMeshAgent agent;
    int patrolIndex = 0;
    int losMask;
    float fleeUntil = 0f;

    enum State { Patrol, Flee, Return }
    State state = State.Patrol;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        losMask = ~(1 << gameObject.layer);
    }

    void OnEnable()
    {
        agent.speed = patrolSpeed;
        if (patrolPoints != null && patrolPoints.Length > 0)
            agent.SetDestination(patrolPoints[patrolIndex].position);
    }

    void Update()
    {
        switch (state)
        {
            case State.Patrol: PatrolUpdate(); TrySeePlayer(); break;
            case State.Flee: FleeUpdate(); break;
            case State.Return: ReturnUpdate(); TrySeePlayer(); break;
        }
    }

    void PatrolUpdate()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        if (!agent.pathPending &&
            agent.remainingDistance <= Mathf.Max(agent.stoppingDistance, waypointTolerance))
        {
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
            agent.SetDestination(patrolPoints[patrolIndex].position);
        }
    }

    void ReturnUpdate()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) { state = State.Patrol; return; }

        if (!agent.pathPending &&
            agent.remainingDistance <= Mathf.Max(agent.stoppingDistance, waypointTolerance))
        {
            state = State.Patrol;
            agent.speed = patrolSpeed;
            agent.SetDestination(patrolPoints[patrolIndex].position);
        }
    }

    void FleeUpdate()
    {
        if (Time.time >= fleeUntil)
        {
            ChooseNearestWaypoint();
            state = State.Return;
            agent.speed = patrolSpeed;
        }
    }

    void TrySeePlayer()
    {
        if (!player) return;

        Vector3 to = player.position - transform.position;
        if (to.magnitude > viewDistance) return;

        Vector3 fwd = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
        Vector3 dir = new Vector3(to.x, 0f, to.z).normalized;
        if (Vector3.Angle(fwd, dir) > viewAngle * 0.5f) return;

        if (HasLOS()) StartFlee(dir);
    }

    bool HasLOS()
    {
        Vector3 o = transform.position + Vector3.up * 1.2f;
        Vector3 d = (player.position - o);
        float max = d.magnitude;
        if (Physics.Raycast(o, d.normalized, out var hit, max, losMask, QueryTriggerInteraction.Ignore))
            return hit.transform == player || hit.transform.IsChildOf(player);
        return true;
    }

    void StartFlee(Vector3 dirToPlayer)
    {
        Vector3 dir = (-dirToPlayer).normalized;
        if (dir.sqrMagnitude < 0.001f) dir = -transform.forward;

        Vector3 origin = transform.position;
        Vector3 fleePoint;
        if (!FindFleePoint(origin, dir, fleeDistance, out fleePoint))
            fleePoint = origin + dir * fleeDistance;

        agent.ResetPath();
        agent.speed = fleeSpeed;
        agent.SetDestination(fleePoint);

        fleeUntil = Time.time + fleeDuration;
        state = State.Flee;
    }

    void ChooseNearestWaypoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        int nearest = 0; float best = float.PositiveInfinity;
        Vector3 p = transform.position;
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            Vector3 w = patrolPoints[i].position;
            float d2 = (new Vector3(w.x - p.x, 0f, w.z - p.z)).sqrMagnitude;
            if (d2 < best) { best = d2; nearest = i; }
        }
        patrolIndex = nearest;
        agent.SetDestination(patrolPoints[patrolIndex].position);
    }

    bool FindFleePoint(Vector3 origin, Vector3 dir, float distance, out Vector3 result)
    {
        for (int i = -4; i <= 4; i++)
        {
            Vector3 testDir = Quaternion.Euler(0f, i * 20f, 0f) * dir;
            Vector3 target = origin + testDir * distance;
            if (NavMesh.SamplePosition(target, out var hit, 2.5f, NavMesh.AllAreas))
            { result = hit.position; return true; }
        }
        result = origin;
        return false;
    }
}
