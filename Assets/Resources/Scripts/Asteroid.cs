using Unity.Mathematics;
using UnityEngine;

namespace Assets.Resources.Scripts
{
    public class Asteroid : MonoBehaviour
    {
        private bool _didSpawn;

        [HideInInspector] [Tooltip("Скорость астероида.")]
        public float Speed;

        [HideInInspector] public GameManager.AsteroidTypes AsteroidType;

        [SerializeField] [Tooltip("Звук взрыва.")]
        private AudioClip _explosionSound;

        private bool _rewardableDeath;

        [SerializeField] [Range(0, 1000)] [Tooltip("Сколько очков получит игрок за уничтожение астероида.")]
        private int _score;

        [SerializeField]
        [Range(30f, 60f)]
        [Tooltip("Угол, на который от направления движения большого астероида будут отклонены маленькие.")]
        private float _newAsteroidRotationAngle = 45f;

        private void Update()
        {
            transform.position = Teleporter.Teleport(transform.position);

            transform.Translate(Speed * Time.deltaTime * transform.up, Space.World);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            switch (collision.gameObject.layer)
            {
                case 6: // SpaceshipBullet
                    collision.gameObject.SetActive(false);
                    if (_didSpawn || AsteroidType == GameManager.AsteroidTypes.Small)
                    {
                        return;
                    }

                    float newAsteroidsSpeed = GameManager.Instance.NewAsteroidsSpeed();
                    SpawnLesserAsteroid(-_newAsteroidRotationAngle, newAsteroidsSpeed);
                    SpawnLesserAsteroid(_newAsteroidRotationAngle, newAsteroidsSpeed);
                    _didSpawn = true;
                    return;
                case 9: // UFO
                    Destroy(collision.gameObject);
                    return;
                case 10: // Spaceship
                    GameManager.Instance.SpaceshipInstance.TakeDamage();
                    return;
            }
        }

        private void SpawnLesserAsteroid(float angle, float speed)
        {
            GameObject lesserAsteroid;
            Asteroid lesserAsteroidComponent;
            if (AsteroidType == GameManager.AsteroidTypes.Big)
            {
                lesserAsteroid = GameManager.Instance._mediumAsteroidsObjectPooler.GetObject();
                if (lesserAsteroid == null)
                {
                    return;
                }

                lesserAsteroidComponent = lesserAsteroid.GetComponent<Asteroid>();
                lesserAsteroidComponent.AsteroidType = GameManager.AsteroidTypes.Medium;
            }
            else
            {
                lesserAsteroid = GameManager.Instance._smallAsteroidsObjectPooler.GetObject();
                if (lesserAsteroid == null)
                {
                    return;
                }

                lesserAsteroidComponent = lesserAsteroid.GetComponent<Asteroid>();
                lesserAsteroidComponent.AsteroidType = GameManager.AsteroidTypes.Small;
            }

            lesserAsteroidComponent.Speed = speed;
            lesserAsteroid.transform.SetPositionAndRotation(transform.position,
                transform.rotation * Quaternion.Euler(0f, 0f, angle));
            lesserAsteroid.SetActive(true);
        }

        public void TakeDamage(bool rewardNeeded = false)
        {
            _rewardableDeath = rewardNeeded;
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            _didSpawn = false;
            _rewardableDeath = false;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnAsteroidActivated();
            }
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnAsteroidDeactivated(_explosionSound, _rewardableDeath, _score);
            }
        }
    }
}
