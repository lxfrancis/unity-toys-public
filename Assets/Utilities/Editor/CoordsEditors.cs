using UnityEngine;
using UnityEditor;

namespace Lx {

   [CustomPropertyDrawer( typeof( Coord2 ) )]
   public class Coord2Drawer: PropertyDrawer {

	   public override void OnGUI( Rect position, SerializedProperty property, GUIContent label ) {

		   EditorGUI.BeginProperty( position, label, property );
		   position              = EditorGUI.PrefixLabel( position, GUIUtility.GetControlID( FocusType.Passive ), label );
		   int indent            = EditorGUI.indentLevel;
		   EditorGUI.indentLevel = 0;
		   Rect[] rects 		    = Utils.SplitRectHorizontal( position, new[] { 80f, 160f }, false );

		   EditorUtils.DrawProperty( "X", property.FindPropertyRelative("x"), rects[ 0 ], 20 );
		   EditorUtils.DrawProperty( "Y", property.FindPropertyRelative("y"), rects[ 1 ], 20 );

		   EditorGUI.indentLevel = indent;
		   EditorGUI.EndProperty();
	   }
   }

   [CustomPropertyDrawer( typeof( NullableCoord2 ) )]
   public class NullableCoord2Drawer: SerializedNullableDrawer< NullableCoord2 > { }

   [CustomPropertyDrawer( typeof( Coord3 ) )]
   public class Coord3Drawer: PropertyDrawer {

	   public override void OnGUI( Rect position, SerializedProperty property, GUIContent label ) {

		   EditorGUI.BeginProperty( position, label, property );
		   position              = EditorGUI.PrefixLabel( position, GUIUtility.GetControlID( FocusType.Passive ), label );
		   int indent            = EditorGUI.indentLevel;
		   EditorGUI.indentLevel = 0;
		   Rect[] rects 		    = Utils.SplitRectHorizontal( position, new[] { 80f, 160f, 240f }, false );

		   EditorUtils.DrawProperty( "X", property.FindPropertyRelative("x"), rects[ 0 ], 20 );
		   EditorUtils.DrawProperty( "Y", property.FindPropertyRelative("y"), rects[ 1 ], 20 );
		   EditorUtils.DrawProperty( "Z", property.FindPropertyRelative("z"), rects[ 2 ], 20 );

		   EditorGUI.indentLevel = indent;
		   EditorGUI.EndProperty();
	   }
   }

   [CustomPropertyDrawer( typeof( NullableCoord3 ) )]
   public class NullableCoord3Drawer: SerializedNullableDrawer< NullableCoord3 > { }

   [CustomPropertyDrawer( typeof( Coord2Range ) )]
   public class Coord2RangeDrawer: PropertyDrawer {

      public override void OnGUI( Rect position, SerializedProperty property, GUIContent label ) {
     
         EditorGUI.BeginProperty( position, label, property );
         position              = EditorGUI.PrefixLabel( position, GUIUtility.GetControlID( FocusType.Passive ), label );
         int indent            = EditorGUI.indentLevel;
         EditorGUI.indentLevel = 0;
         
         int x = 0;
         Rect labelRect = new Rect( position.x + x,      position.y, 12, position.height );
         Rect valueRect = new Rect( position.x + x + 12, position.y, 32, position.height );
         EditorGUI.LabelField( labelRect, "X" );
         EditorGUI.PropertyField( valueRect, property.FindPropertyRelative("xMin"), GUIContent.none );
         
         x += 48;
         labelRect = new Rect( position.x + x,      position.y, 15, position.height );
         valueRect = new Rect( position.x + x + 15, position.y, 32, position.height );
         EditorGUI.LabelField( labelRect, "to" );
         EditorGUI.PropertyField( valueRect, property.FindPropertyRelative("xMax"), GUIContent.none );
         
         x += 60;
         labelRect = new Rect( position.x + x,      position.y, 12, position.height );
         valueRect = new Rect( position.x + x + 12, position.y, 32, position.height );
         EditorGUI.LabelField( labelRect, "Y" );
         EditorGUI.PropertyField( valueRect, property.FindPropertyRelative("yMin"), GUIContent.none );
         
         x += 48;
         labelRect = new Rect( position.x + x,      position.y, 15, position.height );
         valueRect = new Rect( position.x + x + 15, position.y, 32, position.height );
         EditorGUI.LabelField( labelRect, "to" );
         EditorGUI.PropertyField( valueRect, property.FindPropertyRelative("yMax"), GUIContent.none );
         
         EditorGUI.indentLevel = indent;
         EditorGUI.EndProperty();
      }
   }

   [CustomPropertyDrawer( typeof( NullableCoord2Range ) )]
   public class NullableCoord2RangeDrawer: SerializedNullableDrawer< NullableCoord2Range > { }
}
