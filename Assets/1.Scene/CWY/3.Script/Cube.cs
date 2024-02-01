using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour
{
    public float speed;
    private void Update()
    {
        Cube_Move();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ground"))
        {
            ObjectPooling.Instance.cubePool.Add(gameObject);
            gameObject.SetActive(false);
        }
    }

    private void Cube_Move()
    {
        float moveY = -1 * speed * Time.deltaTime;
        transform.Translate(0, moveY, 0);
    }

}
