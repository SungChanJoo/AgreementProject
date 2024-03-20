using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VeneGameOver : MonoBehaviour
{
    [SerializeField] private GameObject GameOverUI; // 게임오버 UI
    [SerializeField] private GameObject[] PlayerOneBtn; // Player Win , Lose 버튼 => 승자만 보여줄것
    [SerializeField] private GameObject[] PlayerTwoBtn;

    [SerializeField] GameObject Pooling;

    [SerializeField] private TextMeshProUGUI PlayerOneText;
    [SerializeField] private TextMeshProUGUI PlayerTwoText;

    [SerializeField] private Animator PlayerOne_Anim;
    [SerializeField] private Animator PlayerTwo_Anim;

    private bool isGameover = false;

    private void Start()
    {
        PlayerOneText.text = "";
        PlayerTwoText.text = "";

        for (int i = 0; i < 2; i++)
        {
            PlayerOneBtn[i].SetActive(false);
            PlayerTwoBtn[i].SetActive(false);
        }
        GameOverUI.SetActive(false);
    }

    private void Update()
    {
        veneCoupleGameOver();
    }

    private void veneCoupleGameOver()
    {
        if(VeneziaManager.Instance.veneGameMode == VeneGameMode.Couple && !isGameover)
        {
            if (VeneziaManager.Instance.isGameover)
            {
                GameOverUI.SetActive(true);
                Pooling.SetActive(false);
                if (TimeSlider.Instance.slider.value <= 0)
                {
                    isGameover = true;
                    //Player 1 승
                    for (int i = 0; i < PlayerTwoBtn.Length; i++)
                    {
                        PlayerTwoBtn[i].SetActive(true);
                        PlayerOneText.text = "Lose";
                        PlayerTwoText.text = "Win";
                        PlayerTwo_Anim.SetBool("isWin", true);
                        PlayerOne_Anim.SetBool("isLose", true);
                    }
                }
                else if (TimeSlider.Instance.slider_PlayerTwo.value <= 0)
                {
                    //Player2  승
                    isGameover = true;
                    for (int i = 0; i < PlayerOneBtn.Length; i++)
                    {
                        PlayerOneBtn[i].SetActive(true);
                        PlayerOneText.text = "Win";
                        PlayerTwoText.text = "Lose";
                        PlayerTwo_Anim.SetBool("isLose", true);
                        PlayerOne_Anim.SetBool("isWin", true);
                    }
                }

            }
        }
    }
}
