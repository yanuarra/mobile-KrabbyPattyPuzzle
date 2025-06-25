using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace YRA
{
    public class MenuSystem : Singleton<MenuSystem>
    {

        [Header("Menu Panels")]
        [SerializeField] private GameObject _winPanel;
        [SerializeField] private GameObject _navPanel;

        [Header("Button References")]
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _quitButton;
        [SerializeField] private Button _menuButton;
        [SerializeField] private Button _undoButton;
        [SerializeField] private Button _skipButton;
        [Header("Text References")]
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _winScoreText;
        private GameObject _currentActivePanel;
        private List<GameObject> _allPanels;

        void Start()
        {
            _allPanels = new List<GameObject>
            {
                _winPanel,
                _navPanel
            };
            if (_restartButton) _restartButton.onClick.AddListener(RestartLevel);
            if (_quitButton)    _quitButton.onClick.AddListener(QuitGame);
            if (_menuButton)    _menuButton.onClick.AddListener(ShowNavMenu);
            if (_skipButton)    _skipButton.onClick.AddListener(SkipLevel);
            if (_undoButton)    _undoButton.onClick.AddListener(UndoMove);

            HideAllPanels();
        }

        public void ManualInputReset()
        {
            FindObjectOfType<InputResetManager>().ManualInputReset();
        }

        public void UpdateUI(string score, string level, bool state)
        {
            _scoreText.text = "Score: " + score;
            _levelText.text = "Level: " + level;
            _undoButton.interactable = state;
        }

        public void ToggleMainMenuInteractable(bool state)
        {
            _undoButton.gameObject.SetActive(state);
            _menuButton.gameObject.SetActive(state);
            _skipButton.gameObject.SetActive(state);
        }

        public void SetWinPanel(string message)
        {
            ShowPanel(_winPanel);
            _winScoreText.text = message;
            ToggleMainMenuInteractable(false);
        }
        

        private void ShowPanel(GameObject panel)
        {
            if (panel == null) return;

            HideAllPanels();
            panel.SetActive(true);
            _currentActivePanel = panel;
        }

        public void HideAllPanelsOnClick()
        {
            foreach (var panel in _allPanels)
            {
                if (panel != null)
                    panel.SetActive(false);
            }
            _currentActivePanel = null;
            ToggleMainMenuInteractable(true);
            ManualInputReset();
        }


        public void HideAllPanels()
        {
            foreach (var panel in _allPanels)
            {
                if (panel != null)
                    panel.SetActive(false);
            }
            _currentActivePanel = null;
            ToggleMainMenuInteractable(true);
        }

        public void ShowNavMenu()
        {
            ShowPanel(_navPanel);
            ToggleMainMenuInteractable(false);
            ManualInputReset();
        }

        public void ShowWinMenu()
        {
            ShowPanel(_winPanel);
            ToggleMainMenuInteractable(false);
        }
        
        public void SkipLevel()
        {
            HideAllPanels();
            GameManager.Instance.SkipLevel();
            }

        public void UndoMove()
        {
            HideAllPanels();
            GameManager.Instance.UndoMove();
        }

        public void RestartLevel()
        {
            HideAllPanels();
            SceneManager.Instance.RestartLevel();
        }

        public void QuitGame()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
    }
}
