using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingTimeText : MonoBehaviour
{
    [SerializeField] private Slider _timeSlider;
    private Text time;

    private void Awake()
    {
        time = GetComponent<Text>();
    }
    public void ValueToText()
    {
        time.text = $"{(int)_timeSlider.value}";
    }
}
