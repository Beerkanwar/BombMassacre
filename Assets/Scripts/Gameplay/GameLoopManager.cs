using UnityEngine;
using Platformer.UI;

public class GameLoopManager : MonoBehaviour {
    [Header("Lives System")]
    public int maxLives = 3;
    private int currentLives;

    [Header("UI Panels")]
    public GameObject gameOverPanel;
    public GameObject levelCompletePanel;
    public GameObject comingSoonPanel;
    public GameObject livesUIParent;
    [Header("HUD Elements")]
    public GameObject[] lifeIcons;

    [Header("References")]
    public MetaGameController metaGameController;

    void Start() {
        currentLives = maxLives;
        UpdateLivesUI();
        if (livesUIParent != null) livesUIParent.SetActive(false);
    }

    public bool OnPlayerDied() {
        currentLives--;
        UpdateLivesUI();
        if (currentLives <= 0) {
            Invoke("ShowGameOver", 1.5f); // Wait 1.5s for the death animation to play
            return false; // No lives left
        }
        return true; // Still has lives
    }

    public void ShowGameOver() {
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f; // Pause the game
    }

    public void ShowComingSoon() {
        comingSoonPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    // Called by LevelExit.cs
    public void OnLevelComplete() {
        levelCompletePanel.SetActive(true);
        Time.timeScale = 0f; // Pause the game
    }

    // Button Action: Return to Menu (Used by both Win and Lose screens)
    public void ReturnToMenu() {
        currentLives = maxLives; // Reset lives
        UpdateLivesUI();
        gameOverPanel.SetActive(false);
        levelCompletePanel.SetActive(false);
        comingSoonPanel.SetActive(false);
        if (livesUIParent != null) livesUIParent.SetActive(false);

        // Force a respawn so the player's health resets and the camera re-attaches!
        Platformer.Core.Simulation.Schedule<Platformer.Gameplay.PlayerSpawn>(0);

        if (metaGameController != null) {
            metaGameController.ToggleMainMenu(true);
        }
    }

    private void UpdateLivesUI() {
        if (lifeIcons == null || lifeIcons.Length == 0) return;

        for (int i = 0; i < lifeIcons.Length; i++) {
            // If the index is less than current lives, turn it on. Otherwise, turn it off.
            lifeIcons[i].SetActive(i < currentLives);
        }
    }
}