using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasImg : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Cube cube;
    void Start()
    {
        image.sprite = cube.Sprite;
    }
}
