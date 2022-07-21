using UnityEngine;

namespace Assets.Resources.Scripts
{
    public class SpaceshipBullet : Bullet
    {
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

        private void OnEnable()
        {
            if (GameManager.Instance != null)
            {
                ResetValues();
            }
        }
    }
}
