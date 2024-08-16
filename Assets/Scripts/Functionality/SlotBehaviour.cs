using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;

public class SlotBehaviour : MonoBehaviour
{
    [SerializeField]
    private RectTransform mainContainer_RT;

    [Header("Sprites")]
    [SerializeField]
    private Sprite[] myImages;  //images taken initially

    [Header("Slot Images")]
    [SerializeField]
    private List<SlotImage> images;     //class to store total images
    [SerializeField]
    private List<SlotImage> Tempimages;     //class to store the result matrix

    [Header("Slots Objects")]
    [SerializeField]
    private GameObject[] Slot_Objects;
    [Header("Slots Elements")]
    [SerializeField]
    private LayoutElement[] Slot_Elements;

    [Header("Slots Transforms")]
    [SerializeField]
    private Transform[] Slot_Transform;

    [Header("Line Button Objects")]
    [SerializeField]
    private List<GameObject> StaticLine_Objects;

    [Header("Line Button Texts")]
    [SerializeField]
    private List<TMP_Text> StaticLine_Texts;

    [Header("Line Button Objects")]
    [SerializeField]
    private List<ManageLineButtons> StaticLine_Scripts;

    [Header("Line Button Objects")]
    [SerializeField]
    private List<Button> StaticLine_Buttons;


    private Dictionary<int, string> x_string = new Dictionary<int, string>();
    private Dictionary<int, string> y_string = new Dictionary<int, string>();

    [Header("Buttons")]
    [SerializeField]
    private Button SlotStart_Button;
    [SerializeField]
    private Button AutoSpin_Button;
    [SerializeField]
    private Button AutoSpinStop_Button;
    [SerializeField]
    private Button MaxBet_Button;
    [SerializeField]
    private Button BetPlus_Button;
    [SerializeField]
    private Button BetMinus_Button;
    [SerializeField]
    private Button LinePlus_Button;
    [SerializeField]
    private Button LineMinus_Button;

    [Header("Animated Sprites")]
    [SerializeField]
    private Sprite[] Nine_Sprite;
    [SerializeField]
    private Sprite[] Ten_Sprite;
    [SerializeField]
    private Sprite[] J_Sprite;
    [SerializeField]
    private Sprite[] K_Sprite;
    [SerializeField]
    private Sprite[] Q_Sprite;
    [SerializeField]
    private Sprite[] A_Sprite;
    [SerializeField]
    private Sprite[] Hedgehog_Sprite;
    [SerializeField]
    private Sprite[] Crab_Sprite;
    [SerializeField]
    private Sprite[] JellyFish_Sprite;
    [SerializeField]
    private Sprite[] Turtle_Sprite;
    [SerializeField]
    private Sprite[] Shell_Sprite;
    [SerializeField]
    private Sprite[] Octopus_Sprite;
    [SerializeField]
    private Sprite[] Bonus_Sprite;
    [SerializeField]
    private Sprite[] Wild_Sprite;
    [SerializeField]
    private Sprite[] Scatter_Sprite;
    [SerializeField]
    private Sprite[] FreeSpin_Sprite;
    [SerializeField]
    private Sprite[] Jackpot_Sprite;

    [Header("Miscellaneous UI")]
    [SerializeField]
    private TMP_Text Balance_text;
    [SerializeField]
    private TMP_Text TotalBet_text;
    [SerializeField]
    private TMP_Text Lines_text;
    [SerializeField]
    private TMP_Text TotalWin_text;
    [SerializeField]
    private TMP_Text BetPerLine_text;

    [SerializeField] private int maxReelItemCount = 18;

    [Header("Audio Management")]
    [SerializeField] private AudioController audioController;


    int tweenHeight = 0;  //calculate the height at which tweening is done

    [SerializeField]
    private GameObject Image_Prefab;    //icons prefab

    [SerializeField]
    private PayoutCalculation PayCalculator;

    private Tweener WinTween = null;
    private List<Tweener> alltweens = new List<Tweener>();


    [SerializeField]
    private List<ImageAnimation> TempList;//stores the sprites whose animation is running at present
                                          //
    [Header("parameters")]
    [SerializeField] private int IconSizeFactor = 100;       //set this parameter according to the size of the icon and spacing
    private int numberOfSlots = 5;          //number of columns
    [SerializeField] private int SpacingFactor = 0;
    [SerializeField] private int verticalVisibility = 3;

    [Header("scripts")]
    [SerializeField] private SocketIOManager SocketManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private Bonus_Controller bonus_Controller;


    Coroutine AutoSpinRoutine = null;
    Coroutine tweenroutine;
    Coroutine FreeSpinRoutine = null;
    bool IsAutoSpin = false;
    [SerializeField] bool IsSpinning = false;
    internal bool IsHoldSpin = false;
    private int BetCounter = 0;
    private int LineCounter = 0;
    internal int linecounter = 20;
    internal bool CheckPopups = false;
    private double bet = 0;
    private double balance = 0;
    private bool IsFreeSpin = false;
    private double currentBalance = 0;
    private double currentTotalBet = 0;

    private int FreeSpins = 0;

    private void Start()
    {
        IsAutoSpin = false;
        if (Lines_text != null)
        {
            Lines_text.text = "20";
        }

        if (BetPlus_Button) BetPlus_Button.onClick.RemoveAllListeners();
        if (BetPlus_Button) BetPlus_Button.onClick.AddListener(delegate { ChangeBet(true); });
        if (BetMinus_Button) BetMinus_Button.onClick.RemoveAllListeners();
        if (BetMinus_Button) BetMinus_Button.onClick.AddListener(delegate { ChangeBet(false); });

        if (LinePlus_Button) LinePlus_Button.onClick.RemoveAllListeners();
        if (LinePlus_Button) LinePlus_Button.onClick.AddListener(delegate { ChangeLine(true); });
        if (LineMinus_Button) LineMinus_Button.onClick.RemoveAllListeners();
        if (LineMinus_Button) LineMinus_Button.onClick.AddListener(delegate { ChangeLine(false); });

        if (MaxBet_Button) MaxBet_Button.onClick.RemoveAllListeners();
        if (MaxBet_Button) MaxBet_Button.onClick.AddListener(MaxBet);

        if (SlotStart_Button)
        {
            SlotStart_Button.onClick.RemoveAllListeners();
            SlotStart_Button.onClick.AddListener(StartSpin);
        }

        if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.RemoveAllListeners();
        if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.AddListener(StopAutoSpin);

        tweenHeight = (myImages.Length * IconSizeFactor) - 280;
    }

    internal void AutoSpin()
    {
        if (!IsAutoSpin)
        {

            IsAutoSpin = true;
            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(true);
            //if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(false);

            if (AutoSpinRoutine != null)
            {
                StopCoroutine(AutoSpinRoutine);
                AutoSpinRoutine = null;
            }
            AutoSpinRoutine = StartCoroutine(AutoSpinCoroutine());

        }
    }

    internal void FreeSpin(int spins)
    {
        if (!IsFreeSpin)
        {

            IsFreeSpin = true;
            ToggleButtonGrp(false);

            if (FreeSpinRoutine != null)
            {
                StopCoroutine(FreeSpinRoutine);
                FreeSpinRoutine = null;
            }
            FreeSpinRoutine = StartCoroutine(FreeSpinCoroutine(spins));

        }
    }

    private IEnumerator FreeSpinCoroutine(int spinchances)
    {
        int i = 0;
        while (i < FreeSpins)
        {
            i++;
            uiManager.updateFreeSPinData(1 - ((float)i / (float)FreeSpins), FreeSpins - i);
            yield return new WaitForSeconds(0.2f);
            StartSlots(IsAutoSpin);
            yield return tweenroutine;
        }
        FreeSpins = 0;
        ToggleButtonGrp(true);
        IsFreeSpin = false;
    }

    private void StopAutoSpin()
    {
        if (IsAutoSpin)
        {
            IsAutoSpin = false;
            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(false);
            //if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(true);
            StartCoroutine(StopAutoSpinCoroutine());
        }

    }

    private IEnumerator AutoSpinCoroutine()
    {
        while (IsAutoSpin)
        {
            StartSlots(IsAutoSpin);
            yield return tweenroutine;
            yield return new WaitForSeconds(2f);


        }
    }

    private IEnumerator StopAutoSpinCoroutine()
    {
        yield return new WaitUntil(() => !IsSpinning);
        ToggleButtonGrp(true);
        if (AutoSpinRoutine != null || tweenroutine != null)
        {
            StopCoroutine(AutoSpinRoutine);
            StopCoroutine(tweenroutine);
            tweenroutine = null;
            AutoSpinRoutine = null;
            StopCoroutine(StopAutoSpinCoroutine());
        }
    }

    internal void StartSpinRoutine()
    {
        IsHoldSpin = false;
        Invoke("AutoSpinHold", 2f);
    }

    internal void StopSpinRoutine()
    {
        CancelInvoke("AutoSpinHold");
        if (IsAutoSpin)
        {
            IsAutoSpin = false;
            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(false);
            //if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(true);
            StartCoroutine(StopAutoSpinCoroutine());
        }
    }

    private void AutoSpinHold()
    {
        Debug.Log("Auto Spin Started");
        IsHoldSpin = true;
        AutoSpin();
    }



    private void StartSpin()
    {
        if (audioController) audioController.PlayButtonAudio("spin");

        StartSlots();
    }


    internal void FetchLines(string LineVal, int count)
    {
        y_string.Add(count + 1, LineVal);
        StaticLine_Texts[count].text = (count + 1).ToString();
        StaticLine_Objects[count].SetActive(true);
    }

    //Generate Static Lines from button hovers
    internal void GenerateStaticLine(TMP_Text LineID_Text)
    {

        int LineID = 1;
        try
        {
            LineID = int.Parse(LineID_Text.text);
        }
        catch (Exception e)
        {
            Debug.Log("Exception while parsing " + e.Message);
        }
        print("Line ID" + LineID);
        //List<int> x_points = null;
        List<int> y_points = null;
        y_points = y_string[LineID]?.Split(',')?.Select(Int32.Parse)?.ToList();
        //PayCalculator.GeneratePayoutLinesBackend(x_points, y_points, x_points.Count, true);
        PayCalculator.GeneratePayoutLinesBackend(y_points, y_points.Count, true);
    }

    //Destroy Static Lines from button hovers
    internal void DestroyStaticLine()
    {
        PayCalculator.ResetStaticLine();
    }

    internal double GetCurrentbetperLine()
    {

        return SocketManager.initialData.Bets[BetCounter];
    }

    private void MaxBet()
    {
        if (audioController) audioController.PlayButtonAudio();
        BetCounter = SocketManager.initialData.Bets.Count - 1;
        if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.Bets[BetCounter] * SocketManager.initialData.Lines.Count).ToString();
        if (BetPerLine_text) BetPerLine_text.text = SocketManager.initialData.Bets[BetCounter].ToString();
        currentTotalBet = SocketManager.initialData.Bets[BetCounter] * SocketManager.initialData.Lines.Count;
        CompareBalance();

    }

    internal void ChangeLine(bool IncDec)
    {

        if (audioController)
            audioController.PlayButtonAudio();



        PayCalculator.ResetLines();
        if (IncDec)
        {
            linecounter++;
        }
        else
        {
            linecounter--;
        }

        if (linecounter < 1)
        {
            linecounter = 1;

        }
        if (linecounter > 20)
        {
            linecounter = 20;
        }


        foreach (Button sb in StaticLine_Buttons)
        {
            sb.interactable = false;
        }

        foreach (ManageLineButtons sb in StaticLine_Scripts)
        {
            sb.isActive = false;
        }

        for (int i = 1; i <= linecounter; i++)
        {
            Debug.Log("run this code" + linecounter);
            Lines_text.text = i.ToString();
            StaticLine_Buttons[i - 1].interactable = true;
            StaticLine_Scripts[i - 1].isActive = true;
            GenerateStaticLine(Lines_text);
        }
    }

    void OnBetOne(bool IncDec)
    {
        if (audioController) audioController.PlayButtonAudio();

        if (BetCounter < SocketManager.initialData.Bets.Count - 1)
        {
            BetCounter++;
        }
        else
        {
            BetCounter = 0;
        }
        Debug.Log("Index:" + BetCounter);

        if (TotalBet_text) TotalBet_text.text = SocketManager.initialData.Bets[BetCounter].ToString();
        if (BetPerLine_text) BetPerLine_text.text = SocketManager.initialData.Bets[BetCounter].ToString();

    }

    private void ChangeBet(bool IncDec)
    {
        if (audioController) audioController.PlayButtonAudio();

        if (IncDec)
        {
            if (BetCounter < SocketManager.initialData.Bets.Count - 1)
            {
                BetCounter++;
            }
        }
        else
        {
            if (BetCounter > 0)
            {
                BetCounter--;
            }
        }
        if (BetPerLine_text) BetPerLine_text.text = SocketManager.initialData.Bets[BetCounter].ToString();
        if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.Bets[BetCounter] * SocketManager.initialData.Lines.Count).ToString();
        currentTotalBet = SocketManager.initialData.Bets[BetCounter] * SocketManager.initialData.Lines.Count;
        CompareBalance();


    }


    //just for testing purposes delete on production
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && SlotStart_Button.interactable)
        {
            StartSlots();
        }
    }

    //populate the slots with the values recieved from backend
    //internal void PopulateInitalSlots(int number, List<int> myvalues)
    //{
    //    PopulateSlot(myvalues, number);
    //}

    internal void SetInitialUI()
    {
        BetCounter = SocketManager.initialData.Bets.Count - 1;
        LineCounter = SocketManager.initialData.LinesCount.Count - 1;
        if (TotalBet_text) TotalBet_text.text = ((SocketManager.initialData.Bets[BetCounter] * SocketManager.initialData.Lines.Count)).ToString();
        if (BetPerLine_text) BetPerLine_text.text = SocketManager.initialData.Bets[BetCounter].ToString();
        if (Lines_text) Lines_text.text = SocketManager.initialData.LinesCount[LineCounter].ToString();
        if (TotalWin_text) TotalWin_text.text = SocketManager.playerdata.currentWining.ToString("f2");
        if (Balance_text) Balance_text.text = SocketManager.playerdata.Balance.ToString("f2");
        uiManager.InitialiseUIData(SocketManager.initUIData.AbtLogo.link, SocketManager.initUIData.AbtLogo.logoSprite, SocketManager.initUIData.ToULink, SocketManager.initUIData.PopLink, SocketManager.initUIData.paylines, SocketManager.initUIData.spclSymbolTxt);
        currentBalance = SocketManager.playerdata.Balance;
        currentTotalBet = SocketManager.initialData.Bets[BetCounter] * SocketManager.initialData.Lines.Count;
        CompareBalance();
    }
    //reset the layout after populating the slots
    internal void LayoutReset(int number)
    {
        if (Slot_Elements[number]) Slot_Elements[number].ignoreLayout = true;
        if (SlotStart_Button) SlotStart_Button.interactable = true;
    }

    //private void PopulateSlot(List<int> values, int number)
    //{
    //    if (Slot_Objects[number]) Slot_Objects[number].SetActive(true);

    //    for (int i = 0; i < values.Count; i++)
    //    {
    //        GameObject myImg = Instantiate(Image_Prefab, Slot_Transform[number]);
    //        images[number].slotImages.Add(myImg.GetComponent<Image>());
    //        images[number].slotImages[i].sprite = myImages[values[i]];
    //        PopulateAnimationSprites(images[number].slotImages[i].gameObject.GetComponent<ImageAnimation>(), values[i]);
    //    }
    //    for (int k = 0; k < 2; k++)
    //    {
    //        GameObject mylastImg = Instantiate(Image_Prefab, Slot_Transform[number]);
    //        images[number].slotImages.Add(mylastImg.GetComponent<Image>());
    //        images[number].slotImages[images[number].slotImages.Count - 1].sprite = myImages[values[k]];
    //        PopulateAnimationSprites(images[number].slotImages[images[number].slotImages.Count - 1].gameObject.GetComponent<ImageAnimation>(), values[k]);
    //    }
    //    if (mainContainer_RT) LayoutRebuilder.ForceRebuildLayoutImmediate(mainContainer_RT);
    //    tweenHeight = (values.Count * IconSizeFactor) - 280;
    //    GenerateMatrix(number);
    //}

    //function to populate animation sprites accordingly
    private void PopulateAnimationSprites(ImageAnimation animScript, int val)
    {
        animScript.textureArray.Clear();
        animScript.textureArray.TrimExcess();
        switch (val)
        {
            case 0:
                for (int i = 0; i < Nine_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Nine_Sprite[i]);
                }

                break;
            case 1:
                for (int i = 0; i < Ten_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Ten_Sprite[i]);
                }

                break;
            case 3:
                for (int i = 0; i < J_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(J_Sprite[i]);
                }

                break;
            case 4:
                for (int i = 0; i < K_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(K_Sprite[i]);
                }
                break;
            case 5:
                for (int i = 0; i < Q_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Q_Sprite[i]);
                }
                break;
            case 2:
                for (int i = 0; i < A_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(A_Sprite[i]);
                }
                break;
            case 6:
                for (int i = 0; i < Hedgehog_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Hedgehog_Sprite[i]);
                }
                break;
            case 7:
                for (int i = 0; i < Crab_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Crab_Sprite[i]);
                }
                break;
            case 8:
                for (int i = 0; i < JellyFish_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(JellyFish_Sprite[i]);
                }
                break;
            case 9:
                for (int i = 0; i < Turtle_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Turtle_Sprite[i]);
                }
                break;
            case 10:
                for (int i = 0; i < Shell_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Shell_Sprite[i]);
                }
                break;
            case 11:
                for (int i = 0; i < Octopus_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Octopus_Sprite[i]);
                }
                break;
            case 12:
                for (int i = 0; i < Bonus_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Bonus_Sprite[i]);
                }
                break;
            case 13:
                for (int i = 0; i < Wild_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Wild_Sprite[i]);
                }
                break;
            case 14:
                for (int i = 0; i < Scatter_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Scatter_Sprite[i]);
                }
                break;
            case 15:
                for (int i = 0; i < FreeSpin_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(FreeSpin_Sprite[i]);
                }

                break;
            case 16:
                for (int i = 0; i < Jackpot_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Jackpot_Sprite[i]);
                }

                break;
        }
    }

    //starts the spin process
    private void StartSlots(bool autoSpin = false)
    {

        if (!autoSpin)
        {
            if (AutoSpinRoutine != null)
            {
                StopCoroutine(AutoSpinRoutine);
                StopCoroutine(tweenroutine);
                tweenroutine = null;
                AutoSpinRoutine = null;
            }
        }

        WinningsAnim(false);

        if (SlotStart_Button) SlotStart_Button.interactable = false;
        if (TempList.Count > 0)
        {
            StopGameAnimation();
        }
        PayCalculator.ResetLines();
        tweenroutine = StartCoroutine(TweenRoutine());
    }
    private void OnApplicationFocus(bool focus)
    {
        if(focus)
        {
            if(!IsSpinning)
            {
                if (audioController) audioController.StopWLAaudio();
            }
        }
    }
    private IEnumerator TweenRoutine()
    {
        audioController.StopWLAaudio();
        audioController.PlaySpinBonusAudio();
        if (currentBalance < currentTotalBet && !IsFreeSpin)
        {
            CompareBalance();
            if (IsAutoSpin)
            {
                StopAutoSpin();
                yield return new WaitForSeconds(1f);
            }
            yield break;
        }

        IsSpinning = true;
        ToggleButtonGrp(false);
        for (int i = 0; i < numberOfSlots; i++)
        {
            InitializeTweening(Slot_Transform[i]);
            yield return new WaitForSeconds(0.1f);
        }
        bet = 0;
        balance = 0;
        if (!IsFreeSpin)
        {

            try
            {
                bet = double.Parse(TotalBet_text.text);
            }
            catch (Exception e)
            {
                Debug.Log("Error while conversion " + e.Message);
            }

            try
            {
                balance = double.Parse(Balance_text.text);
            }
            catch (Exception e)
            {
                Debug.Log("Error while conversion " + e.Message);
            }
            double initAmount = balance;
            balance = balance - (bet);

            DOTween.To(() => initAmount, (val) => initAmount = val, balance, 0.8f).OnUpdate(() =>
            {
                if (Balance_text) Balance_text.text = initAmount.ToString("f2");
            });

        }

        SocketManager.AccumulateResult(BetCounter);

        yield return new WaitUntil(() => SocketManager.isResultdone);


        for (int j = 0; j < SocketManager.resultData.ResultReel.Count; j++)
        {
            List<int> resultnum = SocketManager.resultData.FinalResultReel[j]?.Split(',')?.Select(Int32.Parse)?.ToList();
            for (int i = 0; i < 5; i++)
            {
                if (images[i].slotImages[images[i].slotImages.Count - 5 + j]) images[i].slotImages[images[i].slotImages.Count - 5 + j].sprite = myImages[resultnum[i]];
                PopulateAnimationSprites(images[i].slotImages[images[i].slotImages.Count - 5 + j].gameObject.GetComponent<ImageAnimation>(), resultnum[i]);
            }
        }

        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < numberOfSlots; i++)
        {
            yield return StopTweening(5, Slot_Transform[i], i);
        }
        yield return new WaitForSeconds(0.5f);

        CheckPayoutLineBackend(SocketManager.resultData.linesToEmit, SocketManager.resultData.FinalsymbolsToEmit);
        KillAllTweens();

        CheckPopups = true;

        currentBalance = SocketManager.playerdata.Balance;
        if (SocketManager.resultData.jackpot > 0)
        {
            uiManager.PopulateWin(4, SocketManager.resultData.jackpot);

            yield return new WaitUntil(() => !CheckPopups);
            CheckPopups = true;

        }

        if (SocketManager.resultData.isBonus)
        {
            bonus_Controller.StartBonusGame(SocketManager.resultData.BonusResult);
            yield return new WaitUntil(() => bonus_Controller.isfinished);
            yield return new WaitForSeconds(1f);
            bonus_Controller.FinishBonusGame();

        }
        else if (SocketManager.resultData.WinAmout >= currentTotalBet * 10 && SocketManager.resultData.WinAmout < currentTotalBet * 15 && SocketManager.resultData.jackpot == 0)
        {
            uiManager.PopulateWin(1, SocketManager.resultData.WinAmout);
        }
        else if (SocketManager.resultData.WinAmout >= currentTotalBet * 15 && SocketManager.resultData.WinAmout < currentTotalBet * 20 && SocketManager.resultData.jackpot == 0)
        {
            uiManager.PopulateWin(2, SocketManager.resultData.WinAmout);
        }
        else if (SocketManager.resultData.WinAmout >= currentTotalBet * 20 && SocketManager.resultData.jackpot == 0)
        {
            uiManager.PopulateWin(3, SocketManager.resultData.WinAmout);
        }
        else
        {
            CheckPopups = false;
        }

        yield return new WaitUntil(() => !CheckPopups);
        if (audioController) audioController.StopWLAaudio();


        if (TotalWin_text) TotalWin_text.text = SocketManager.resultData.WinAmout.ToString("f2");
        if (Balance_text) Balance_text.text = SocketManager.playerdata.Balance.ToString("f2");


        if (!IsAutoSpin)
        {
            ToggleButtonGrp(true);
            IsSpinning = false;
        }
        else
        {
            yield return new WaitForSeconds(2f);
            IsSpinning = false;
        }

        if (SocketManager.resultData.freeSpins > 0 && !IsFreeSpin)
        {
            FreeSpins += (int)SocketManager.resultData.freeSpins;
            uiManager.setFreeSpinData(FreeSpins);
            if (IsAutoSpin)
            {
                StopAutoSpin();
                yield return new WaitForSeconds(0.1f);
            }

            yield return new WaitForSeconds(1.0f);
            FreeSpin(FreeSpins);
        }

    }

    internal void CallCloseSocket()
    {
        SocketManager.CloseSocket();
    }

    private void CompareBalance()
    {
        if (currentBalance < currentTotalBet)
        {
            uiManager.LowBalPopup();
            if (AutoSpin_Button) AutoSpin_Button.interactable = false;
            if (SlotStart_Button) SlotStart_Button.interactable = false;
        }
        else
        {
            if (AutoSpin_Button) AutoSpin_Button.interactable = true;
            if (SlotStart_Button) SlotStart_Button.interactable = true;
        }
    }

    void ToggleButtonGrp(bool toggle)
    {

        if (SlotStart_Button) SlotStart_Button.interactable = toggle;
        if (MaxBet_Button) MaxBet_Button.interactable = toggle;
        if (AutoSpin_Button) AutoSpin_Button.interactable = toggle;
        if (LinePlus_Button) LinePlus_Button.interactable = toggle;
        if (LineMinus_Button) LineMinus_Button.interactable = toggle;
        if (BetMinus_Button) BetMinus_Button.interactable = toggle;
        if (BetPlus_Button) BetPlus_Button.interactable = toggle;

    }

    //start the icons animation
    private void StartGameAnimation(GameObject animObjects)
    {

        ImageAnimation temp = animObjects.GetComponent<ImageAnimation>();
        temp.StartAnimation();
        TempList.Add(temp);

    }

    //stop the icons animation
    private void StopGameAnimation()
    {
        for (int i = 0; i < TempList.Count; i++)
        {
            TempList[i].StopAnimation();
        }
        TempList.Clear();
        TempList.TrimExcess();
    }

    //Win Animation When A Line Is Matched
    private void WinningsAnim(bool IsStart)
    {
        if (IsStart)
        {
            WinTween = TotalWin_text.gameObject.GetComponent<RectTransform>().DOScale(new Vector2(1.5f, 1.5f), 1f).SetLoops(-1, LoopType.Yoyo).SetDelay(0);
        }
        else
        {
            WinTween.Kill();
            TotalWin_text.gameObject.GetComponent<RectTransform>().localScale = Vector3.one;
        }
    }

    internal void shuffleInitialMatrix()
    {
        for (int i = 0; i < Tempimages.Count; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                int randomIndex = UnityEngine.Random.Range(0, myImages.Length);
                Tempimages[i].slotImages[j].sprite = myImages[randomIndex];
            }
        }
    }

    private void CheckPayoutLineBackend(List<int> LineId, List<string> points_AnimString)
    {
        audioController.StopApinBonusAudio();

        List<int> y_points = null;
        List<int> points_anim = null;
        if (LineId.Count > 0)
        {
            if (audioController) audioController.PlayWLAudio("win");

            for (int i = 0; i < LineId.Count; i++)
            {
                y_points = y_string[LineId[i] + 1]?.Split(',')?.Select(Int32.Parse)?.ToList();
                PayCalculator.GeneratePayoutLinesBackend(y_points, y_points.Count);
            }

            for (int i = 0; i < points_AnimString.Count; i++)
            {
                points_anim = points_AnimString[i]?.Split(',')?.Select(Int32.Parse)?.ToList();

                for (int k = 0; k < points_anim.Count; k++)
                {
                    if (points_anim[k] >= 10)
                    {
                        StartGameAnimation(Tempimages[(points_anim[k] / 10) % 10].slotImages[points_anim[k] % 10].gameObject);
                    }
                    else
                    {
                        StartGameAnimation(Tempimages[0].slotImages[points_anim[k]].gameObject);
                    }
                }
            }
            WinningsAnim(true);
        }
        //else
        //{

        //    if (audioController) audioController.StopWLAaudio();
        //}
    }

    private void GenerateMatrix(int value)
    {
        for (int j = 0; j < 3; j++)
        {
            Tempimages[value].slotImages.Add(images[value].slotImages[images[value].slotImages.Count - 5 + j]);
        }
    }

    #region TweeningCode
    private void InitializeTweening(Transform slotTransform)
    {
        slotTransform.localPosition = new Vector2(slotTransform.localPosition.x, 0);
        Tweener tweener = slotTransform.DOLocalMoveY(-tweenHeight, 0.2f).SetLoops(-1, LoopType.Restart).SetDelay(0);
        tweener.Play();
        alltweens.Add(tweener);
    }



    private IEnumerator StopTweening(int reqpos, Transform slotTransform, int index)
    {
        alltweens[index].Pause();
        int tweenpos = (reqpos * (IconSizeFactor + SpacingFactor)) - (IconSizeFactor + (2 * SpacingFactor));
        alltweens[index] = slotTransform.DOLocalMoveY(-tweenpos + 100 + (SpacingFactor > 0 ? SpacingFactor / 4 : 0), 0.5f).SetEase(Ease.OutElastic);
        yield return new WaitForSeconds(0.2f);
    }


    private void KillAllTweens()
    {
        for (int i = 0; i < numberOfSlots; i++)
        {
            alltweens[i].Kill();
        }
        alltweens.Clear();

    }
    #endregion

}

[Serializable]
public class SlotImage
{
    public List<Image> slotImages = new List<Image>(10);
}

