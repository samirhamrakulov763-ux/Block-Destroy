using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using UnityEngine.Networking;


[System.Serializable]
public class RankingData
{
    public int rank;
    public string countryName;
    public string userName;
    public int score;
    public int turn;
}

[System.Serializable]
public class RankingDataList
{
    public List<RankingData> items;
}

public class PanelRanking : PanelBase
{

    public RankingList myRankingList;
    public Transform content;
    public GameObject pRankingList;
    public List<RankingList> rankingLists = new List<RankingList>();
    public List<RankingData> rankingDatas = new List<RankingData>();

    public CanvasGroup canvasGroupLoading;
    public Transform loading;
    public Sprite[] flagSprites;

    // GameObject'ы для режимов
    [Header("Mode GameObjects")]
    public GameObject normalModeObject;      // GameObject для Normal режима
    public GameObject arcadeModeObject;      // GameObject для Arcade режима

    public bool isRankingDataLoad = false;

    // Режим отображения рейтинга: Normal или Arcade
    public enum RankingMode
    {
        Normal,
        Arcade
    }

    private RankingMode currentMode = RankingMode.Normal;


    //Streaming Asset Folder Path
#if UNITY_EDITOR
    string streamingPath = Application.streamingAssetsPath;
#elif UNITY_IOS
    string streamingPath = Application.dataPath + "/Raw";
#elif UNITY_ANDROID
    string streamingPath = Application.streamingAssetsPath;
#endif

    /// <summary>
    /// Initialize all UI.
    /// </summary>
    public void UIReset()
    {
        canvasGroup.transform.DOScale(0.95f, 0f);
        canvasGroup.DOFade(0f, 0f);
        content.gameObject.SetActive(false);

        for (int i = 0; i < 100; i++)
        {
            GameObject list = Instantiate(pRankingList);
            list.transform.SetParent(content, false);
            list.transform.SetAsLastSibling();
            list.SetActive(false);
            rankingLists.Add(list.GetComponent<RankingList>());
        }
    }

    public void GetData()
    {
        StartCoroutine(GeteDataCo());
    }

    IEnumerator GeteDataCo()
    {
        // Загружаем данные из Firebase вместо локального файла
        bool dataLoaded = false;

        FirebaseLeaderboardManager.Instance.GetLeaderboard(
            currentMode == RankingMode.Arcade,
            100,
            (data) => {
                rankingDatas = data;
                dataLoaded = true;
            }
        );

        // Ждем пока данные загрузятся
        while (!dataLoaded)
        {
            yield return null;
        }

        isRankingDataLoad = true;
    }

    //Retrieve the RankingRankingData.json ranking data in the StreamingAssets folder and put it in the rankingDatas list.
    IEnumerator GetRankingDataCo()
    {
        string path = streamingPath + "/SampleRankingData.json";
        string jsonString = "";

        UnityWebRequest www = UnityWebRequest.Get(path);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            jsonString = www.downloadHandler.text;
        }
        else
        {
            Debug.LogError("Failed to load ranking data: " + www.error);
            // Используем пустой массив если файл не загрузился
            jsonString = "[]";
        }

        try
        {
            RankingDataList wrapper = JsonUtility.FromJson<RankingDataList>("{\"items\":" + jsonString + "}");
            rankingDatas = wrapper.items;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to parse ranking JSON: " + e.Message);
            rankingDatas = new List<RankingData>();
        }
    }



    public void ShowRanking()
    {
        Loading(true);

        //Internet check for server communication
        // if (!PlayManager.Instance.IsInternet()) return;

        // Установить GameObject текущего режима
        UpdateModeObject();

        content.gameObject.SetActive(false);
        StartCoroutine(LoadRankingScoreCo());
    }


    //todo 데이터가져오기
    IEnumerator LoadRankingScoreCo()
    {
        GetData();
        while (!isRankingDataLoad) yield return null;

        // Выбираем данные в зависимости от текущего режима
        int bestScore;
        int bestTurn;

        if (currentMode == RankingMode.Arcade)
        {
            bestScore = GameData.BestScoreArcade;
            bestTurn = GameData.BestTurnArcade;
        }
        else
        {
            bestScore = GameData.BestScore;
            bestTurn = GameData.BestTurn;
        }

        // Найти ранг игрока в списке
        int myRank = 999;
        string myDeviceId = SystemInfo.deviceUniqueIdentifier;

        for (int i = 0; i < rankingDatas.Count; i++)
        {
            // Проверяем по счёту и количеству ходов (так как deviceId не хранится в RankingData)
            if (rankingDatas[i].score == bestScore && rankingDatas[i].turn == bestTurn)
            {
                myRank = rankingDatas[i].rank;
                break;
            }
        }

        //Setting my ranking information
        myRankingList.SetList(
            myRank,
            GetLangFlag(GameData.CountryCode),
            GameData.NickName,
            bestScore,
            bestTurn
        );


        //List of all ranking information
        for (int i = 0; i < rankingDatas.Count; i++)
        {
            rankingLists[i].SetList(
                rankingDatas[i].rank,
                GetLangFlag(rankingDatas[i].countryName),
                rankingDatas[i].userName,
                rankingDatas[i].score,
                rankingDatas[i].turn
            );

            //Change the color of my list in the overall score
            //if(data.rank == i+1) {
            //    rankingLists[i].textName.color = PlayManager.Instance.HexToColor("FFF028");
            //}

        }

        yield return null;

        //Loading complete
        content.gameObject.SetActive(true);
        Loading(false);
    }


   /// <summary>
   /// LoadingAnimation
   /// </summary>
    void Loading(bool value)
    {
        if (value)
        {
            canvasGroupLoading.gameObject.SetActive(true);
            loading.transform.DORotate(new Vector3(0f, 0f, -360f), 1f).SetRelative(true).SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart).SetUpdate(true);
            canvasGroupLoading.DOKill();
            canvasGroupLoading.DOFade(1f, 0.25f).SetEase(Ease.OutSine).SetUpdate(true).SetUpdate(true);
        }
        else
        {
            canvasGroupLoading.DOKill();
            canvasGroupLoading.DOFade(0f, 0.15f).SetEase(Ease.OutSine).OnComplete(() =>
            {
                loading.transform.DOKill();
                canvasGroupLoading.gameObject.SetActive(false);
            }).SetUpdate(true);
        }
    }



    /// <summary>
    /// Flag already imported with country code
    /// </summary>
    public Sprite GetLangFlag(string code)
    {
        string res = code.ToLower();
        res.ToLower();

        foreach (Sprite sprite in flagSprites)
        {

            if (sprite.name == res)
            {
                return sprite;
            }
        }

        return null;
    }

    /// <summary>
    /// Переключение на обычный режим рейтинга
    /// </summary>
    public void SwitchToNormalMode()
    {
        if (currentMode == RankingMode.Normal) return;

        currentMode = RankingMode.Normal;
        UpdateModeObject();
        RefreshRanking();
    }

    /// <summary>
    /// Переключение на Arcade режим рейтинга
    /// </summary>
    public void SwitchToArcadeMode()
    {
        if (currentMode == RankingMode.Arcade) return;

        currentMode = RankingMode.Arcade;
        UpdateModeObject();
        RefreshRanking();
    }

    /// <summary>
    /// Обновление отображения GameObject режима
    /// </summary>
    private void UpdateModeObject()
    {
        if (currentMode == RankingMode.Normal)
        {
            // Включить Normal, выключить Arcade
            if (normalModeObject != null)
                normalModeObject.SetActive(true);

            if (arcadeModeObject != null)
                arcadeModeObject.SetActive(false);
        }
        else
        {
            // Включить Arcade, выключить Normal
            if (normalModeObject != null)
                normalModeObject.SetActive(false);

            if (arcadeModeObject != null)
                arcadeModeObject.SetActive(true);
        }
    }

    /// <summary>
    /// Обновление отображения рейтинга
    /// </summary>
    private void RefreshRanking()
    {
        content.gameObject.SetActive(false);
        isRankingDataLoad = false;
        ShowRanking();
    }
}
