using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

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

    public string fleeTriggerTag = "KeyItem";
    public bool requireHeldKey = true;
    public float fleeDuration = 2f;
    public float fleeDistance = 5f;
    public float fleeSpeed = 3.5f;

    NavMeshAgent agent;
    int patrolIndex = 0;

    enum State { Patrol, Seek, Return, Flee }
    State state = State.Patrol;

    float repathTimer = 0f;
    float lostTimer = 0f;

    float fleeUntil = 0f;
    float originalSpeed = 0f;

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
            case State.Flee: FleeUpdate(); break;
        }

        if (player != null && state != State.Flee)
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

    void FleeUpdate()
    {
        if (Time.time >= fleeUntil)
        {
            if (agent) agent.speed = originalSpeed;
            state = State.Return;
        }
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

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(fleeTriggerTag)) return;

        if (requireHeldKey)
        {
            var grab = other.GetComponentInParent<XRGrabInteractable>();
            if (grab == null || !grab.isSelected) return;
        }

        StartFlee(other.bounds.center);
    }

    void StartFlee(Vector3 threatPos)
    {
        if (agent == null) return;

        originalSpeed = agent.speed;
        agent.speed = fleeSpeed;

        Vector3 origin = transform.position;
        Vector3 dir = (origin - threatPos).normalized;
        if (dir.sqrMagnitude < 0.001f) dir = transform.forward;

        Vector3 fleePoint;
        if (!FindFleePoint(origin, dir, fleeDistance, out fleePoint))
            fleePoint = origin + dir * fleeDistance;

        agent.ResetPath();
        agent.SetDestination(fleePoint);

        fleeUntil = Time.time + fleeDuration;
        state = State.Flee;
        lostTimer = 0f;
    }

    bool FindFleePoint(Vector3 origin, Vector3 dir, float distance, out Vector3 result)
    {
        for (int i = -3; i <= 3; i++)
        {
            Vector3 testDir = Quaternion.Euler(0f, i * 20f, 0f) * dir;
            Vector3 target = origin + testDir * distance;
            if (NavMesh.SamplePosition(target, out NavMeshHit hit, 2.5f, NavMesh.AllAreas))
            { result = hit.position; return true; }
        }
        result = origin;
        return false;
    }
}
