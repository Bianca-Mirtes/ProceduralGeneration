using UnityEngine;

public class PrefabGenerator : MonoBehaviour
{
    public GameObject prefab; // Reference to the prefab to be generated
    public float scale = 1f; // Scale of the terrain (affects noise scale)
    public float threshold = 0.5f; // Threshold to determine prefab placement
    public float prefabSpacing = 2f; // Spacing between prefabs

    public void GeneratePrefabs(float perlinValue, int x, int z)
    {
        // Place a prefab if the Perlin value exceeds the threshold
        if (perlinValue > threshold)
        {
            Vector3 position = new Vector3(x * prefabSpacing, 0, z * prefabSpacing);
            Instantiate(prefab, position, Quaternion.identity); // Instantiate the prefab at the generated position
        }
    }
}

