using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class FruitSpawner : MonoBehaviour
{
    [SerializeField]
    private BoxCollider area;
    [SerializeField]
    private float timer;
    [SerializeField]
    private List<GameObject> fruits;

    private float offset = 10.0f;
    
    private void Start()
    {
        StartCoroutine(FruitSpawnerCoroutine());
    }

    private IEnumerator FruitSpawnerCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(timer);
            var randomPoint = RandomHelper.RandomPointWithinArea(area);

            var randomPointOffset = randomPoint;
            randomPointOffset.y += offset;
            if (Physics.Raycast(randomPointOffset, Vector3.down, 
                out var hit, offset * 2.0f))
            {
                Instantiate(fruits[Random.Range(0, fruits.Count)], 
                    hit.point, Quaternion.identity);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(area.center, area.size);
    }
}
