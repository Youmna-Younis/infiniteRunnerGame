using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstcle : MonoBehaviour
{
    playermovement playerMovementobj;
    // Start is called before the first frame update
    void Start()
    {
        playerMovementobj = GameObject.FindObjectOfType<playermovement>();
    }
    private void OnCollisionEnter(Collision collision)
    { 
        //check obj get collied with obsctle is player
        if(collision.gameObject.name== "Running")
        {  //kill player
            playerMovementobj.Die();
        }
      
    }
        // Update is called once per frame
        void Update()
    {
        
    }
}
