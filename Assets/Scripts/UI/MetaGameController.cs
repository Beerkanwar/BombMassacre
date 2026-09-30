using Platformer.Mechanics;
using Platformer.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Platformer.UI {
    public class MetaGameController : MonoBehaviour {
        public MainUIController mainMenu;
        public Canvas[] gamePlayCanvasii;
        public GameController gameController;

        [Header("Game Jam Settings")]
        [Tooltip("Uncheck this to skip the menu and just play the game immediately.")]
        public bool startWithMenuOpen = true;

        bool showMainCanvas;
        private InputAction m_MenuAction;

        void Start() {
            showMainCanvas = startWithMenuOpen;
            _ToggleMainMenu(showMainCanvas);

            // Safe check for Input System
            if (InputSystem.actions != null) {
                m_MenuAction = InputSystem.actions.FindAction("Player/Menu");
            } else {
                Debug.LogWarning("InputSystem.actions is null. Did you assign an action asset in Project Settings?");
            }
        }

        public void ToggleMainMenu(bool show) {
            _ToggleMainMenu(show);
        }

        void _ToggleMainMenu(bool show) {
            showMainCanvas = show;

            // Pauses the game if menu is shown, unpauses if hidden
            Time.timeScale = show ? 0 : 1;

            if (mainMenu != null) mainMenu.gameObject.SetActive(show);

            foreach (var i in gamePlayCanvasii) {
                if (i != null) i.gameObject.SetActive(!show);
            }
        }

        void Update() {
            if (m_MenuAction != null && m_MenuAction.WasPressedThisFrame()) {
                ToggleMainMenu(!showMainCanvas);
            }
        }
    }
}