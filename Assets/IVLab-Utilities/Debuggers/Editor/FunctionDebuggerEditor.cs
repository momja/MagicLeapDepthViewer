using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Linq;

[CustomEditor(typeof(FunctionDebugger))]
public class FunctionDebuggerEditor : Editor
{

    private FunctionDebugger script;

    private void OnEnable()
    {
        script = (FunctionDebugger)serializedObject.targetObject;
    }

    public override void OnInspectorGUI()
    {
        //DrawDefaultInspector();
        var monobehaviours = script.GetComponents<MonoBehaviour>();
        int index = 0;
        int i = 0;
        foreach(var mb in monobehaviours)
        {
            var methods = mb.GetType().GetMethods(
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.GetParameters().Length == 0 && m.IsDefined(typeof(FunctionDebuggerAttribute)));

            foreach (var m in methods)
            {
                if (GUILayout.Button(mb.GetType().Name + "." + m.Name + "()", GUILayout.ExpandWidth(false)))
                {
                    m.Invoke(mb, null);
                }
            }

        }

        


    }
}
