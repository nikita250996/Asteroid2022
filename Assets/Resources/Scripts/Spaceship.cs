using System.Linq;
using UnityEngine;
using UnityResources = UnityEngine.Resources;

namespace Assets.Resources.Scripts
{
    public class Spaceship : MonoBehaviour
    {
        [Header("Движение")] [SerializeField] [Range(5f, 20f)] [Tooltip("Максимальная скорость.")]
        private float _maximumSpeed = 10f;

        [SerializeField] [Range(1f, 5f)] [Tooltip("Максимальная скорость поворота.")]
        private float _rotationSpeed = 3f;

        [SerializeField]
        [Range(50f, 100f)]
        [Tooltip("Коэффициент ускорения скорости поворота при управлении только клавиатурой.")]
        private float _keyboardControlsRotationSpeedAcceleration = 75f;

        [SerializeField] [Range(0.01f, 0.1f)] [Tooltip("Ускорение.")]
        private float _acceleration = 0.05f;

        [SerializeField] [Tooltip("Звук «газа».")]
        private AudioClip _thrustSound;

        private float _velocity;

        [Header("Неуязвимость")] [SerializeField] [Range(0f, 10f)] [Tooltip("Длительность неуязвимости.")]
        private float _invincibilityTime = 3f;

        [SerializeField] [Range(0.1f, 1f)] [Tooltip("Периодичность появления и исчезновения.")]
        private float _blinkTime = 0.5f;

        [SerializeField] [Tooltip("Спрайт.")] private Renderer _renderer;

        private bool _isInvincible;

        [Header("Стрельба")] [SerializeField] [Range(1, 60)] [Tooltip("Сколько раз можно выстрелить до сброса.")]
        private int _maximumShots = 3;

        [SerializeField] [Range(0f, 10f)] [Tooltip("Через сколько сбросится ограничение на число выстрелов.")]
        private float _fireResetTime = 1f;

        [SerializeField] [Tooltip("Звук выстрела.")]
        private AudioClip _fireSound;

        [SerializeField] [Tooltip("Точка появления пуль.")]
        private GameObject _bulletSpawnPoint;

        private int _shotsMade;
        private ObjectPooler _objectPooler;

        private Camera _camera;

        [SerializeField] [Tooltip("Звук взрыва.")]
        private AudioClip _explosionSound;

        private void Start()
        {
            _camera = Camera.main;

            _objectPooler = GameManager.Instance.SpaceshipBulletsObjectPooler;

            _isInvincible = true;
            InvokeRepeating(nameof(Blink), 0f, _blinkTime);
            Invoke(nameof(StartTakingDamage), _invincibilityTime);

            InvokeRepeating(nameof(ResetFiring), 0f, _fireResetTime);
        }

        private void Blink()
        {
            _renderer.enabled = !_renderer.enabled;
        }

        private void StartTakingDamage()
        {
            CancelInvoke(nameof(Blink));
            _renderer.enabled = true;
            _isInvincible = false;
        }

        private void ResetFiring()
        {
            _shotsMade = -1;
        }

        private void Update()
        {
            HandleFire();

            HandleThrust();

            HandleRotation();
        }

        private void HandleFire()
        {
            KeyCode[] fireKeys = MenuManager.Instance.GetFireKeys();
            if (!(Time.timeScale > 0) || !fireKeys.Any(Input.GetKeyDown) || ++_shotsMade >= _maximumShots)
            {
                return;
            }

            GameObject bullet = _objectPooler.GetObject();
            if (bullet == null)
            {
                return;
            }

            bullet.transform.SetPositionAndRotation(_bulletSpawnPoint.transform.position, transform.rotation);
            bullet.SetActive(true);

            GameManager.Instance.PlaySound(_fireSound);
        }

        private void HandleThrust()
        {
            transform.position = Teleporter.Teleport(transform.position);

            float thrustAmount = Input.GetAxis("VerticalKeyboard");
            if (thrustAmount > 0f && !GameManager.Instance.IsAudioSourcePlaying())
            {
                GameManager.Instance.PlaySound(_thrustSound);
            }

            _velocity += thrustAmount * _acceleration;
            _velocity = Mathf.Clamp(_velocity, 0f, _maximumSpeed);

            transform.Translate(_velocity * Time.deltaTime * transform.up, Space.World);
        }

        private void HandleRotation()
        {
            if (MenuManager.Instance.KeyboardOnlyControls)
            {
                transform.Rotate(0, 0,
                    Input.GetAxis("Horizontal") * _rotationSpeed * _keyboardControlsRotationSpeedAcceleration *
                    Time.deltaTime);
            }
            else
            {
                Vector3 directionToCursor = _camera.ScreenToWorldPoint(Input.mousePosition) - transform.position;
                float angle = Mathf.Atan2(directionToCursor.x, directionToCursor.y) * Mathf.Rad2Deg;
                Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.back);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, _rotationSpeed * Time.deltaTime);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            switch (collision.gameObject.layer)
            {
                case 7: // UFOBullet
                    collision.gameObject.SetActive(false);
                    return;
                case 8: // Asteroid
                    collision.gameObject.GetComponent<Asteroid>().TakeDamage(true);
                    return;
                case 9: // UFO
                    Destroy(collision.gameObject);
                    return;
            }
        }

        public void TakeDamage()
        {
            if (!_isInvincible)
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnSpaceshipDestroyed(_explosionSound);
            }
        }
    }
}
