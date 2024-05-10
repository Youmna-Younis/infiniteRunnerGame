using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinCollector : MonoBehaviour
{
    int coinsCounter = 0;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("coin"))
        {
            Debug.Log("coin");
            Destroy(other.gameObject);
            coinsCounter++;
        }
    }
}