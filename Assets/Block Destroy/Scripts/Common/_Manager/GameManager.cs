using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public partial class GameManager : Singleton<GameManager>
{
    public void Initialized()
    {
        SetSystem();
    }

    void SetSystem()
    {
        // Target frame settings
        if (Application.targetFrameRate != 60)
        {
            Application.targetFrameRate = 60;
        }

        // Multi-touch setting
        Input.multiTouchEnabled = false;

        // Local push initialization
        this.gameObject.AddComponent<Localpush>();

        // Canceling sleep mode
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        // Определение страны по IP адресу
        StartCoroutine(DetectUserCountry());
    }

    IEnumerator DetectUserCountry()
    {
        // Если код страны уже установлен - не определяем заново
        if (!string.IsNullOrEmpty(GameData.CountryCode) && GameData.CountryCode != "us")
        {
            Debug.Log("Country code already set: " + GameData.CountryCode);
            yield break;
        }

        using (UnityWebRequest request = UnityWebRequest.Get("https://ip-api.com/json/?fields=countryCode"))
        {
            request.timeout = 5;
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string jsonResponse = request.downloadHandler.text;
                    IPApiResponse response = JsonUtility.FromJson<IPApiResponse>(jsonResponse);

                    if (!string.IsNullOrEmpty(response.countryCode))
                    {
                        string countryCode = response.countryCode.ToLower();
                        GameData.CountryCode = countryCode;
                        Debug.Log("Country detected by IP: " + countryCode);
                    }
                    else
                    {
                        GameData.CountryCode = GetCountryBySystemLanguage();
                        Debug.LogWarning("Failed to detect country by IP, using system language");
                    }
                }
                catch (Exception e)
                {
                    GameData.CountryCode = GetCountryBySystemLanguage();
                    Debug.LogError("Failed to parse IP API response: " + e.Message);
                }
            }
            else
            {
                GameData.CountryCode = GetCountryBySystemLanguage();
                Debug.LogWarning("Failed to detect country by IP: " + request.error);
            }
        }
    }

    string GetCountryBySystemLanguage()
    {
        SystemLanguage lang = Application.systemLanguage;

        switch (lang)
        {
            case SystemLanguage.English: return "us";
            case SystemLanguage.Korean: return "kr";
            case SystemLanguage.Chinese:
            case SystemLanguage.ChineseSimplified:
            case SystemLanguage.ChineseTraditional: return "cn";
            case SystemLanguage.Thai: return "th";
            case SystemLanguage.Japanese: return "jp";
            case SystemLanguage.Russian: return "ru";
            case SystemLanguage.Vietnamese: return "vn";
            case SystemLanguage.Indonesian: return "id";
            case SystemLanguage.Arabic: return "sa";
            case SystemLanguage.Spanish: return "es";
            case SystemLanguage.Portuguese: return "pt";
            case SystemLanguage.French: return "fr";
            case SystemLanguage.German: return "de";
            case SystemLanguage.Italian: return "it";
            case SystemLanguage.Turkish: return "tr";
            default: return "us";
        }
    }
}

[Serializable]
public class IPApiResponse
{
    public string countryCode;
}

