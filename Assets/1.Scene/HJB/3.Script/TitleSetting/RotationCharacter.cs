using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationCharacter : MonoBehaviour
{
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
        rotation = new Vector3(RandFloat(), RandFloat(), RandFloat());
    }

    float RandFloat()
    {
        return Random.Range(0f, 1.01f);
    }

}
