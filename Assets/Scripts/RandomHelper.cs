using UnityEngine;

public class RandomHelper
{
    public static Vector3 RandomPointWithinArea(Collider area)
    {
        var areaBounds = area.bounds;
        var areaBoundsMin = areaBounds.min;
        var areaBoundsMax = areaBounds.max;
        return new Vector3(
            Random.Range(areaBoundsMin.x, areaBoundsMax.x), 
            Random.Range(areaBoundsMin.y, areaBoundsMax.y), 
            Random.Range(areaBoundsMin.z, areaBoundsMax.z));
    }
}