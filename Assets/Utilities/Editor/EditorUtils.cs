using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Lx {

   public static class EditorUtils {
  
      /// <summary>Draw a label and serialized property value.</summary>
      public static void DrawProperty( string label, SerializedProperty property, Rect rect, float labelWidth,
                                       bool reverse=false ) {
     
         Rect leftRect  = new Rect( rect.x,              rect.y, labelWidth,              rect.height );
         Rect rightRect = new Rect( rect.x + labelWidth, rect.y, rect.width - labelWidth, rect.height );
         
         EditorGUI.LabelField( reverse ? rightRect : leftRect, label );
         EditorGUI.PropertyField( reverse ? leftRect : rightRect, property, GUIContent.none );
      }
     
      /// <summary>Draw a label and two serialized property values representing min and max of a range.</summary>
      public static void DrawRangeProperties( string label, SerializedProperty propertyA, SerializedProperty propertyB,
                                              Rect rect, float labelWidth, float valueWidth ) {
     
         Rect[] rects = Utils.SplitRectHorizontal( rect, new[] { labelWidth + valueWidth + 4,
                                                                 labelWidth + valueWidth * 2 + 32 }, false, 8 );
         DrawProperty( label, propertyA, rects[ 0 ], labelWidth );
         DrawProperty( "to",  propertyB, rects[ 1 ], 20 );
      }
     
      /// <summary>Find all assets of a given type matching the given partial or exact name.</summary>
      public static T[] FindAssets< T >( string name, bool exactMatch=true ) where T: Object {
     
         return AssetDatabase.FindAssets( "\"" + name + "\" t:" + typeof( T ).Name )
                             .Select( guid => AssetDatabase.GUIDToAssetPath( guid ) )
                             .Select( path => AssetDatabase.LoadAssetAtPath< T >( path ) )
                             .Where( asset => exactMatch ? asset.name == name : true ).ToArray();
      }
      
      /// <summary>Find an assets of a given type matching the given partial or exact name.</summary>
      public static T FindAsset< T >( string name, bool exactMatch=true ) where T: Object {
    
         return FindAssets< T >( name, exactMatch ).FirstOrDefault();
      }
   }
}
