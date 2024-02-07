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
        //카메라 플레이어 따라다니기
        CVcam = GameObject.FindGameObjectWithTag("CVcam").GetComponent<CinemachineVirtualCamera>();
        CVcam.Follow = transform;
        PlayerInputUI.SetActive(true);
    }

    private void Update()
    {
        //로컬 플레이어가 아니면 실행 안함
        if (!isLocalPlayer) return;
        #region Walk
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
            EmtiCanvas.transform.localRotation = Quaternion.Euler(0f, -90f, 0);
        }
        if (IsPress && buttonState == ButtonState.Left)
        {
            //Rigidbody에 힘을 적용하며, Vector2.right를 곱하여 X 축에만 영향을 줍
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

    //상호작용 가능한 UI show or hide 
    public void InteractableUI(GameObject UI, bool value)
    {
        if (!isLocalPlayer) return;

        UI.SetActive(value);
    }
    #region PlayerInput (ButtonEvnet)
    //오른쪽 버튼을 누르면 이동방향, 상태변경
    public void OnRightButtonDown()
    {
        IsPress = true;
        moveInput = Vector3.right.x;
        if (buttonState != ButtonState.Right)
        {
            buttonState = ButtonState.Right;
        }
    }
    //왼쪽 버튼을 누르면 이동방향, 상태변경
    public void OnLeftButtonDown()
    {
        IsPress = true;
        moveInput = Vector3.left.x;

        if (buttonState != ButtonState.Left)
        {
            buttonState = ButtonState.Left;
        }
    }
    //버튼을 땠을 때
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

    //이모티콘 재사용 대기시간
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
