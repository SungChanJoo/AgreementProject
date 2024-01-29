using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerController : MonoBehaviour
{
    public float MoveSpeed = 7f;
    public float hori;
    public float verti;
    public Rigidbody rigidbody;
    public Capsulation capsulation;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        hori = 0;
        verti = 0;
        

    }
    private void Start()
    {
        
        
    }
    public abstract void ControllKey();
}
