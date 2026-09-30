using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;
using Platformer.UI;
using UnityEngine;

public class LevelSelectMenu : MonoBehaviour {
    [Header("References")]
    [SerializeField] private GameObject player;
    [SerializeField] private MetaGameController metaGameController;
    [SerializeField] private GameObject livesUIParent;

    [Header("Level Start Positions")]
    [SerializeField] private Transform[] levelStartPoints;

    public void GoToLevel(int levelIndex) {
        if (levelIndex < 0 || levelIndex >= levelStartPoints.Length) return;

        Transform targetStart = levelStartPoints[levelIndex];

        var model = Simulation.GetModel<PlatformerModel>();
        if (model != null) {
            model.spawnPoint = targetStart;
        }

        Simulation.Schedule<Platformer.Gameplay.PlayerSpawn>(0);

        if (metaGameController != null) {
            metaGameController.ToggleMainMenu(false);
        }

        Time.timeScale = 1f;
        if (livesUIParent != null) livesUIParent.SetActive(true);
    }
}