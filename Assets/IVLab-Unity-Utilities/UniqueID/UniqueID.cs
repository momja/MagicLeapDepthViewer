using IVLab.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace IVLab.Utilities
{
    public class UID
    {
        public UID(string uuid)
        {
            Guid = new Guid(uuid);
        }
        public UID(Guid guid)
        {
            Guid = guid;
        }
        public UID()
        {
            Guid = Guid.NewGuid();
        }
        public Guid Guid { get; private set; }
    }
    public class UniqueID : Singleton<UniqueID>
    {
        ConditionalWeakTable<object, UID> idTable;

        private UniqueID()
        {
            idTable = new ConditionalWeakTable<object, UID>();
        }

        public void SetUID(object obj, UID uid)
        {
            idTable.Remove(obj);
            idTable.Add(obj, uid);
        }

        public UID GetUID(object obj)
        {
            return idTable.GetOrCreateValue(obj);
        }

    }

}
