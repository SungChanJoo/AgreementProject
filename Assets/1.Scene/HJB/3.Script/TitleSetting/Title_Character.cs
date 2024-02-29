using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Title_Character : MonoBehaviour
{    
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        animator.SetTrigger("Run");
        animator.SetFloat("Speed", 1f);
    }
}
