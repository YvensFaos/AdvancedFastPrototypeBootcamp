using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerNavigation : MonoBehaviour
{
    [SerializeField]
    private NavMeshAgent navMeshAgent;
    [SerializeField]
    private BoxCollider walkableArea;
    [SerializeField]
    private float thinkTimer;
    [SerializeField] 
    private float fieldOfViewRadius;
    [SerializeField] 
    private float approachTolerance = 0.5f;
    [SerializeField] 
    private LayerMask interestMask;
    [SerializeField] 
    private LayerMask terrainMask;

    private int _score;
    private Vector3 _nextPosition = Vector3.zero;

    private void Awake()
    {
        if (navMeshAgent == null)
        {
            navMeshAgent = GetComponent<NavMeshAgent>();    
        }
        
        StartCoroutine(PlayerNavigationCoroutine());
    }

    private IEnumerator PlayerNavigationCoroutine()
    {
        var selfTransform = transform;
        var validPosition = false;
        
        while (true)
        {
            navMeshAgent.isStopped = true;
            yield return new WaitForSeconds(thinkTimer);
            var found = Physics.OverlapSphere(selfTransform.position, 
                fieldOfViewRadius, interestMask);
            if (found.Length > 0)
            {
                _nextPosition = found[0].transform.position;
            }
            else
            {
                do
                {
                    var nextSpot = RandomHelper.RandomPointWithinArea(walkableArea);
                    nextSpot.y += 10.0f;
                    if (!Physics.Raycast(nextSpot, Vector3.down,
                        out var hit, 20.0f, terrainMask)) continue;
                    if (!NavMesh.SamplePosition(hit.point, out var navMeshHit, 
                        1.0f, NavMesh.AllAreas)) continue;

                    validPosition = true;
                    _nextPosition = navMeshHit.position;
                    
                } while (!validPosition);
            }
            validPosition = false;
            navMeshAgent.isStopped = false;
            navMeshAgent.destination = _nextPosition;
            yield return new WaitUntil(() => 
                navMeshAgent.remainingDistance <= approachTolerance);
            _nextPosition = Vector3.zero;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Fruit")) return;
        Destroy(other.gameObject);
        ++_score;
        gameObject.name = $"Player A [{_score}]";
    }

    private void OnDrawGizmos()
    {
        var transformPosition = transform.position;
        Gizmos.DrawWireSphere(transformPosition,fieldOfViewRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transformPosition,approachTolerance);
        if (navMeshAgent == null || !navMeshAgent.isOnNavMesh || navMeshAgent.isStopped) return;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transformPosition,_nextPosition);
    }
}
