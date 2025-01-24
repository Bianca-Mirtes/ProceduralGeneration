using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryBlock : MonoBehaviour
{
    [SerializeField] private GameObject eIndicatorPrefab;
    private GameObject eIndicator;
    private bool isPlayerNearby = false;

    void Start()
    {
        eIndicator = Instantiate(eIndicatorPrefab, transform.position + Vector3.up * 2, Quaternion.identity, transform);
        eIndicator.SetActive(false);
    }

    void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            CollectBlock();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            eIndicator.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            eIndicator.SetActive(false);
        }
    }

    void CollectBlock()
    {
        Vector2 blockPosition = new Vector2(transform.position.x, transform.position.y);
        if (!Inventory.Instance.IsPositionCollected(blockPosition))
        {
            Inventory.Instance.AddPosition(blockPosition);
            Destroy(gameObject);
        }
    }
}