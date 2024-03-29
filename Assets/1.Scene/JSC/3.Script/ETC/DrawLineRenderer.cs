using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawLineRenderer : MonoBehaviour
{
    [SerializeField] private List<GameObject> stepPos;
    [SerializeField] private float depth = 6f;
    private LineRenderer lr;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
    }
    private void Start()
    {
        DrawLine();
    }
    public void DrawLine()
    {
        if (lr.enabled == false)
            lr.enabled = true;
        lr.positionCount = stepPos.Count;
        lr.loop = false;
        for (int i = 0; i < stepPos.Count; i++)
        {
            var position = stepPos[i].transform.position + new Vector3(0, 0, depth);
            lr.SetPosition(i, position);
        }
    }
}
