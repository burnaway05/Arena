using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MultiFPS
{

    public class ObjectPooler : MonoBehaviour
    {
        public static ObjectPooler Instance { private set; get; }

        [SerializeField] PoolFamily[] _pools;

        List<ObjectPool> _gamePools = new List<ObjectPool>();

        void Awake()
        {
            Instance = this;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
        {
            //spawn pools for maps
            if (SceneManager.GetActiveScene().buildIndex == 0) return;

            for (int familyID = 0; familyID < _pools.Length; familyID++)
            {
                PoolFamily family = _pools[familyID];
                for (int i = 0; i < family.pools.Length; i++)
                {
                    _gamePools.Add(CreatePool(family.pools[i]));
                }
            }
        }

        ObjectPool CreatePool(ObjectPool objectPool)
        {
            if (!objectPool.PoolPrefab) return null;

            objectPool.ObjectsInPool = new PooledObject[objectPool.NumberOfObjects];

            for (int i = 0; i < objectPool.NumberOfObjects; i++)
            {
                PooledObject newObj = Instantiate(objectPool.PoolPrefab).GetComponent<PooledObject>();

                objectPool.ObjectsInPool.SetValue(newObj, i);

                newObj.OnObjectInstantiated();
            }

            return objectPool;
        }

        public PooledObject SpawnObjectFromFamily(string familyName, string poolID, Vector3 position, Quaternion rotation)
        {
            PooledObject spawnedObjectFromPool = null;
            for (int familyID = 0; familyID < _pools.Length; familyID++)
            {
                if (_pools[familyID].PoolFamilyName != familyName) continue;

                PoolFamily family = _pools[familyID];

                for (int i = 0; i < family.pools.Length; i++)
                {
                    if (family.pools[i].PoolName != poolID) continue;
                    spawnedObjectFromPool = family.pools[i].ReturnObject();
                    spawnedObjectFromPool.transform.SetPositionAndRotation(position, rotation);
                    break;
                }
            }

            return spawnedObjectFromPool;
        }

        public PooledObject SpawnObjectFromFamily(int familyID, int poolID, Vector3 position, Quaternion rotation)
        {
            PooledObject spawnedObjectFromPool = _pools[familyID].pools[poolID].ReturnObject();

            spawnedObjectFromPool.transform.SetPositionAndRotation(position, rotation);
            return spawnedObjectFromPool;
        }

        public PooledObject SpawnObject(int poolID, Vector3 position, Quaternion rotation)
        {
            PooledObject spawnedObjectFromPool = _gamePools[poolID].ReturnObject();

            spawnedObjectFromPool.transform.SetPositionAndRotation(position, rotation);
            return spawnedObjectFromPool;
        }

        public int GetFamilyPoolID(string familyName)
          {
              for (int i = 0; i < _pools.Length; i++)
              {
                  if (_pools[i].PoolFamilyName != familyName) continue;

                  return i;
              }
              return -1;
          }

        /* public int GetPoolID(int familyID, string poolName)
         {
             for (int i = 0; i < _pools[familyID].pools.Length; i++)
             {
                 if (_pools[familyID].pools[i].PoolName != poolName) continue;

                 return i;
             }
             return -1;
         }*/

        public int GetPoolID(string assetName)
        {
            for (int i = 0; i < _gamePools.Count; i++)
            {
                if (_gamePools[i].PoolPrefab.name != assetName) continue;

                return i;
            }
            return -1;
        }
    }
    [System.Serializable]
    public class PoolFamily
    {
        public string PoolFamilyName;
        public ObjectPool[] pools;
    }

    [System.Serializable]
    public class ObjectPool
    {
        public string PoolName = "Pool Name";
        public GameObject PoolPrefab;
        public int NumberOfObjects = 10;
        private int lastUsedObjectID = -1;

        [HideInInspector] public PooledObject[] ObjectsInPool;

        public PooledObject ReturnObject()
        {
            lastUsedObjectID++;

            if (lastUsedObjectID >= NumberOfObjects)
                lastUsedObjectID = 0;

            PooledObject selectedObj = ObjectsInPool[lastUsedObjectID];
            selectedObj.OnObjectReused();

            return selectedObj;
        }
    }

}