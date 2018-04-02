using UnityEngine;
using UnityEditor;

namespace Lx {

   public class SerializedNullableDrawer< T >: PropertyDrawer {

      public override void OnGUI( Rect position, SerializedProperty property, GUIContent label ) {
      
         EditorGUI.BeginProperty( position, label, property );
         position              = EditorGUI.PrefixLabel( position, GUIUtility.GetControlID( FocusType.Passive ), label );
         int indent            = EditorGUI.indentLevel;
         EditorGUI.indentLevel = 0;
        
         Rect hasValueRect = new Rect( position.x,      position.y, 20, 				     position.height );
         Rect valueRect    = new Rect( position.x + 30, position.y, position.width - 30, position.height );
         EditorGUI.PropertyField( hasValueRect, property.FindPropertyRelative("hasValue"), GUIContent.none );
         EditorGUI.PropertyField( valueRect,    property.FindPropertyRelative("value"),    GUIContent.none );
        
         EditorGUI.indentLevel = indent;
         EditorGUI.EndProperty();
      }
   }

   [CustomPropertyDrawer( typeof( NullableInt ) )]
   public class NullableIntDrawer: SerializedNullableDrawer< NullableInt > { }

   [CustomPropertyDrawer( typeof( NullableFloat ) )]
   public class NullableFloatDrawer: SerializedNullableDrawer< NullableFloat > { }

   [CustomPropertyDrawer( typeof( NullableColor ) )]
   public class NullableColorDrawer: SerializedNullableDrawer< NullableColor > { }
}
