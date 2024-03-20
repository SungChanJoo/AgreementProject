using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VeneGameOver : MonoBehaviour
{
    [SerializeField] private GameObject GameOverUI; // °ÔÀÓ¿À¹ö UI
    [SerializeField] private GameObject[] LobbyBot; // 1 bot 2 / 2 bot 2
    [SerializeField] private GameObject[] PlayerOneBtn; // Player Win , Lose ¹öÆ° => ½ÂÀÚ¸¸ º¸¿©ÁÙ°Í
    [SerializeField] private GameObject[] PlayerTwoBtn;

    [SerializeField] GameObject Pooling;

    [SerializeField] private TextMeshProUGUI PlayerOneText;
    [SerializeField] private TextMeshProUGUI PlayerTwoText;

    [Header("PlayerOneAnimation")]
    [SerializeField] private Animator PlayerOne_LoseAnim;
    [SerializeField] private Animator PlayerOne_WinAnim;
    [Header("PlayerTwoAnimation")]
    [SerializeField] private Animator PlayerTwo_Lose_Anim;
    [SerializeField] private Animator PlayerTwo_Win_Anim;

    private bool isGameover = false;

    private void Start()
    {
        PlayerOneText.text = "";
        PlayerTwoText.text = "";

        for (int i = 0; i < LobbyBot.Length; i++)
        {
            LobbyBot[i].SetActive(false);
        }

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
        if(VeneziaManager.Instance.play_mode == PlayMode.Couple && !isGameover)
        {
            if (VeneziaManager.Instance.isGameover)
            {
                GameOverUI.SetActive(true);
                Pooling.SetActive(false);
                if (TimeSlider.Instance.slider.value <= 0)
                {
                    isGameover = true;
                    //Player 2 ½Â
                    for (int i = 0; i < PlayerTwoBtn.Length; i++)
                    {
                        PlayerTwoBtn[i].SetActive(true);
                        PlayerOneText.text = "Lose";
                        PlayerTwoText.text = "Win";
                        // 1 2 > win lose / 3 4 win lose
                        LobbyBot[1].SetActive(true);
                        LobbyBot[2].SetActive(true);
                        PlayerTwo_Win_Anim.SetBool("isWin", true);
                        PlayerOne_LoseAnim.SetBool("isLose", true);
                    }
                }
                else if (TimeSlider.Instance.slider_PlayerTwo.value <= 0)
                {
                    //Player1  ½Â
                    isGameover = true;
                    for (int i = 0; i < PlayerOneBtn.Length; i++)
                    {
                        PlayerOneBtn[i].SetActive(true);
                        LobbyBot[0].SetActive(true);
                        LobbyBot[3].SetActive(true);
                        PlayerOneText.text = "Win";
                        PlayerTwoText.text = "Lose";
                        PlayerTwo_Lose_Anim.SetBool("isLose", true);
                        PlayerOne_WinAnim.SetBool("isWin", true);
                    }
                }

            }
        }
    }
}
