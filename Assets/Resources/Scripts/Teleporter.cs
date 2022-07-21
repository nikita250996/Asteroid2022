using UnityEngine;

namespace Assets.Resources.Scripts
{
    public class Teleporter
    {
        public static Vector3 Teleport(Vector3 oldPosition)
        {
            Vector3 newPosition = oldPosition;

            if (oldPosition.x > GameManager.Instance.GameFieldHalfWidth)
            {
                newPosition.x = oldPosition.x - GameManager.Instance.GameFieldWidth;
            }
            else if (oldPosition.x < -GameManager.Instance.GameFieldHalfWidth)
            {
                newPosition.x = oldPosition.x + GameManager.Instance.GameFieldWidth;
            }

            if (oldPosition.y > GameManager.Instance.GameFieldHalfHeight)
            {
                newPosition.y = oldPosition.y - GameManager.Instance.GameFieldHeight;
            }
            else if (oldPosition.y < -GameManager.Instance.GameFieldHalfHeight)
            {
                newPosition.y = oldPosition.y + GameManager.Instance.GameFieldHeight;
            }

            return newPosition;
        }
    }
}
