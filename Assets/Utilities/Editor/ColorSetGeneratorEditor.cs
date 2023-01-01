using UnityEditor;

[CustomEditor( typeof( ColorSetGenerator ) )]
public class ColorSetGeneratorEditor: Editor {
    
    ColorSetGenerator script;
    
    public override void OnInspectorGUI() {
    
        script ??= target as ColorSetGenerator;
        
        base.OnInspectorGUI();
        
        EditorGUILayout.HelpBox( script!.outOfGamut ? "Colours out of gamut" : "Colours are in range",
                                 script.outOfGamut ? MessageType.Warning : MessageType.None );
    }
}
