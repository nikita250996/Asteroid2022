using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Resources.Scripts
{
    public class ObjectPooler : MonoBehaviour
    {
        [SerializeField] [Tooltip("Объект для повторного использования.")]
        private GameObject _objectToPool;

        [SerializeField] [Tooltip("Как много таких объектов должно быть создано сразу.")]
        private int _amountToPool;

        [SerializeField] [Tooltip("Можно ли расширять пул.")]
        private bool _expandable = true;

        private readonly List<GameObject> _pooledObjects = new();

        private void Awake()
        {
            for (int i = 0; i < _amountToPool; ++i)
            {
                AddObject();
            }
        }

        private GameObject AddObject()
        {
            GameObject objectToPool = Instantiate(_objectToPool);
            objectToPool.SetActive(false);
            _pooledObjects.Add(objectToPool);

            return objectToPool;
        }

        public GameObject GetObject()
        {
            foreach (GameObject pooledObject in _pooledObjects.Where(pooledObject => !pooledObject.activeInHierarchy))
            {
                return pooledObject;
            }

            return _expandable ? AddObject() : null;
        }
    }
}
