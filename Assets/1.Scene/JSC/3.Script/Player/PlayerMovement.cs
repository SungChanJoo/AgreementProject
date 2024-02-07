using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Cinemachine;

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
    ButtonState buttonState = ButtonState.None;

    [Header("Emoticion")]
    public GameObject EmtiCanvas;
    public GameObject[] EmtiArray;
    public float TimebetUsingEmti = 3f;
    public bool IsUseEmti = false;
    [Header("ETC")]
    public bool IsPress = false;
    public GameObject PlayerInputUI;
    private Rigidbody _rb;
    private CinemachineVirtualCamera CVcam;
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        if (PlayerInputUI.activeSelf)
            PlayerInputUI.SetActive(false);

    }
    public override void OnStartLocalPlayer()
    {
        //ī�޶� �÷��̾� ����ٴϱ�
        CVcam = GameObject.FindGameObjectWithTag("CVcam").GetComponent<CinemachineVirtualCamera>();
        CVcam.Follow = transform;
        PlayerInputUI.SetActive(true);
    }

    private void Update()
    {
        //���� �÷��̾ �ƴϸ� ���� ����
        if (!isLocalPlayer) return;
        #region Walk
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
            EmtiCanvas.transform.localRotation = Quaternion.Euler(0f, -90f, 0);
        }
        if (IsPress && buttonState == ButtonState.Left)
        {
            //Rigidbody�� ���� �����ϸ�, Vector2.right�� ���Ͽ� X �࿡�� ������ ��
            _rb.AddForce(movement * Vector3.right);
            transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            EmtiCanvas.transform.localRotation = Quaternion.Euler(0f, 90f, 0);
        }
        if (!IsPress)
        {
            _rb.velocity = Vector3.zero;
        } 
        #endregion
    }

    //��ȣ�ۿ� ������ UI show or hide 
    public void InteractableUI(GameObject UI, bool value)
    {
        if (!isLocalPlayer) return;

        UI.SetActive(value);
    }
    #region PlayerInput (ButtonEvnet)
    //������ ��ư�� ������ �̵�����, ���º���
    public void OnRightButtonDown()
    {
        IsPress = true;
        moveInput = Vector3.right.x;
        if (buttonState != ButtonState.Right)
        {
            buttonState = ButtonState.Right;
        }
    }
    //���� ��ư�� ������ �̵�����, ���º���
    public void OnLeftButtonDown()
    {
        IsPress = true;
        moveInput = Vector3.left.x;

        if (buttonState != ButtonState.Left)
        {
            buttonState = ButtonState.Left;
        }
    }
    //��ư�� ���� ��
    public void OnPointerUp()
    {
        IsPress = false;
    }
    #endregion

    #region Emoticon
    public void UseEmti(int index)
    {
        if (!IsUseEmti)
            CmdUseEmti(index);
    }

    [Command]
    public void CmdUseEmti(int index)
    {
        RpcUseEmti(index);
    }
    [ClientRpc]
    public void RpcUseEmti(int index)
    {
        StartCoroutine(DelayEmit_co(index));
    }

    //�̸�Ƽ�� ���� ���ð�
    IEnumerator DelayEmit_co(int index)
    {
        IsUseEmti = true;
        EmtiArray[index].SetActive(true);
        yield return new WaitForSeconds(TimebetUsingEmti);
        EmtiArray[index].SetActive(false);
        IsUseEmti = false;
    } 
    #endregion
}
