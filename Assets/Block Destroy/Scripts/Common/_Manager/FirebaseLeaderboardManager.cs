using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Firebase Leaderboard Manager
/// Manages online rankings using Firebase Realtime Database REST API
/// </summary>
public class FirebaseLeaderboardManager : Singleton<FirebaseLeaderboardManager>
{
    // Firebase Realtime Database URL (замени на свой URL из Firebase Console)
    private const string FIREBASE_DATABASE_URL = "https://block-destroy-d2554-default-rtdb.firebaseio.com";

    // Endpoints для разных режимов
    private const string NORMAL_LEADERBOARD_PATH = "/leaderboard/normal";
    private const string ARCADE_LEADERBOARD_PATH = "/leaderboard/arcade";

    /// <summary>
    /// Отправить счет игрока на сервер
    /// </summary>
    public void SubmitScore(string playerName, int score, int turn, bool isArcadeMode, Action<bool> callback = null)
    {
        StartCoroutine(SubmitScoreCoroutine(playerName, score, turn, isArcadeMode, callback));
    }

    private IEnumerator SubmitScoreCoroutine(string playerName, int score, int turn, bool isArcadeMode, Action<bool> callback)
    {
        // Выбираем путь в зависимости от режима
        string path = isArcadeMode ? ARCADE_LEADERBOARD_PATH : NORMAL_LEADERBOARD_PATH;

        // Создаем уникальный ID для записи (используем timestamp + random)
        string entryId = SystemInfo.deviceUniqueIdentifier + "_" + DateTime.UtcNow.Ticks;

        // Создаем объект с данными игрока
        var playerData = new LeaderboardEntry
        {
            playerName = playerName,
            score = score,
            turn = turn,
            countryCode = GameData.CountryCode,
            timestamp = DateTime.UtcNow.ToString("o"),
            deviceId = SystemInfo.deviceUniqueIdentifier
        };

        string jsonData = JsonUtility.ToJson(playerData);
        string url = FIREBASE_DATABASE_URL + path + "/" + entryId + ".json";

        using (UnityWebRequest request = UnityWebRequest.Put(url, jsonData))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();

            bool success = request.result == UnityWebRequest.Result.Success;

            if (!success)
            {
                Debug.LogError("Failed to submit score: " + request.error);
            }

            callback?.Invoke(success);
        }
    }

    /// <summary>
    /// Получить топ рейтинга
    /// </summary>
    public void GetLeaderboard(bool isArcadeMode, int limit, Action<List<RankingData>> callback)
    {
        StartCoroutine(GetLeaderboardCoroutine(isArcadeMode, limit, callback));
    }

    private IEnumerator GetLeaderboardCoroutine(bool isArcadeMode, int limit, Action<List<RankingData>> callback)
    {
        string path = isArcadeMode ? ARCADE_LEADERBOARD_PATH : NORMAL_LEADERBOARD_PATH;

        // Запрос с сортировкой по score и лимитом
        string url = FIREBASE_DATABASE_URL + path + ".json?orderBy=\"score\"&limitToLast=" + limit;

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string jsonResponse = request.downloadHandler.text;

                    // Парсим ответ от Firebase
                    var entries = ParseFirebaseResponse(jsonResponse);

                    // Сортируем по убыванию score
                    entries.Sort((a, b) => b.score.CompareTo(a.score));

                    // Присваиваем ранги
                    for (int i = 0; i < entries.Count; i++)
                    {
                        entries[i].rank = i + 1;
                    }

                    callback?.Invoke(entries);
                }
                catch (Exception e)
                {
                    Debug.LogError("Failed to parse leaderboard: " + e.Message);
                    callback?.Invoke(new List<RankingData>());
                }
            }
            else
            {
                Debug.LogError("Failed to get leaderboard: " + request.error);
                callback?.Invoke(new List<RankingData>());
            }
        }
    }

    /// <summary>
    /// Парсинг ответа от Firebase
    /// </summary>
    private List<RankingData> ParseFirebaseResponse(string jsonResponse)
    {
        var result = new List<RankingData>();

        if (string.IsNullOrEmpty(jsonResponse) || jsonResponse == "null")
        {
            return result;
        }

        try
        {
            // Firebase возвращает объект вида: {"key1": {...}, "key2": {...}}
            // Убираем внешние фигурные скобки и парсим вручную
            jsonResponse = jsonResponse.Trim();

            if (jsonResponse.StartsWith("{") && jsonResponse.EndsWith("}"))
            {
                // Удаляем первую и последнюю скобку
                jsonResponse = jsonResponse.Substring(1, jsonResponse.Length - 2);

                // Разбиваем на отдельные записи по запятым на верхнем уровне
                int braceCount = 0;
                int startIndex = 0;

                for (int i = 0; i < jsonResponse.Length; i++)
                {
                    if (jsonResponse[i] == '{') braceCount++;
                    else if (jsonResponse[i] == '}') braceCount--;
                    else if (jsonResponse[i] == ',' && braceCount == 0)
                    {
                        // Нашли разделитель между записями
                        string entryJson = jsonResponse.Substring(startIndex, i - startIndex);
                        ParseSingleEntry(entryJson, result);
                        startIndex = i + 1;
                    }
                }

                // Обработать последнюю запись
                if (startIndex < jsonResponse.Length)
                {
                    string entryJson = jsonResponse.Substring(startIndex);
                    ParseSingleEntry(entryJson, result);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Parse error: " + e.Message + "\nJSON: " + jsonResponse);
        }

        return result;
    }

    /// <summary>
    /// Парсинг одной записи из Firebase
    /// </summary>
    private void ParseSingleEntry(string entryJson, List<RankingData> result)
    {
        try
        {
            // Формат: "key": {"playerName":"...", "score":123, ...}
            int colonIndex = entryJson.IndexOf(':');
            if (colonIndex > 0)
            {
                // Берём только JSON объект после двоеточия
                string objectJson = entryJson.Substring(colonIndex + 1).Trim();

                // Парсим через JsonUtility
                LeaderboardEntry entry = JsonUtility.FromJson<LeaderboardEntry>(objectJson);

                if (entry != null)
                {
                    result.Add(new RankingData
                    {
                        rank = 0, // Будет присвоен позже
                        countryName = entry.countryCode,
                        userName = entry.playerName,
                        score = entry.score,
                        turn = entry.turn
                    });
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to parse entry: " + e.Message);
        }
    }

    /// <summary>
    /// Получить позицию игрока в рейтинге
    /// </summary>
    public void GetPlayerRank(string deviceId, bool isArcadeMode, Action<int> callback)
    {
        StartCoroutine(GetPlayerRankCoroutine(deviceId, isArcadeMode, callback));
    }

    private IEnumerator GetPlayerRankCoroutine(string deviceId, bool isArcadeMode, Action<int> callback)
    {
        string path = isArcadeMode ? ARCADE_LEADERBOARD_PATH : NORMAL_LEADERBOARD_PATH;
        string url = FIREBASE_DATABASE_URL + path + ".json?orderBy=\"deviceId\"&equalTo=\"" + deviceId + "\"";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // Здесь можно реализовать подсчет ранга
                // Для простоты возвращаем 999
                callback?.Invoke(999);
            }
            else
            {
                callback?.Invoke(999);
            }
        }
    }
}

/// <summary>
/// Структура записи в лидерборде
/// </summary>
[Serializable]
public class LeaderboardEntry
{
    public string playerName;
    public int score;
    public int turn;
    public string countryCode;
    public string timestamp;
    public string deviceId;
}
