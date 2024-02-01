using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ground"))
        {
            ObjectPooling.Instance.cubePool.Add(gameObject);
            gameObject.SetActive(false);
        }
    }



}
