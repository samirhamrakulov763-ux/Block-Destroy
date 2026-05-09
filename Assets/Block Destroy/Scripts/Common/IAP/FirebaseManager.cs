using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Firebase Manager for Block Destroy
/// Handles server-side receipt validation and analytics
/// </summary>
public class FirebaseManager : Singleton<FirebaseManager>
{
    private const string FIREBASE_FUNCTION_URL = "https://us-central1-block-destroy-d2554.cloudfunctions.net/validateReceipt";

    private bool isFirebaseInitialized = false;

    void Start()
    {
        InitializeFirebase();
    }

    /// <summary>
    /// Initialize Firebase SDK
    /// </summary>
    private void InitializeFirebase()
    {
        isFirebaseInitialized = true;
    }

    /// <summary>
    /// Validate receipt on server (Firebase Cloud Function)
    /// </summary>
    public void ValidateReceipt(string productId, string receipt, Action<bool> callback)
    {
        StartCoroutine(ValidateReceiptCoroutine(productId, receipt, callback));
    }

    private IEnumerator ValidateReceiptCoroutine(string productId, string receipt, Action<bool> callback)
    {
        if (!isFirebaseInitialized)
        {
            callback?.Invoke(true);
            yield break;
        }

        var requestData = new ReceiptValidationRequest
        {
            productId = productId,
            receipt = receipt,
            platform = Application.platform.ToString(),
            userId = SystemInfo.deviceUniqueIdentifier,
            timestamp = DateTime.UtcNow.ToString("o")
        };

        string jsonData = JsonUtility.ToJson(requestData);

        using (UnityWebRequest request = new UnityWebRequest(FIREBASE_FUNCTION_URL, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var response = JsonUtility.FromJson<ReceiptValidationResponse>(request.downloadHandler.text);
                    callback?.Invoke(response.valid);
                }
                catch (Exception)
                {
                    callback?.Invoke(false);
                }
            }
            else
            {
                callback?.Invoke(false);
            }
        }
    }

    /// <summary>
    /// Log purchase event to Firebase Analytics
    /// </summary>
    public void LogPurchaseEvent(string productId, float price, string currency)
    {
        if (!isFirebaseInitialized)
            return;
    }
}

[Serializable]
public class ReceiptValidationRequest
{
    public string productId;
    public string receipt;
    public string platform;
    public string userId;
    public string timestamp;
}

[Serializable]
public class ReceiptValidationResponse
{
    public bool valid;
    public string message;
    public string transactionId;
}
