using UnityEngine;
using UnityEngine.UI;

public class ToggleTimerOnClick_Legacy : MonoBehaviour
{
    [Header("UI (Legacy)")]
    public Text timerText;

    public GameObject txt;
    private float elapsed = 0f;
    private bool running = false;

    void Start()
    {
        running = false;
        elapsed = 0f;
        UpdateUI();
    }



    void Update()
    {
        // Press T to START (only starts, doesn't stop)
        if (Input.GetKeyDown(KeyCode.T))
        {
            timerText.gameObject.SetActive(true);
            txt.SetActive(false);
            StartTimer();
        }

        if (!running) return;

        elapsed += Time.deltaTime;
        UpdateUI();
    }

    // Button toggle (optional)
    public void ToggleTimer()
    {
        running = !running;
        UpdateUI();
    }

    public void StartTimer()
    {
        running = true;
        UpdateUI();
    }

    public void StopTimer()
    {
        running = false;
        UpdateUI();
    }

    public void ResetTimer()
    {
        running = false;
        elapsed = 0f;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (timerText != null)
            timerText.text = FormatTime(elapsed);

    }

    private string FormatTime(float t)
    {
        int totalSeconds = Mathf.FloorToInt(t);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        return minutes.ToString("00") + ":" + seconds.ToString("00");
    }
}
