using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    private HashSet<Vector2> collectedPositions;

    private static Inventory _instance;
    public static Inventory Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<Inventory>();

                if (_instance == null)
                {
                    GameObject singleton = new GameObject("Inventory");
                    _instance = singleton.AddComponent<Inventory>();
                    DontDestroyOnLoad(singleton);
                }
            }

            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        collectedPositions = new HashSet<Vector2>();
    }

    private void Update()
    {
        Debug.Log("Inventário: " + collectedPositions.Count);
    }

    public int getBlocks()
    {
        return collectedPositions.Count;
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