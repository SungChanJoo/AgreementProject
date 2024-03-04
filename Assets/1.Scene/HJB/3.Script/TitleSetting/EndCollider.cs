using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndCollider : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        other.gameObject.SetActive(false);
        StartCoroutine(ActiveTrue_co(other.gameObject));
    }

    private IEnumerator ActiveTrue_co(GameObject other)
    {
        yield return new WaitForSeconds(1f);
        other.SetActive(true);
    }
}
