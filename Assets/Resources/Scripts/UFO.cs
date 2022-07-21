using UnityEngine;

namespace Assets.Resources.Scripts
{
    public class UFO : MonoBehaviour
    {
        [SerializeField] [Range(8f, 20f)] [Tooltip("Время, за которое НЛО преодолеет экран.")]
        private float _moveTime = 10f;

        private float _speed;

        private ObjectPooler _objectPooler;

        [SerializeField] [Range(0.5f, 2.2f)] [Tooltip("Минимальное время «перезарядки» между выстрелами.")]
        private float _minimumTimeToShoot = 2f;

        [SerializeField] [Range(2.5f, 8f)] [Tooltip("Максимальное время «перезарядки» между выстрелами.")]
        private float _maximumTimeToShoot = 5f;

        [SerializeField] [Tooltip("Звук выстрела.")]
        private AudioClip _fireSound;

        private bool _peacefulDeath;

        [SerializeField] [Tooltip("Звук взрыва.")]
        private AudioClip _explosionSound;

        private bool _rewardableDeath;

        [SerializeField] [Range(0, 2000)] [Tooltip("Сколько очков получит игрок за уничтожение НЛО.")]
        private int _score = 200;

        private void Start()
        {
            _objectPooler = GameManager.Instance.UFOBulletsObjectPooler;
            _speed = GameManager.Instance.GameFieldWidth / _moveTime;
            SetRotation();
            InvokeRepeating(nameof(Shoot), _minimumTimeToShoot, Random.Range(_minimumTimeToShoot, _maximumTimeToShoot));
        }

        private void SetRotation()
        {
            float z = Mathf.Abs(transform.position.x + GameManager.Instance.GameFieldHalfWidth) < 0.1f ? -90f : 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, z);
        }

        private void Shoot()
        {
            if (GameManager.Instance.SpaceshipInstance == null)
            {
                return;
            }

            GameObject bullet = _objectPooler.GetObject();
            if (bullet == null)
            {
                return;
            }

            bullet.transform.position = transform.position;
            bullet.SetActive(true);
            GameManager.Instance.PlaySound(_fireSound);
        }

        private void Update()
        {
            transform.Translate(_speed * Time.deltaTime * transform.up, Space.World);
            if (Mathf.Abs(transform.position.x) < GameManager.Instance.GameFieldHalfWidth)
            {
                return;
            }

            _peacefulDeath = true;
            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            switch (collision.gameObject.layer)
            {
                case 6: // SpaceshipBullet
                    _rewardableDeath = true;
                    collision.gameObject.SetActive(false);
                    return;
                case 8: // Asteroid
                    collision.gameObject.GetComponent<Asteroid>().TakeDamage();
                    return;
                case 10: // Spaceship
                    _rewardableDeath = true;
                    GameManager.Instance.SpaceshipInstance.TakeDamage();
                    return;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnUFODestroyed(_peacefulDeath, _explosionSound, _rewardableDeath, _score);
            }
        }
    }
}
