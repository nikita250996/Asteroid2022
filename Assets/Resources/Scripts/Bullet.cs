using UnityEngine;

namespace Assets.Resources.Scripts
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField] [Range(4f, 10f)] [Tooltip("Скорость пули.")]
        private float _speed = 6f;

        private Vector3 _previousPosition;

        private float _distanceTravelled;

        private void Update()
        {
            HandleTeleportation();

            transform.Translate(_speed * Time.deltaTime * transform.up, Space.World);
        }

        private void HandleTeleportation()
        {
            Vector3 teleportedPosition = Teleporter.Teleport(transform.position);
            float teleportedDistance = Vector3.Distance(transform.position, teleportedPosition);

            transform.position = teleportedPosition;
            _distanceTravelled += Vector3.Distance(transform.position, _previousPosition) - teleportedDistance;
            _previousPosition = transform.position;

            if (_distanceTravelled > GameManager.Instance.GameFieldWidth)
            {
                gameObject.SetActive(false);
            }
        }

        protected void ResetValues()
        {
            _previousPosition = transform.position;
            _distanceTravelled = 0f;
        }
    }
}
