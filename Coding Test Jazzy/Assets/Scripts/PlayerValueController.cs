using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerValueController : MonoBehaviour
{
    [Header("Value Settings")]
    public int currentValue = 1; // Starting head value

    [Header("UI")]
    public TMP_Text valueText;

    [Header("Snake Settings")]
    public ChainManager chainManager; // Assign in inspector

    [Header("Level Fail")]
    public GameObject gameOverUI; // Set your Game Over UI panel here

    private Vector3 originalScale;
    private bool isGameOver = false;

    void Start()
    {
        originalScale = transform.localScale;
        UpdateText();

        // Initialize snake at start
        if (chainManager != null)
            chainManager.InitializeSnake();
    }

    /// <summary>
    /// Call this when player picks up a number
    /// </summary>
    public void AddValue(int amount)
    {
        if (isGameOver) return;

        currentValue += amount;
        currentValue = Mathf.Max(1, currentValue); // Never below 1

        UpdateText();
        PunchScale();

        // Update snake segments
        if (chainManager != null)
            chainManager.UpdateChain();
    }

    /// <summary>
    /// Call this when player hits obstacle to reduce snake
    /// </summary>
    public void ReduceValue(int amount)
    {
        if (isGameOver) return;

        currentValue -= amount;
        UpdateText();

        if (chainManager != null)
            chainManager.UpdateChain();

        // Check for level fail
        if (currentValue <= 0)
        {
            LevelFail();
        }
    }

    /// <summary>
    /// Handles level fail / game over
    /// </summary>
    private void LevelFail()
    {
        if (isGameOver) return;
        isGameOver = true;

        Debug.Log("Game Over! Level Failed.");

        // Show Game Over UI if assigned
        if (gameOverUI != null)
            gameOverUI.SetActive(true);

        // Stop snake updates and destroy segments
        if (chainManager != null)
            chainManager.DestroyAllSegments();

        // Stop position recording
        PlayerPositionHistory history = GetComponent<PlayerPositionHistory>();
        if (history != null)
            history.enabled = false;

        // Optionally, stop player movement here if you have input/movement scripts

        // Optional: restart level after delay
        StartCoroutine(RestartLevelAfterDelay(2f)); // 2 seconds delay
    }

    IEnumerator RestartLevelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Updates the TMP text to show current head value
    /// </summary>
    void UpdateText()
    {
        if (valueText != null)
            valueText.text = currentValue.ToString();
    }

    /// <summary>
    /// Simple punch scale animation on pickup
    /// </summary>
    void PunchScale()
    {
        StopAllCoroutines();
        StartCoroutine(ScalePunch());
    }

    IEnumerator ScalePunch()
    {
        float t = 0f;
        float duration = 0.15f;
        Vector3 targetScale = originalScale * 1.2f;

        // Scale up
        while (t < duration)
        {
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        t = 0f;
        // Scale back to original
        while (t < duration)
        {
            transform.localScale = Vector3.Lerp(targetScale, originalScale, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale;
    }
}
