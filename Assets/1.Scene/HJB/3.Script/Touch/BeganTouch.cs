using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeganTouch : MonoBehaviour
{
    [SerializeField] float scaleSet = 0.01f;
    private void FixedUpdate()
    {
        transform.localScale += new Vector3(scaleSet, scaleSet, scaleSet);
        if (transform.localScale.x>4)
        {
            Destroy(gameObject);
        }
    }

    
}
