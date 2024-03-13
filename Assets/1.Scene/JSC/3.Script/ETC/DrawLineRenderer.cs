using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawLineRenderer : MonoBehaviour
{
    [SerializeField] private List<GameObject> StepPos;
    [SerializeField] private float Depth = 6f;
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
        lr.positionCount = StepPos.Count;
        lr.loop = false;
        for (int i = 0; i < StepPos.Count; i++)
        {
            var position = StepPos[i].transform.position + new Vector3(0, 0, Depth);
            Debug.Log(StepPos[i].transform.position +" , " +position);
            lr.SetPosition(i, position);
        }
    }
}
