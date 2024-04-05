using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleCharacter : MonoBehaviour
{    
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {        
        animator.SetFloat("Speed", 1f);
    }
}
