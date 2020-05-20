using IVLab.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;


public class ChangeStamp : IComparable
{
    private static long m_counter = 0;

    private static long GetNewChangeStampValue()
    {
        long s = System.Threading.Interlocked.Increment(ref m_counter);
        return s;
    }
    public static ChangeStamp GetNewChangeStamp()
    {
        return new ChangeStamp { stampValue = GetNewChangeStampValue() };
    }

    public static ChangeStamp GetNullTimestamp()
    {
        return new ChangeStamp { stampValue = 0 };
    }
    public void Update()
    {
        stampValue = GetNewChangeStampValue();
    }
    public int CompareTo(object obj)
    {
        if (obj is ChangeStamp)
        {
            return stampValue.CompareTo((obj as ChangeStamp).stampValue);

        }
        else
        {
            return 0;
        }
    }

    private long stampValue;

}

public class ChangeTracker : Singleton<ChangeTracker>
{
   

    ConditionalWeakTable<object, ChangeStamp> stampTable;

    private ChangeTracker()
    {
        stampTable = new ConditionalWeakTable<object, ChangeStamp>();
    }

    public void Stamp(object obj)
    {
        if (obj == null) return;
        lock (obj)
        {
            var stamp = stampTable.GetOrCreateValue(obj);
            stamp.Update();
        }
    }
    public bool IsNewerThan(object newer, object older)
    {
        ChangeStamp newerStamp, olderStamp;
        if (newer == null || !stampTable.TryGetValue(newer, out newerStamp)) newerStamp = ChangeStamp.GetNullTimestamp();
        if (older == null || !stampTable.TryGetValue(older, out olderStamp)) olderStamp = ChangeStamp.GetNullTimestamp();

        return newerStamp.CompareTo(olderStamp) > 0;
    }
}

namespace Extentions
{
    public static class WeakReferenceExtentions
    {
        public static void UpdateTimeStamp(this object self)
        {
            ChangeTracker.Instance.Stamp(self);
        }

        public static bool IsNewerThan(this object self, object obj2)
        {
            return ChangeTracker.Instance.IsNewerThan(self, obj2);
        }
    }
}
