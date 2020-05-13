using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.Utilities.GenericObjectPool
{

    public static class ObjectRecycler
    {
        public static void RecycleChildren(GameObject gameObject, bool recurse = false)
        {
             while(gameObject.transform.childCount > 0)
            {
                Recycle(gameObject.transform.GetChild(0).gameObject);
            }

        }
        public static void Recycle(MonoBehaviour component, bool recurse = true)
        {
            Recycle(component?.gameObject, recurse);
        }
        public static void Recycle(GameObject gameObject, bool recurse = true)
        {
            if (gameObject == null) return;
            if(recurse)
            {
                RecycleChildren(gameObject);
            }
               
            PooledObject pooledObject = gameObject.GetComponent<PooledObject>();

            if(pooledObject != null)
            {
                GenericObjectPool.Instance.ReturnObjectToPool(pooledObject);
            } else
            {
                throw new System.Exception("Can't return an unpoolable object to the pool");
            }
        }
    }
}
