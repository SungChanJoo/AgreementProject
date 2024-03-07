using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationCharacter : MonoBehaviour
{
    [SerializeField] private bool hold_Y = false;
    [SerializeField] private float SetRotation;
    Vector3 rotation;
    

    void Start()
    {
        RandomRotaion();
        
    }
    private void FixedUpdate()
    {
        transform.Rotate(rotation, 1f);
    }
    private void RandomRotaion()
    {
        if (hold_Y)
        {
            rotation = new Vector3(0, SetRotation, 0);
        }
        else
        {
            rotation = new Vector3(RandFloat(), RandFloat(), RandFloat());
        }
    }

    float RandFloat()
    {
        return Random.Range(0f, 1.01f);
    }

}
