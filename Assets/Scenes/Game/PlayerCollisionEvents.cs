using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PerlinNoiseGenerator;

public class PlayerCollisionEvents : MonoBehaviour
{
    [SerializeField] private GameObject blockGenerator;
    private bool isCloseToCave = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PortalCave") && !isCloseToCave)
        {
            Debug.Log("Player is close to cave");
            isCloseToCave = true;
            blockGenerator.GetComponent<DynamicBlockWorldGenerator>().SetIsCloseToCave(isCloseToCave);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PortalCave") && isCloseToCave)
        {
            Debug.Log("Player is out of cave");
            isCloseToCave = false;
            blockGenerator.GetComponent<DynamicBlockWorldGenerator>().SetIsCloseToCave(isCloseToCave);
        }
    }
}