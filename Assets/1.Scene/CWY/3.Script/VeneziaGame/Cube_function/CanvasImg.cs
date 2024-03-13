using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasImg : MonoBehaviour
{
    [SerializeField] Image image;
    [SerializeField] Cube cube;
    void Start()
    {
        image.sprite = cube.sprite;
    }
}
