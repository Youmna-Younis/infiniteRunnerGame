using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerAnimation : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("Jump")) {
            Debug.Log("jump"); 
            animator.SetInteger("New Int", 6);
            Invoke("sleep", 0.7f);
        }
    }
    void sleep()
    {
        animator.SetInteger("New Int", 0);
    }
}
