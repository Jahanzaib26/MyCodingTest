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

    private Vector3 originalScale;

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
    /// <param name="amount">Value to add</param>
    public void AddValue(int amount)
    {
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
    /// <param name="amount">Value to reduce</param>
    public bool isGameOver = false;

    public void ReduceValue(int amount)
    {
        currentValue -= amount;

        // Update TMP
        UpdateText();

        // Update snake
        if (chainManager != null)
            chainManager.UpdateChain();

        // If value <= 0, restart level
        if (currentValue <= 0)
        {
            GameOverRestart();
        }
    }
    public void GameOverRestart()
    {
        Debug.Log("Game Over! Restarting Level...");

        // Stop snake updates
        if (chainManager != null)
            chainManager.enabled = false;

        PlayerPositionHistory history = GetComponent<PlayerPositionHistory>();
        if (history != null)
            history.enabled = false;

        this.enabled = false;

        // Wait one frame before reloading
        StartCoroutine(RestartLevelNextFrame());
    }

    IEnumerator RestartLevelNextFrame()
    {
        // Wait a tiny moment so all updates finish
        yield return null;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }



    //void GameOver()
    //{
    //    if (isGameOver) return;
    //    isGameOver = true;

    //    Debug.Log("Game Over!");

    //    // Stop all chain segments safely
    //    if (chainManager != null)
    //        chainManager.DestroyAllSegments();

    //    // Stop recording positions
    //    PlayerPositionHistory history = GetComponent<PlayerPositionHistory>();
    //    if (history != null)
    //        history.enabled = false;

    //    // Optional: stop player movement here if you have input
    //}


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
    public void CheckGameOver()
    {
        if (currentValue <= 0)
        {
            Debug.Log("Game Over!");
            // Here you can stop movement, play animation, or reload the scene
        }
    }

    System.Collections.IEnumerator ScalePunch()
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
