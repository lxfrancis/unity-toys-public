using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer( typeof( WeightedItem<> ) )]
public class WeightedItemDrawer: PropertyDrawer {
    
    public override void OnGUI( Rect pos, SerializedProperty property, GUIContent label ) {

        const float weightLabelWidth = 45, weightValueWidth = 50, padding = 4; 

        EditorGUI.BeginProperty( pos, label, property );
        pos = EditorGUI.PrefixLabel( pos, GUIUtility.GetControlID( FocusType.Passive ), label );
        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        Rect mainRect = new Rect( pos.x, pos.y, pos.width - weightLabelWidth - weightValueWidth - padding, pos.height );
        EditorGUI.PropertyField( mainRect, property.FindPropertyRelative( "value" ), GUIContent.none );

        Rect labelRect = new Rect( mainRect.xMax + padding, pos.y, weightLabelWidth, pos.height );
        GUI.Label( labelRect, "Weight" );

        Rect weightRect = new Rect( labelRect.xMax, pos.y, weightValueWidth, pos.height );
        EditorGUI.PropertyField( weightRect, property.FindPropertyRelative( "weight" ), GUIContent.none );
        
        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }
}
