using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundTiles : MonoBehaviour
{
    GroundSpawner groundSpawner;
   
    // Start is called before the first frame update
    void Start()
    {
        groundSpawner = GameObject.FindObjectOfType<GroundSpawner>();
        spawnObstacle();
    }
    private void OnTriggerExit(Collider other)
    {
        groundSpawner.SpawnTile(true);
        Destroy(gameObject, 2);
    }

    // Update is called once per frames
    void Update()
    {
        
    }
    public GameObject obstacalePrefab;

    void spawnObstacle()
    {
        //choose random points

        //spwan obstcle at position
        int obstcleSpwanIdx = Random.Range(2, 5); //[2,5[
        Transform spwanPoint = transform.GetChild(obstcleSpwanIdx).transform;
        // spwan obstcle
        Instantiate(obstacalePrefab, spwanPoint.position, Quaternion.identity, transform);
        //                             obstcle point ,   No Rotation 
    }
}
