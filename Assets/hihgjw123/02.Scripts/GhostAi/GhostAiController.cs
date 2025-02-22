using UnityEngine;
using UnityEngine.AI;

public class GhostAiController : MonoBehaviour
{
    [SerializeField] Transform player; // ?ë ?´ì´ ?ì¹ë¥?ì¶ì ?ê¸° ?í´ ?ì
    [SerializeField] float detectionRange = 10f; // ê·??ê°ì? ê±°ë¦¬
    [SerializeField] float fieldOfView = 100f; // ?ì¼ê°?(100??
    [SerializeField] float patrolWaitTime = 2f; // ?ì°° ?¬ì¸???ì°© ???ê¸°ìê°?
    [SerializeField] float navMeshSearchRadius = 20f; // ?ë¤ ?¬ì¸?¸ë? ì°¾ì ë²ì
    [SerializeField] HeartbeatEffect heartbeatEffect;

    public Animator animator;
    private NavMeshAgent navMeshAgent;
    private bool isChasing = false; // ì¶ì  ì¤ì¸ì§ ?íë¥?ì²´í¬
    private float waitTimer = 0; // ?ê¸??ê°

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        SetRandomPatrolPoint(); // ì´ê¸° ?ì°° ?¬ì¸??ì§??
    }

    private void Update()
    {
        if (!isChasing)
        {
            CheckForPlayer(); // ?ë ?´ì´ ê°ì?
        }

        if (isChasing)
        {
            navMeshAgent.SetDestination(player.position); // ?ë ?´ì´ ?ì¹ë¡?ì¶ê²©

            // ?ë ?´ì´ ?ì¹???ì°©?ì¼ë©??ì°° ?íë¡??í
            if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            {
                StopChase();
            }
        }
        else // ?ì°° ?í????
        {
            if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            {
                waitTimer += Time.deltaTime;
                if (waitTimer >= patrolWaitTime)
                {
                    waitTimer = 0f;
                    SetRandomPatrolPoint();
                }
            }
        }
    }

    private void CheckForPlayer()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // ê°ì? ê±°ë¦¬ ?ì ?ëì§ ?ì¸
        if (distanceToPlayer > detectionRange)
        {
            return; // ê°ì? ê±°ë¦¬ ë°ì´ë©?ë¦¬í´
        }

        // ?ì¼ê°??ì ?ëì§ ?ì¸
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        if (angleToPlayer > fieldOfView / 2f)
        {
            return; // ?ì¼ê°?ë°ì´ë©?ë¦¬í´
        }

        // Raycastë¡??¥ì ë¬??¬ë? ?ì¸
        int layerMask = LayerMask.GetMask("Player", "LibraryObject"); // ?ë ?´ì´? ?¥ì ë¬??ì´??
        if (Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit, detectionRange, layerMask))
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Player")) // ?¥ì ë¬??ì´ ?ë ?´ì´ ë³´ì
            {
                Debug.Log("?ë ?´ì´ ê°ì?");
                StartChase();
                heartbeatEffect.isEffectActive = true;
                heartbeatEffect.heartbeatSound.Play();
            }
            else
            {
                Debug.Log("?¥ì ë¬?ê°ì?");
            }
        }
    }

    private void SetRandomPatrolPoint() // ?ì°° ?¬ì¸?¸ë? ?ë¤?¼ë¡ ì§?í???¨ì
    {
        Vector3 randomDirection = Random.insideUnitSphere * navMeshSearchRadius; // ?ë¤ ë°©í¥
        randomDirection += transform.position;
        animator.SetTrigger("Patrol");
        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, navMeshSearchRadius, NavMesh.AllAreas))
        {
            navMeshAgent.SetDestination(hit.position);
        }
        else
        {
            Debug.LogWarning("? í¨??NavMesh ?¬ì¸?¸ë? ì°¾ì? ëª»í?µë??");
        }
    }

    private void StartChase() // ì¶ê²© ?ì
    {
        isChasing = true;
        animator.ResetTrigger("Patrol");
        animator.SetTrigger("Chase");
        navMeshAgent.speed = 2.5f;
        Debug.Log("ì¶ê²© ?ì");
    }

    public void StopChase() // ì¶ê²© ì¢ë£
    {
        isChasing = false;
        navMeshAgent.speed = 0.5f;
        SetRandomPatrolPoint(); // ?ì°° ?íë¡??í
        animator.ResetTrigger("Chase");
        animator.SetTrigger("Patrol");
        Debug.Log("ì¶ê²© ì¢ë£, ?ì°° ?ì");
    }
}
