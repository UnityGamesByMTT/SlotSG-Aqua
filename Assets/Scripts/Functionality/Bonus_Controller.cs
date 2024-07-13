using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class Bonus_Controller : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private Button[] chest;
    [SerializeField] private ImageAnimation[] chestAnim;
    [SerializeField] private TMP_Text[] reward_text;
    [SerializeField] private List<int> resultData;
    [SerializeField] private GameObject bonusObject;

    public int openCount;
    public bool isfinished = false;
    private bool opening = false;

    [SerializeField] private List<int> openIndex;
    [SerializeField] private AudioController audioController;
    [SerializeField] private SlotBehaviour slotBehaviour;

    [SerializeField] private GameObject WinPopUp;
    [SerializeField] private TMP_Text WinPopUpText;
    private double winAmount;
    private void Start()
    {
        for (int i = 0; i < chest.Length; i++)
        {
            int index = i;
            chest[i].onClick.RemoveAllListeners();
            chest[i].onClick.AddListener(delegate { OnChestOpen(index); });
        }
    }

    internal void StartBonusGame(List<string> result)
    {

        for (int i = 0; i < result.Count; i++)
        {
            resultData.Add(int.Parse(result[i]));
        }

        audioController.StopBgAudio();
        audioController.StopWLAaudio();
        audioController.playBgAudio("bonus");

        bonusObject.SetActive(true);

    }

    internal void FinishBonusGame()
    {
        opening = false;
        isfinished = false;
        resultData.Clear();
        openCount = 0;
        winAmount = 0;
        WinPopUpText.text = "";
        bonusObject.SetActive(false);
        WinPopUp.SetActive(false);

        audioController.playBgAudio("normal");
        foreach (Button item in chest)
        {
            item.interactable = true;
        }


    }


    void OnChestOpen(int index)
    {
        if (isfinished) return;
        if (opening) return;
        audioController.PlayButtonAudio();

        StartCoroutine(chestOpenRoutine(index));

    }

    IEnumerator chestOpenRoutine(int index)
    {
        audioController.PlaySpinBonusAudio("bonus");
        opening = true;
        openIndex.Add(index);
        chest[index].interactable = false;
        bool gameFinishied = false;
        chestAnim[index].transform.DOShakePosition(3f, new Vector3(15,0,0), 30, 90,true);
        yield return new WaitForSeconds(3f);
        audioController.StopApinBonusAudio();
        chestAnim[index].StartAnimation();

        if (resultData[openCount] > 0)
        {
            audioController.PlayWLAudio("bonuswin");
            reward_text[index].text = "+ " + (resultData[openCount]* slotBehaviour.GetCurrentbetperLine()).ToString("f2");
            winAmount += (resultData[openCount] * slotBehaviour.GetCurrentbetperLine());
        }
        else
        {
            audioController.PlayWLAudio("bonuslose");
            reward_text[index].text = "game Over";
            gameFinishied = true;
            
        }
        reward_text[index].color =Color.black;
        reward_text[index].transform.localScale = Vector3.zero;
        reward_text[index].gameObject.SetActive(true);
        reward_text[index].transform.DOScale(1, 0.8f);
        reward_text[index].transform.DOLocalMoveY(235, 0.8f);
        yield return new WaitForSeconds(0.8f);
        reward_text[index].gameObject.SetActive(false);
        reward_text[index].transform.localPosition = new Vector3(-50, -42);
        openCount++;
        opening = false;
        audioController.StopWLAaudio();

        if (gameFinishied) {

            WinPopUp.transform.localScale = Vector3.zero;
            WinPopUpText.text = winAmount.ToString();
            WinPopUp.SetActive(true);
            WinPopUp.transform.DOScale(Vector3.one, 0.8f);
            yield return new WaitForSeconds(1);
            isfinished = true;

        }



    }


}
