using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum ButtonState
{ 
    None = 0,
    Right,
    Left
}

public class PlayerMovement : MonoBehaviour
{
    public bool IsPress = false;
    ButtonState buttonState = ButtonState.None;
    public float MoveSpeed = 0f;
    //��ư�� ������ ��
    public void OnRightButtonDown()
    {
        IsPress = true;
        if (buttonState != ButtonState.Right)
            buttonState = ButtonState.Right;
    }
    public void OnLeftButtonDown()
    {
        IsPress = true;
        if (buttonState != ButtonState.Left)
            buttonState = ButtonState.Left;
    }
    //��ư�� ���� ��
    public void OnPointerUp()
    {
        IsPress = false;
    }

    private void Update()
    {
        if (IsPress && buttonState == ButtonState.Right)
        {
            transform.position += Vector3.right * Time.deltaTime * MoveSpeed;
            transform.rotation = Quaternion.Euler(0f, -90f, 0f);
        }
        if (IsPress && buttonState == ButtonState.Left)
        {
            transform.position += Vector3.left * Time.deltaTime * MoveSpeed;
            transform.rotation = Quaternion.Euler(0f, 90f, 0f);

        }
    }

    
}
