using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class DependencyGraph : MonoBehaviour
{

    ConditionalWeakTable<object, ChangeStamp> stampTable;

    private DependencyGraph()
    {
        stampTable = new ConditionalWeakTable<object, ChangeStamp>();
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
