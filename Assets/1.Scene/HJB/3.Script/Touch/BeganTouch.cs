using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Coffee.UIExtensions;

public class BeganTouch : MonoBehaviour
{
    [SerializeField] private float timeSet = 1f;
    private float currentTime;    
    private void FixedUpdate()
    {
        currentTime += Time.fixedDeltaTime;
        if (currentTime>timeSet)
        {
            Destroy(gameObject);
        }        
    }

    
}
