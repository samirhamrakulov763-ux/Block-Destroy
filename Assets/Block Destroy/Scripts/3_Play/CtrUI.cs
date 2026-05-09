using System.Collections;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class CtrUI : CtrBase
{

    static CtrUI _instance;

    public static CtrUI instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<CtrUI>();
            }

            return _instance;
        }
    }


    public PopupPause _PopupPause;
    public PopupContinue _PopupContinue;
    public TextMeshProUGUI textBallCount;
    public GameObject btnReturnBall;
    public TextMeshProUGUI textTurn;

    public TextMeshProUGUI textScore;

    public Sprite[] spriteCombo;

    public ComboEffectText _ComboEffectText;
    public ButtonRocket _ButtonRocket;

    public void SetTurn(int num)
    {
        textTurn.text = num.ToString();
    }

    public void AllClear()
    {
        _ComboEffectText.AllClear();

        //Counting GameData
        PlayManager.Instance.countAllClear++;

        // Publish all clear event
        EventBus.Instance.Publish(new AllClearEvent());
    }

    bool isLucky = false;

    public void LuckyBonus()
    {
        if (isLucky) return;
        isLucky = true;
        _ComboEffectText.Lucky();

        //Counting GameData
        PlayManager.Instance.countLuckyBonus++;

        // Publish lucky bonus event
        EventBus.Instance.Publish(new LuckyBonusEvent());
    }

    /// <summary>
    /// Reset for next turn
    /// </summary>
    public void NextTurnReady()
    {
        isLucky = false;
        _ButtonRocket.CheckRocketCoolTime();

    }


    private void Awake()
    {
        _PopupPause.UIReset();
        _PopupContinue.UIReset();
        _ComboEffectText.UIReset();

        SetReturnBallButton(false);
        SetResolutionScreen();

        // Subscribe to events
        EventBus.Instance.Subscribe<TurnChangedEvent>(OnTurnChanged);
        EventBus.Instance.Subscribe<ScoreChangedEvent>(OnScoreChanged);
        EventBus.Instance.Subscribe<AllClearEvent>(OnAllClear);
        EventBus.Instance.Subscribe<LuckyBonusEvent>(OnLuckyBonus);
    }

    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (EventBus.Instance != null)
        {
            EventBus.Instance.Unsubscribe<TurnChangedEvent>(OnTurnChanged);
            EventBus.Instance.Unsubscribe<ScoreChangedEvent>(OnScoreChanged);
            EventBus.Instance.Unsubscribe<AllClearEvent>(OnAllClear);
            EventBus.Instance.Unsubscribe<LuckyBonusEvent>(OnLuckyBonus);
        }
    }

    // Event handlers
    private void OnTurnChanged(TurnChangedEvent evt)
    {
        textTurn.text = evt.TurnNumber.ToString();
    }

    private void OnScoreChanged(ScoreChangedEvent evt)
    {
        textScore.text = Utility.ChangeThousandsSeparator(evt.NewScore);
    }

    private void OnAllClear(AllClearEvent evt)
    {
        _ComboEffectText.AllClear();
        PlayManager.Instance.countAllClear++;
    }

    private void OnLuckyBonus(LuckyBonusEvent evt)
    {
        if (!isLucky)
        {
            isLucky = true;
            _ComboEffectText.Lucky();
            PlayManager.Instance.countLuckyBonus++;
        }
    }

    public Camera mainCamera;


    int width;
    int heigh;
    float screenRatio;

    public void SetResolutionScreen()
    {
        float screenRatio = (1.0f * Screen.width) / (1.0f * Screen.height);


#if UNITY_ANDROID
        if (screenRatio < 0.47f) {
            //9:19.5
            mainCamera.orthographicSize = 8f;
        } else if (screenRatio > 0.47f && screenRatio < 0.48f) {
            //9:19
            mainCamera.orthographicSize = 7.8f;
        } else if (screenRatio > 0.48f && screenRatio < 0.495f) {
            //9:18.5
            mainCamera.orthographicSize = 7.6f;
        } else if (screenRatio > 0.495f && screenRatio < 0.55f) {
            //9:18
            mainCamera.orthographicSize = 7.4f;
        } else {
            //9:16
            mainCamera.orthographicSize = 6.2f;
        }
#else
        if (screenRatio > 0.6f && screenRatio < 0.7f)
        {
            //3:2 iPhones models 4 and earlier
        }
        else if (screenRatio > 0.5f && screenRatio < 0.6f)
        {
            //16:9 iPhones models 5, SE, 8+
        }
        else if (screenRatio > 0.4f && screenRatio < 0.5f)
        {
            //19.5:9 iPhones - models X, Xs, Xr, Xsmax
            mainCamera.orthographicSize = 7.8f;
        }
        else
        {
            //Find Not iPhones Size
        }
#endif
    }


    public void SetBallCount(int ballCount)
    {
        textBallCount.text = string.Format("x{0}", ballCount);
    }

    public void Click_Pause()
    {
        SoundManager.Instance.PlayEffect(SoundList.sound_common_btn_in);
        _PopupPause.Open();
    }


    public void SetReturnBallButton(bool value)
    {
        btnReturnBall.transform.DOKill();

        if (value)
        {
            btnReturnBall.transform.DOScale(1f, 0.1f).SetEase(Ease.OutCubic);
        }
        else
        {
            btnReturnBall.transform.DOScale(0f, 0f);
        }
    }

    public void Click_ReturnBall()
    {
        if (Player.instance.isReturnBall) return;
        Player.instance.isReturnBall = true;

        SoundManager.Instance.PlayEffect(SoundList.sound_play_common_sfx_ballcollect);
        Player.instance.ReturnBall();
    }


    bool isScoreAnim = false;

    public void AddScore(int num)
    {
        int oldScore = PlayManager.Instance.score;
        PlayManager.Instance.score += num;

        // Publish score changed event
        EventBus.Instance.Publish(new ScoreChangedEvent(PlayManager.Instance.score, num));

        StartCoroutine(ScoreAnimCo(num));
    }

    IEnumerator ScoreAnimCo(int num)
    {
        isScoreAnim = true;
        int bScore = PlayManager.Instance.score - num;
        int score = PlayManager.Instance.score;

        DOTween.To(() => bScore, x => score = x, score, 0.5f).SetEase(Ease.OutCubic)
            .OnComplete(() => { isScoreAnim = false; });

        while (isScoreAnim)
        {
            textScore.text = Utility.ChangeThousandsSeparator(score);
            yield return null;
        }
    }
}