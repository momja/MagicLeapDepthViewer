using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.Utilities.GenericObjectPool
{
    public class PooledObject : MonoBehaviour
    {
        public static GameObject Create(string poolName, Transform newParent = null)
        {
            GameObject go = new GameObject(poolName + " object");
            go.transform.SetParent(newParent, false);
            return MakePoolable(go, poolName).gameObject;
        }
        public static bool IsPoolable(GameObject gameObject)
        {
            PooledObject pooledObject = gameObject.GetComponent<PooledObject>();
            return pooledObject != null;
        }
        public static GameObject MakePoolable(GameObject gameObject, string poolName)
        {
            PooledObject pooledObject = gameObject.GetComponent<PooledObject>();
            if (pooledObject != null)
            {
                if (pooledObject.PoolName != poolName)
                {
                    Debug.LogError("Object is already assigned to pool " + pooledObject.PoolName + ", can't assign to pool " + poolName);
                }
            } else
            {
                pooledObject = gameObject.AddComponent<PooledObject>();
                pooledObject.PoolName = poolName;
            }
            return pooledObject.gameObject;
        }


        public string PoolName
        {
            get;
            private set;
        }

        [SerializeField]
        string poolName;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void OnGUI()
        {
            poolName = PoolName;

        }
    }

}
