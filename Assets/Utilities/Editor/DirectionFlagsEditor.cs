﻿using UnityEngine;
using UnityEditor;

namespace Lx {
   
   [CustomPropertyDrawer( typeof( DirectionFlags ) )]
   public class DirectionFlagsDrawer: PropertyDrawer {

      public override void OnGUI( Rect position, SerializedProperty property, GUIContent label ) {
     
         EditorGUI.BeginProperty( position, label, property );
         position              = EditorGUI.PrefixLabel( position, GUIUtility.GetControlID( FocusType.Passive ), label );
         int indent            = EditorGUI.indentLevel;
         EditorGUI.indentLevel = 0;
         Rect[] rects 		    = Utils.SplitRectHorizontal( position, new[] { 0.25f, 0.5f, 0.75f } );
        
         EditorUtils.DrawProperty( "North", property.FindPropertyRelative("north"), rects[ 0 ], 16, true );
         EditorUtils.DrawProperty( "East",  property.FindPropertyRelative("east"),  rects[ 1 ], 16, true );
         EditorUtils.DrawProperty( "South", property.FindPropertyRelative("south"), rects[ 2 ], 16, true );
         EditorUtils.DrawProperty( "West",  property.FindPropertyRelative("west"),  rects[ 3 ], 16, true );
        
         EditorGUI.indentLevel = indent;
         EditorGUI.EndProperty();
      }
   }

   [CustomPropertyDrawer( typeof( NullableCardinalDir ) )]
   public class NullableCardinalDirDrawer: SerializedNullableDrawer< NullableCardinalDir > { }
}