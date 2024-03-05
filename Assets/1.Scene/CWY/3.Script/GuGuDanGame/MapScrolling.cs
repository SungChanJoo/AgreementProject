using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapScrolling : MonoBehaviour
{
    public Transform target;
    public float speed;
    public Vector3 dir = Vector3.left;

    public RectTransform rectTransform;

    Vector3 SavePosition;
    private void Start()
    {
        SavePosition = new Vector3(target.transform.position.x, target.transform.position.y, target.transform.position.z);
    }
    private void Update()
    {
        transform.position += dir * speed * Time.deltaTime;
        if (rectTransform.transform.position.x < - 1920f)
        {
            //transform.position = SavePoint.transform.position;
            transform.position = SavePosition;
        }
    }
}
