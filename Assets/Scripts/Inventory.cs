using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }
    private HashSet<Vector2> collectedPositions;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            collectedPositions = new HashSet<Vector2>();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool IsPositionCollected(Vector2 position)
    {
        return collectedPositions.Contains(position);
    }

    public void AddPosition(Vector2 position)
    {
        collectedPositions.Add(position);
    }
}