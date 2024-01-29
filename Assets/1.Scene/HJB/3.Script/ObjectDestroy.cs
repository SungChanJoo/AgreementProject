using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDestroy : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DestoryObj_Co());
    }

    private IEnumerator DestoryObj_Co()
    {
        yield return new WaitForSeconds(3f);
        Destroy(gameObject);
    }
}
