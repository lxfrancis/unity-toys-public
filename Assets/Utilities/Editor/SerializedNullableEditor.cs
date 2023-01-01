using UnityEngine;
using UnityEditor;

namespace Lx {

    [CustomPropertyDrawer( typeof( SerializedNullable<> ), true )]
    public class SerializedNullableDrawer: SimplePropertyDrawer {

        const float checkboxWidth = 20f;

        public override void DrawControl( Rect rect, SerializedProperty property ) {

            var rects         = Utils.SplitRectHorizontal( rect, checkboxWidth, gapSize: 4f );
            rects.left.height = EditorGUIUtility.singleLineHeight;
            
            EditorGUI.PropertyField( rects.left,  property.FindPropertyRelative( "hasValue" ), GUIContent.none );
            EditorGUI.PropertyField( rects.right, property.FindPropertyRelative( "value" ),    GUIContent.none );
        }

        public override float GetPropertyHeight( SerializedProperty property, GUIContent label )
            => EditorGUI.GetPropertyHeight( property.FindPropertyRelative( "value" ) );
    }
}
