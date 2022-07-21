using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Scripts
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        private int _score;

        [SerializeField] [Tooltip("Текстовое поля для отображения очков.")]
        private Text _scoreText;

        [Header("Астероиды")]
        [SerializeField]
        [Range(1, 100)]
        [Tooltip("Сколько астероидов должно появиться в начале игры.")]
        private int _asteroidsCount = 2;

        private int _asteroidsLeft;

        [SerializeField] [Range(0.5f, 5f)] [Tooltip("Длительность перерыва между волнами астероидов.")]
        private float _asteroidsRespawnDelay = 2f;

        private const float SafeIndent = 2f;

        [SerializeField] [Range(1f, 1.5f)] [Tooltip("Минимальная скорость группы астероидов.")]
        private float _asteroidsMinimumSpeed = 1f;

        [SerializeField] [Range(2f, 2.5f)] [Tooltip("Максимальная скорость группы астероидов.")]
        private float _asteroidsMaximumSpeed = 2.5f;

        [SerializeField] [Tooltip("Пул больших астероидов.")]
        private ObjectPooler _bigAsteroidsObjectPooler;

        private bool _asteroidsActivated;

        [Tooltip("Пул средних астероидов.")] public ObjectPooler _mediumAsteroidsObjectPooler;
        [Tooltip("Пул маленьких астероидов.")] public ObjectPooler _smallAsteroidsObjectPooler;

        public enum AsteroidTypes
        {
            Big,
            Medium,
            Small
        }

        [Header("НЛО")] [SerializeField] [Tooltip("Префаб НЛО.")]
        private GameObject _UFO;

        [SerializeField] [Range(10f, 25f)] [Tooltip("Минимальное время до следующего появления НЛО.")]
        private float _minimumUFOSpawnTime = 20f;

        [SerializeField] [Range(30f, 60f)] [Tooltip("Максимальное время до следующего появления НЛО.")]
        private float _maximumUFOSpawnTime = 40f;

        private float _UFOSpawnVerticalIndent;

        [SerializeField]
        [Range(0.15f, 0.5f)]
        [Tooltip(
            "На каком удалении от границ экрана сверху и снизу будет НЛО при появлении (пример: 0.5 — сдвиг на половину экрана).")]
        private float _UFOSpawnVerticalIndentCoefficient = 0.2f;

        [Tooltip("Пул пуль НЛО.")] public ObjectPooler UFOBulletsObjectPooler;

        [Header("Космический корабль")] [SerializeField] [Tooltip("Префаб космического корабля.")]
        private GameObject _spaceshipPrefab;

        [SerializeField] [Range(0.5f, 2f)] [Tooltip("Через сколько возродится космический корабль.")]
        private float _spaceshipRespawnDelay = 1f;

        [HideInInspector] public Spaceship SpaceshipInstance;

        [SerializeField] [Range(1, 20)] [Tooltip("Число жизней космического корабля.")]
        private int _lives = 4;

        private GameObject[] _liveIcons;

        [SerializeField] [Tooltip("Префаб иконки здоровья.")]
        private GameObject _liveIcon;

        [SerializeField] [Tooltip("К чему будут крепиться иконки здоровья.")]
        private GameObject _liveIconsHub;

        [SerializeField] [Range(25f, 40f)] [Tooltip("Расстояние между иконками здоровья.")]
        private float _livesIconsIndent = 25f;

        [SerializeField] [Range(0.2f, 0.3f)] [Tooltip("Коэффициент масштабирования иконки здоровья.")]
        private float _liveIconScale = 0.25f;

        [SerializeField] private AudioSource _audioSource;

        [Tooltip("Пул пуль космического корабля.")]
        public ObjectPooler SpaceshipBulletsObjectPooler;

        public float GameFieldHalfHeight { get; private set; }
        public float GameFieldHeight { get; private set; }

        public float GameFieldHalfWidth { get; private set; }
        public float GameFieldWidth { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        }

        private void Start()
        {
            SetupGameFieldVariables();

            SetupLiveIcons();

            _asteroidsActivated = true;
            ActivateAsteroids();
            Invoke(nameof(SpawnUFO), Random.Range(_minimumUFOSpawnTime, _maximumUFOSpawnTime));
            SpawnSpaceship();
        }

        private void SetupGameFieldVariables()
        {
            GameFieldHalfHeight = Camera.main.orthographicSize;
            GameFieldHeight = GameFieldHalfHeight * 2f;

            float screenRatio = (float) Screen.width / Screen.height;

            GameFieldHalfWidth = GameFieldHalfHeight * screenRatio;
            GameFieldWidth = GameFieldHalfWidth * 2f;

            _UFOSpawnVerticalIndent = GameFieldHalfHeight * _UFOSpawnVerticalIndentCoefficient;
        }

        private void SetupLiveIcons()
        {
            _liveIcons = new GameObject[_lives];
            float indent = 0f;
            for (int i = 0; i < _lives; ++i)
            {
                _liveIcons[i] = Instantiate(_liveIcon);
                _liveIcons[i].transform.SetParent(_liveIconsHub.transform);
                RectTransform liveIconRectTransform = _liveIcons[i].GetComponent<RectTransform>();
                liveIconRectTransform.localScale = new Vector3(_liveIconScale, _liveIconScale);
                liveIconRectTransform.anchoredPosition = new Vector2(indent, 0);
                indent += _livesIconsIndent;
            }
        }

        private void ActivateAsteroids()
        {
            for (int i = 0; i < _asteroidsCount; ++i)
            {
                float positionX = Random.Range(-GameFieldHalfWidth, GameFieldHalfWidth);
                float positionY = Random.value > 0.5f
                    ? Random.Range(-GameFieldHalfHeight, -GameFieldHalfHeight + SafeIndent)
                    : Random.Range(GameFieldHalfHeight - SafeIndent, GameFieldHalfHeight);
                Vector3 newBigAsteroidPosition = new(positionX, positionY);

                GameObject bigAsteroid = _bigAsteroidsObjectPooler.GetObject();
                if (bigAsteroid == null)
                {
                    continue;
                }

                bigAsteroid.transform.SetPositionAndRotation(newBigAsteroidPosition,
                    Quaternion.Euler(0f, 0f, Random.Range(1f, 359f)));
                Asteroid bigAsteroidComponent = bigAsteroid.GetComponent<Asteroid>();
                bigAsteroidComponent.AsteroidType = AsteroidTypes.Big;
                bigAsteroidComponent.Speed = NewAsteroidsSpeed();
                bigAsteroid.SetActive(true);
            }

            ++_asteroidsCount;
        }

        public float NewAsteroidsSpeed()
        {
            return Random.Range(_asteroidsMinimumSpeed, _asteroidsMaximumSpeed);
        }

        private void SpawnUFO()
        {
            float x = Random.value > 0.5f ? -GameFieldHalfWidth : GameFieldHalfWidth;
            float y = Random.Range(-GameFieldHalfHeight + _UFOSpawnVerticalIndent,
                GameFieldHalfHeight - _UFOSpawnVerticalIndent);
            Instantiate(_UFO, new Vector3(x, y), Quaternion.identity);
        }

        private void SpawnSpaceship()
        {
            SpaceshipInstance = Instantiate(_spaceshipPrefab, Vector3.zero, Quaternion.identity)
                .GetComponent<Spaceship>();
        }

        private void Update()
        {
            HandlePause();
        }

        private static void HandlePause()
        {
            if (!Input.GetKeyDown(KeyCode.Escape) || Time.timeScale == 0f)
            {
                return;
            }

            MenuManager.Instance.Pause();
        }

        public void OnAsteroidActivated()
        {
            ++_asteroidsLeft;
            CancelInvoke(nameof(ActivateAsteroids));
        }

        public void OnAsteroidDeactivated(AudioClip asteroidExplosionSound, bool rewardableDeath, int score)
        {
            if (!_asteroidsActivated)
            {
                _asteroidsLeft = 0;
                return;
            }

            PlaySound(asteroidExplosionSound);
            if (rewardableDeath)
            {
                AddScore(score);
            }

            if (--_asteroidsLeft == 0)
            {
                Invoke(nameof(ActivateAsteroids), _asteroidsRespawnDelay);
            }
        }

        public void OnUFODestroyed(bool peacefulDeath, AudioClip UFOExplosionSound, bool rewardableDeath, int score)
        {
            if (!peacefulDeath)
            {
                PlaySound(UFOExplosionSound);
            }

            if (rewardableDeath)
            {
                AddScore(score);
            }

            Invoke(nameof(SpawnUFO), Random.Range(_minimumUFOSpawnTime, _maximumUFOSpawnTime));
        }

        public void OnSpaceshipDestroyed(AudioClip spaceshipExplosionSound)
        {
            Destroy(_liveIcons[--_lives]);
            PlaySound(spaceshipExplosionSound);

            if (_lives > 0)
            {
                Invoke(nameof(SpawnSpaceship), _spaceshipRespawnDelay);
            }
            else
            {
                Invoke(nameof(Restart), spaceshipExplosionSound.length);
            }
        }

        private void Restart()
        {
            MenuManager.Instance.Pause(true);
        }

        private void AddScore(int score)
        {
            _score += score;
            _scoreText.text = "Score: " + _score;
        }

        public void PlaySound(AudioClip sound)
        {
            if (_audioSource != null && sound != null)
            {
                _audioSource.PlayOneShot(sound);
            }
        }

        public bool IsAudioSourcePlaying()
        {
            return _audioSource.isPlaying;
        }
    }
}
