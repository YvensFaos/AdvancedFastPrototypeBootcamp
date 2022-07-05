using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerNavigation : MonoBehaviour
{
    [SerializeField]
    private NavMeshAgent navMeshAgent;
    [SerializeField]
    private BoxCollider walkableArea;
    [SerializeField]
    private float regularSpeed;
    [SerializeField]
    private float sprintSpeed;
    [SerializeField]
    private float sprintCooldown;
    [SerializeField]
    private float thinkTimer;
    [SerializeField] 
    private float recheckTimer;
    [SerializeField] 
    private float fieldOfViewRadius;
    [SerializeField] 
    private float approachTolerance = 0.5f;
    [SerializeField] 
    private LayerMask interestMask;
    [SerializeField] 
    private LayerMask terrainMask;
    [SerializeField] 
    private LayerMask playerMask;
    [SerializeField] 
    private UnityEvent pickupEvent;
    [SerializeField] 
    private UnityEvent recheckEvent;
    [SerializeField] 
    private UnityEvent sprintEvent;

    private int _score;
    private Vector3 _nextPosition = Vector3.zero;
    private string _originalName;
    private bool _sprintAvailable;
    

    private void Awake()
    {
        if (navMeshAgent == null)
        {
            navMeshAgent = GetComponent<NavMeshAgent>();    
        }
        
        StartCoroutine(PlayerNavigationCoroutine());
        _originalName = gameObject.name;
        _sprintAvailable = true;
    }

    private IEnumerator PlayerNavigationCoroutine()
    {
        void ActivateSprint()
        {
            navMeshAgent.speed = sprintSpeed;
            _sprintAvailable = false;
            StartCoroutine(SprintCooldown());
            sprintEvent.Invoke();
        }

        Collider[] LookAround(Transform objectTransform, LayerMask mask)
        {
            var o = gameObject;
            var layer = o.layer;
            o.layer = 2;
            var found = Physics.OverlapSphere(objectTransform.position,
                fieldOfViewRadius, mask);
            gameObject.layer = layer;
            return found;
        }

        var selfTransform = transform;
        
        bool LookForFruits(out Vector3 fruitPosition, out FruitBehavior fruit)
        {
            float CalculateFruitScore(FruitBehavior fruitBehavior)
            {
                var fruitScore = fruitBehavior.FruitScore /
                                 Vector3.SqrMagnitude(selfTransform.position - fruitBehavior.transform.position);
                fruitBehavior.name = $"{fruitScore}";
                return fruitScore;
            }

            fruitPosition = Vector3.zero;
            fruit = null;
            
            var found = LookAround(selfTransform, interestMask);
            if (found.Length <= 0) return found.Length > 0;

            var fruits = found.Select(o => o.GetComponent<FruitBehavior>());
            var orderedFruits = fruits.OrderBy(CalculateFruitScore);
            fruit = orderedFruits.Last();
            fruitPosition = fruit.transform.position;
            return found.Length > 0;
        }

        bool LookForOtherPlayers(out Collider[] players)
        {
            players = LookAround(selfTransform, playerMask);
            return players != null && players.Length > 0;
        }
        
        while (true)
        {
            var validFruit = true;
            navMeshAgent.isStopped = true;
            yield return new WaitForSeconds(thinkTimer);
            if (LookForFruits(out var fruitPosition, out var fruit))
            {
                _nextPosition = fruitPosition;
                MoveToNextPosition();
                if (_sprintAvailable && LookForOtherPlayers(out _))
                {
                    ActivateSprint();
                }
                
                while (validFruit && navMeshAgent.remainingDistance > approachTolerance)
                {
                    yield return new WaitForSeconds(recheckTimer);
                    if (fruit != null) continue;
                    //Fruit was eaten by someone else
                    validFruit = false;
                    recheckEvent.Invoke();
                }
            }
            else
            {
                var validPosition = false;
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
                
                MoveToNextPosition();
                var validRandomWalk = true;
                while (validRandomWalk && navMeshAgent.remainingDistance > approachTolerance)
                {
                    yield return new WaitForSeconds(recheckTimer);
                    if (LookAround(selfTransform, interestMask).Length <= 0) continue;

                    if (LookForOtherPlayers(out var players))
                    {
                        
                    }
                    
                    //Found something interesting
                    validRandomWalk = false;
                    recheckEvent.Invoke();
                }
            }
            _nextPosition = Vector3.zero;
        }
    }

    private IEnumerator SprintCooldown()
    {
        yield return new WaitForSeconds(sprintCooldown);
        _sprintAvailable = true;
        navMeshAgent.speed = regularSpeed;
    }

    private void MoveToNextPosition()
    {
        navMeshAgent.isStopped = false;
        navMeshAgent.destination = _nextPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Fruit")) return;
        GameObject fruitObject;
        var fruit = (fruitObject = other.gameObject).GetComponent<FruitBehavior>();
        _score += fruit.FruitScore;
        Destroy(fruitObject);
        gameObject.name = $"{_originalName} [{_score}]";
        pickupEvent.Invoke();
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
