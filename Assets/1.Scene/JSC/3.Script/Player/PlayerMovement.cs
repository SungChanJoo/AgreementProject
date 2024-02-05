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
    //��ư�� ������ ��
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
    //��ư�� ���� ��
    public void OnPointerUp()
    {
        IsPress = false;
    }

    private void Update()
    {
        //���� �÷��̾ �ƴϸ� ���� ����
        if (!isLocalPlayer) return;

        //�̵��ϰ��� �ϴ� ����� ��ǥ �ӵ��� ���
        float targetSpeed = moveInput * MoveSpeed;
        //���� �ӵ��� ��ǥ �ӵ� ������ ���̸� ���
        float speedDif = targetSpeed - _rb.velocity.x;
        //��Ȳ�� ���� ���ӵ��� ����
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : decceleration;
        //���ӵ��� �ӵ� ���̿� �����ϰ�, Ư���� ������ �����Ͽ� ���ӵ��� ����
        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);

        if (IsPress && buttonState == ButtonState.Right)
        {
            //Rigidbody�� ���� �����ϸ�, Vector2.right�� ���Ͽ� X �࿡�� ������ ��
            _rb.AddForce(movement * Vector3.right);
            transform.rotation = Quaternion.Euler(0f, -90f, 0f);
        }
        if (IsPress && buttonState == ButtonState.Left)
        {
            //Rigidbody�� ���� �����ϸ�, Vector2.right�� ���Ͽ� X �࿡�� ������ ��
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
