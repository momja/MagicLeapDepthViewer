using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.Utilities.GenericObjectPool
{
    public class GenericObjectPool : Singleton<GenericObjectPool>
    {

        class Pool
        {
            private Stack<PooledObject> availableObjStack = new Stack<PooledObject>();

            private GameObject poolObjectPrefab;
            private string poolName;
            private int poolSize;

            string InPoolPrefix()
            {
                return "[" + poolName + "] ";
            }
            public Pool(string poolName, GameObject poolObjectPrefab = null)
            {
                this.poolName = poolName;
                this.poolObjectPrefab = poolObjectPrefab;
            }

            private void AddObjectToPool(PooledObject po)
            {
                //add to pool
                po.gameObject.SetActive(false);
                po.gameObject.name = InPoolPrefix() + po.gameObject.name;
                po.gameObject.transform.SetParent(GenericObjectPool.Instance.transform, false);
                availableObjStack.Push(po);
            }

            private PooledObject NewObjectInstance()
            {
                if (poolObjectPrefab == null)
                {
                    return null;
                }
                else
                {
                    GameObject go = (GameObject)GameObject.Instantiate(poolObjectPrefab);
                    return PooledObject.MakePoolable(go, poolName).GetComponent<PooledObject>();
                }

            }


            public PooledObject NextAvailableObject(Transform newParent)
            {
                PooledObject po = null;
                if (availableObjStack.Count > 0)
                {
                    po = availableObjStack.Pop();
                }
                else
                {
                    //increment size var, this is for info purpose only
                    poolSize++;
                    Debug.Log(string.Format("Growing pool {0}. New size: {1}", poolName, poolSize));
                    //create new object
                    po = NewObjectInstance();
                }



                GameObject result = null;
                if (po != null)
                {
                    result = po.gameObject;
                    result.SetActive(true);
                    result.name = result.name.Substring(InPoolPrefix().Length);

                    result.transform.SetParent(newParent, false);
                    result.transform.position = Vector3.zero;
                    result.transform.localScale = Vector3.one;
                    result.transform.rotation = Quaternion.identity;
                    PooledObject.MakePoolable(result,poolName);

                }


                return result?.GetComponent<PooledObject>();
            }


            public void ReturnObjectToPool(PooledObject po)
            {
                AddObjectToPool(po);
            }


        }


        //mapping of pool name vs list
        private Dictionary<string, Pool> poolDictionary = new Dictionary<string, Pool>();

        public T GetObjectFromPool<T>(Transform newParent = null) where T : MonoBehaviour
        {
            var InitMethod = typeof(T).GetMethod("Init");
            if (InitMethod == null) throw new Exception("Type " + nameof(T) + " does not have an Init static method.");
            return GetObjectFromPool(typeof(T).Name, newParent, go => InitMethod.Invoke(null, new object[] { go })).GetComponent<T>();
        }

        public T GetObjectFromPool<T>(Action<GameObject> newObjectProcedure) where T : MonoBehaviour
        {
            return GetObjectFromPool(nameof(T), newObjectProcedure).GetComponent<T>();
        }
        public T GetObjectFromPool<T>(string poolName, Action<GameObject> newObjectProcedure) where T : MonoBehaviour
        {
            return GetObjectFromPool(poolName, newObjectProcedure).GetComponent<T>();
        }

        public T GetObjectFromPool<T>(string poolName, Transform newParent, Action<GameObject> newObjectProcedure) where T : MonoBehaviour
        {
            return GetObjectFromPool(poolName, newParent, newObjectProcedure).GetComponent<T>();
        }

        public GameObject GetObjectFromPool(string poolName, Action<GameObject> newObjectProcedure)
        {
            return GetObjectFromPool(poolName, null, newObjectProcedure);
        }

        public GameObject GetObjectFromPool(string poolName, Transform newParent,  Action<GameObject> newObjectProcedure)
        {
            var result = GetObjectFromPool(poolName, newParent);
            if(result == null)
            {
                result = PooledObject.Create(poolName,newParent);
                newObjectProcedure.Invoke(result);
            }

            return result;
        }


        public GameObject GetObjectFromPool(string poolName, Transform newParent = null)
        {
            PooledObject result = null;

            if (poolDictionary.ContainsKey(poolName))
            {
                Pool pool = poolDictionary[poolName];
                result = pool.NextAvailableObject(newParent);
            }

            return result?.gameObject;
        }

        public void ReturnObjectToPool(GameObject gameObject)
        {
            PooledObject pooledObject = gameObject.GetComponent<PooledObject>();
            if (pooledObject == null)
            {
                Debug.LogError("Unpooled object could not be returned to pool");
                GameObject.Destroy(gameObject);
            } else
            {
                ReturnObjectToPool(pooledObject);
            }
        }

        public void ReturnObjectToPool(PooledObject pooledObject)
        {
            if (poolDictionary.ContainsKey(pooledObject.PoolName))
            {
                Pool pool = poolDictionary[pooledObject.PoolName];
                pool.ReturnObjectToPool(pooledObject);
            }
            else
            {
                Pool pool = new Pool(pooledObject.PoolName, null);
                poolDictionary[pooledObject.PoolName] = pool;
                pool.ReturnObjectToPool(pooledObject);
            }

        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}