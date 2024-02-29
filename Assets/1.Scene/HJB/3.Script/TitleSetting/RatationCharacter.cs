using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RatationCharacter : MonoBehaviour
{
    Vector3 rotation;
    // Start is called before the first frame update
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
