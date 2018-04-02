using UnityEngine;
using UnityEditor;

namespace Lx {

   [CustomPropertyDrawer( typeof( IntRange ) )]
   public class IntRangeDrawer: PropertyDrawer {

      public override void OnGUI( Rect position, SerializedProperty property, GUIContent label ) {
     
         EditorGUI.BeginProperty( position, label, property );
         position              = EditorGUI.PrefixLabel( position, GUIUtility.GetControlID( FocusType.Passive ), label );
         int indent            = EditorGUI.indentLevel;
         EditorGUI.indentLevel = 0;
        
         Rect minValueRect = new Rect( position.x,       position.y, 76, position.height );
         Rect labelRect    = new Rect( position.x + 84,  position.y, 20, position.height );
         Rect maxValueRect = new Rect( position.x + 104, position.y, 76, position.height );
         EditorGUI.PropertyField( minValueRect, property.FindPropertyRelative("min"), GUIContent.none );
         EditorGUI.LabelField( labelRect, "to" );
         EditorGUI.PropertyField( maxValueRect, property.FindPropertyRelative("max"), GUIContent.none );
        
         EditorGUI.indentLevel = indent;
         EditorGUI.EndProperty();
      }
   }

   [CustomPropertyDrawer( typeof( NullableIntRange ) )]
   public class NullableIntRangeDrawer: SerializedNullableDrawer< NullableIntRange > { }

   [CustomPropertyDrawer( typeof( FloatRange ) )]
   public class FloatRangeDrawer: PropertyDrawer {

      public override void OnGUI( Rect position, SerializedProperty property, GUIContent label ) {
     
         EditorGUI.BeginProperty( position, label, property );
         position              = EditorGUI.PrefixLabel( position, GUIUtility.GetControlID( FocusType.Passive ), label );
         int indent            = EditorGUI.indentLevel;
         EditorGUI.indentLevel = 0;
        
         Rect minValueRect = new Rect( position.x,       position.y, 76, position.height );
         Rect labelRect    = new Rect( position.x + 84,  position.y, 20, position.height );
         Rect maxValueRect = new Rect( position.x + 104, position.y, 76, position.height );
        
         EditorGUI.PropertyField( minValueRect, property.FindPropertyRelative("min"), GUIContent.none );
         EditorGUI.LabelField( labelRect, "to" );
         EditorGUI.PropertyField( maxValueRect, property.FindPropertyRelative("max"), GUIContent.none );
        
         EditorGUI.indentLevel = indent;
         EditorGUI.EndProperty();
      }
   }

   [CustomPropertyDrawer( typeof( NullableFloatRange ) )]
   public class NullableFloatRangeDrawer: SerializedNullableDrawer< NullableFloatRange > { }
}