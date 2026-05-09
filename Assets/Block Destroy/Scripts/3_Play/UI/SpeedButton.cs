using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SpeedButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Text buttonText;
    private bool isSpeedUp = false;
    private float normalTimeScale = 1f;
    private float speedUpTimeScale = 2f;

    private void Start()
    {
        if (buttonText == null)
        {
            buttonText = GetComponentInChildren<Text>();
        }

        if (buttonText != null)
        {
            buttonText.text = "x2";
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isSpeedUp)
        {
            isSpeedUp = true;
            Time.timeScale = speedUpTimeScale;

            if (buttonText != null)
            {
                buttonText.color = Color.yellow;
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isSpeedUp)
        {
            isSpeedUp = false;
            Time.timeScale = normalTimeScale;

            if (buttonText != null)
            {
                buttonText.color = Color.white;
            }
        }
    }

    private void OnDisable()
    {
        Time.timeScale = normalTimeScale;
        isSpeedUp = false;
    }

    private void OnDestroy()
    {
        Time.timeScale = normalTimeScale;
    }
}
