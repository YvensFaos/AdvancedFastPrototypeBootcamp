using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerNavigation : MonoBehaviour
{
    private NavMeshAgent _navMeshAgent;

    [SerializeField]
    public Transform moveTowards;
    
    private void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (moveTowards != null)
        {
            _navMeshAgent.destination = moveTowards.position;    
        }
    }
}
