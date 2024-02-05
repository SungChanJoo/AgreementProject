using Mirror;
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

public class PlayerMovement : NetworkBehaviour
{
    [Header("PlayerMovement")]
    public float MoveSpeed = 0f;
    public float acceleration = 10f;
    public float decceleration = 1f;
    public float velPower = 1f;

    float moveInput;

    public bool IsPress = false;
    ButtonState buttonState = ButtonState.None;

    public GameObject PlayerInputUI;
    Rigidbody _rb;
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }
    public override void OnStartLocalPlayer()
    {
        PlayerInputUI.SetActive(true);
    }
    //버튼을 눌렀을 때
    public void OnRightButtonDown()
    {
        IsPress = true;
        moveInput = Vector3.right.x;
        if (buttonState != ButtonState.Right)
            buttonState = ButtonState.Right;
    }
    public void OnLeftButtonDown()
    {
        IsPress = true;
        moveInput = Vector3.left.x;

        if (buttonState != ButtonState.Left)
            buttonState = ButtonState.Left;
    }
    //버튼을 땠을 때
    public void OnPointerUp()
    {
        IsPress = false;
    }

    private void Update()
    {
        //로컬 플레이어가 아니면 실행 안함
        if (!isLocalPlayer) return;

        //이동하고자 하는 방향과 목표 속도를 계산
        float targetSpeed = moveInput * MoveSpeed;
        //현재 속도와 목표 속도 사이의 차이를 계산
        float speedDif = targetSpeed - _rb.velocity.x;
        //상황에 따라 가속도를 변경
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : decceleration;
        //가속도를 속도 차이에 적용하고, 특정한 지수로 제곱하여 가속도를 조절
        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);

        if (IsPress && buttonState == ButtonState.Right)
        {
            //Rigidbody에 힘을 적용하며, Vector2.right를 곱하여 X 축에만 영향을 줍
            _rb.AddForce(movement * Vector3.right);
            transform.rotation = Quaternion.Euler(0f, -90f, 0f);
        }
        if (IsPress && buttonState == ButtonState.Left)
        {
            //Rigidbody에 힘을 적용하며, Vector2.right를 곱하여 X 축에만 영향을 줍
            _rb.AddForce(movement * Vector3.right);
            transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        }
        if(!IsPress)
        {
            _rb.velocity = Vector3.zero;
        }
    }
    public void InteractableUI(GameObject UI, bool value)
    {
        if (!isLocalPlayer) return;

        UI.SetActive(value);
    }
}
