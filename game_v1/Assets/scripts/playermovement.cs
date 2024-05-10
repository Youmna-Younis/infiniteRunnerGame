using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.SceneManagement;
public class playermovement : MonoBehaviour
{
    // Start is called before the first frame update
    //  void Start()
    //{

    //}
    bool alive = true;
    public float speed = 5;
    public Rigidbody rb;
    float horizontalinput;

    public float horizontalmultiplyr=1.5F;
    private void FixedUpdate()
    {
        if (!alive) return; //if player die not to move
        Vector3 forwordmove = transform.forward * speed * Time.fixedDeltaTime;

        Vector3 horizpntalmove = transform.right * horizontalinput * speed * Time.fixedDeltaTime* horizontalmultiplyr;
        rb.MovePosition(rb.position + forwordmove + horizpntalmove);

    }
    // Update is called once per frame
    void Update()
    {
        horizontalinput = Input.GetAxis("Horizontal");
        //player fall from platform
        if (transform.position.y < -5)
        {
            Die();
        }
    }
    public void Die()
    {
        alive = false;
        // double waitTimr = 2;
        //Invoke("Restart",waitTimr);
        Restart();
    } 
    void Restart()
    {   //restare game
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
