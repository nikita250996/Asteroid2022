using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Resources.Scripts
{
    public class MenuManager : MonoBehaviour
    {
        public static MenuManager Instance { get; private set; }

        [SerializeField] [Tooltip("Кнопка «Продолжить».")]
        private Button _continueButton;

        [Tooltip("Можно ли управлять только клавиатурой.")]
        public bool KeyboardOnlyControls { get; private set; } = true;

        [SerializeField] [Tooltip("Текстовое поля кнопки «Управление».")]
        private Text _controlsText;

        private readonly Dictionary<bool, Tuple<KeyCode[], string>> _controlsInformation = new()
        {
            [true] = new Tuple<KeyCode[], string>
                (new[] {KeyCode.Space}, "Управление: клавиатура"),
            [false] = new Tuple<KeyCode[], string>
                (new[] {KeyCode.Space, KeyCode.Mouse0}, "Управление: клавиатура + мышь")
        };

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(this);

            _controlsText.text = _controlsInformation[KeyboardOnlyControls].Item2;
        }

        private void Continue()
        {
            gameObject.SetActive(false);
        }

        private void NewGame()
        {
            SceneManager.LoadScene("Level", LoadSceneMode.Single);
            gameObject.SetActive(false);
        }

        private void Controls()
        {
            KeyboardOnlyControls = !KeyboardOnlyControls;
            _controlsText.text = _controlsInformation[KeyboardOnlyControls].Item2;
        }

        private void Quit()
        {
            Application.Quit();
        }

        public void Pause(bool gameOver = false)
        {
            gameObject.SetActive(true);

            Time.timeScale = 0f;
            AudioListener.pause = true;
            _continueButton.interactable = !gameOver;
        }

        public KeyCode[] GetFireKeys()
        {
            return _controlsInformation[KeyboardOnlyControls].Item1;
        }

        private void OnDisable()
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
            _continueButton.interactable = false;
        }
    }
}
