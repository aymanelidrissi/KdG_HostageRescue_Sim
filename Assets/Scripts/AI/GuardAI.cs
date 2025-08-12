using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class GuardAI : MonoBehaviour
{
    public Transform player;
    public Transform[] patrolPoints;

    public float waypointTolerance = 0.3f;
    public float repathEvery = 0.2f;

    public float viewDistance = 10f;
    [Range(1f, 180f)] public float viewAngle = 90f;
    public float loseAfterSeconds = 1.0f;

    private NavMeshAgent agent;
    private int patrolIndex = 0;

    private enum State { Patrol, Seek, Return }
    private State state = State.Patrol;

    private float repathTimer = 0f;
    private float lostTimer = 0f;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void OnEnable()
    {
        if (patrolPoints != null && patrolPoints.Length > 0)
            SetDestinationSafe(patrolPoints[patrolIndex].position);
    }

    void Update()
    {
        switch (state)
        {
            case State.Patrol: PatrolUpdate(); break;
            case State.Seek: SeekUpdate(); break;
            case State.Return: ReturnUpdate(); break;
        }

        if (player != null)
            TrySeePlayer();
    }

    void PatrolUpdate()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        if (!agent.pathPending &&
            agent.remainingDistance <= Mathf.Max(agent.stoppingDistance, waypointTolerance))
        {
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
            SetDestinationSafe(patrolPoints[patrolIndex].position);
        }
    }

    void SeekUpdate()
    {
        repathTimer -= Time.deltaTime;
        if (repathTimer <= 0f)
        {
            repathTimer = repathEvery;
            SetDestinationSafe(player.position);
        }

        if (HasLineOfSight())
        {
            lostTimer = 0f;
        }
        else
        {
            lostTimer += Time.deltaTime;
            if (lostTimer >= loseAfterSeconds)
            {
                lostTimer = 0f;
                state = State.Return;
            }
        }
    }

    void ReturnUpdate()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            state = State.Patrol;
            return;
        }

        int nearest = 0;
        float best = float.PositiveInfinity;
        Vector3 p = transform.position;

        for (int i = 0; i < patrolPoints.Length; i++)
        {
            Vector3 w = patrolPoints[i].position;
            float d2 = (new Vector3(w.x - p.x, 0f, w.z - p.z)).sqrMagnitude;
            if (d2 < best) { best = d2; nearest = i; }
        }

        patrolIndex = nearest;
        SetDestinationSafe(patrolPoints[patrolIndex].position);
        state = State.Patrol;
    }

    void TrySeePlayer()
    {
        Vector3 toPlayer = player.position - transform.position;
        if (toPlayer.magnitude > viewDistance) return;

        Vector3 forward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
        Vector3 dir = new Vector3(toPlayer.x, 0f, toPlayer.z).normalized;
        if (Vector3.Angle(forward, dir) > viewAngle * 0.5f) return;

        if (HasLineOfSight() && (state == State.Patrol || state == State.Return))
        {
            lostTimer = 0f;
            repathTimer = 0f;
            state = State.Seek;
        }
    }

    bool HasLineOfSight()
    {
        Vector3 origin = transform.position;
        Vector3 dir = (player.position - origin);
        float maxDist = dir.magnitude;

        if (Physics.Raycast(origin, dir.normalized, out RaycastHit hit, maxDist, ~0, QueryTriggerInteraction.Ignore))
            return hit.transform == player || hit.transform.IsChildOf(player);

        return true;
    }

    void SetDestinationSafe(Vector3 worldPos)
    {
        agent.SetDestination(worldPos);
    }
}
