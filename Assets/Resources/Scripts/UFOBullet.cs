using UnityEngine;

namespace Assets.Resources.Scripts
{
    public class UFOBullet : Bullet
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {
            switch (collision.gameObject.layer)
            {
                case 6: // SpaceshipBullet
                    collision.gameObject.SetActive(false);
                    return;
                case 10: // Spaceship
                    GameManager.Instance.SpaceshipInstance.TakeDamage();
                    return;
            }
        }

        private void OnEnable()
        {
            if (GameManager.Instance == null || GameManager.Instance.SpaceshipInstance == null)
            {
                gameObject.SetActive(false);
                return;
            }

            transform.rotation = Quaternion.LookRotation(Vector3.forward,
                GameManager.Instance.SpaceshipInstance.transform.position - transform.position);

            ResetValues();
        }
    }
}
