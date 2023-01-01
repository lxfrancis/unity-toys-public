using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;


[CustomEditor( typeof( LxPropertyDrawerTest ) )]
public class LxPropertyDrawerTestEditor: Editor {
    
    LxPropertyDrawerTest script;
    
    public override void OnInspectorGUI() {
    
        script ??= target as LxPropertyDrawerTest;

        if (script!.indentMe) { EditorGUI.indentLevel++; }
        
        base.OnInspectorGUI();

        if (script.indentMe) { EditorGUI.indentLevel--; }
    }
}
