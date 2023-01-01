using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using static UnityEditor.EditorGUI;
using static UnityEditor.EditorGUIUtility;
using Object = UnityEngine.Object;

namespace Lx {

    public abstract class SingleLinePropertyDrawer: PropertyDrawer {

        protected virtual bool controlHasPrefixLabel => false; 

        public override void OnGUI( Rect position, SerializedProperty property, GUIContent label ) {

            (var controlRect, int prevIndent) = EditorUtils.BeginSingleLineProperty( position, label, property,
                                                                                     !controlHasPrefixLabel );
            DrawControl( controlRect, property );
            EditorUtils.EndSingleLineProperty( prevIndent );
        }

        public abstract void DrawControl( Rect position, SerializedProperty property );
    }

    
    public abstract class SimplePropertyDrawer: PropertyDrawer {

        protected virtual bool controlHasPrefixLabel => false; 

        public override void OnGUI( Rect position, SerializedProperty property, GUIContent label ) {
            
            BeginProperty( position, label, property );

            if (label != GUIContent.none) {
                
                if (!controlHasPrefixLabel) {
                    position = PrefixLabel( position, label );
                }
                else {
                    var lineRect    = position;
                    lineRect.height = singleLineHeight;
                
                    Rect labelRect  = lineRect;
                    labelRect.width = labelWidth;
                
                    HandlePrefixLabel( position, labelRect, new GUIContent( label ) );
                    position.xMin += labelWidth;
                }
            }
            

            var indent  = indentLevel;
            indentLevel = 0;
            
            DrawControl( position, property );
            
            indentLevel = indent;
            EndProperty();
        }

        public abstract void DrawControl( Rect position, SerializedProperty property );
    }
    
    
    public class PropertyGroup {

        readonly SerializedProperty[]        properties;
        readonly string                      header;
        readonly int                         space;
        readonly bool                        collapsible;
        readonly Dictionary < string, bool > expandedHeaders;

        bool isExpanded {
            get => !collapsible || expandedHeaders[ header ];
            set => expandedHeaders[ header ?? "this shouldn't happen" ] = value;
        }


        public PropertyGroup( SerializedObject so, int space=0, string header=null,
                              Dictionary< string, bool > expandedHeadersDict=null, params string[] props ) {

            this.header     = !header.IsNullOrWhitespace() ? header : null;
            this.space      = space;
            collapsible     = this.header != null;
            expandedHeaders = expandedHeadersDict;
            properties      = new SerializedProperty[ props.Length ];

            for (var index = 0; index < props.Length; index++) {
                properties[ index ] = so.FindProperty( props[ index ] );
            }
        }


        public PropertyGroup( SerializedProperty sp, int space=0, string header=null,
                              Dictionary< string, bool > expandedHeadersDict=null, params string[] props ) {

            this.header     = !header.IsNullOrWhitespace() ? header : null;
            this.space      = space;
            collapsible     = this.header != null;
            expandedHeaders = expandedHeadersDict;
            properties      = new SerializedProperty[ props.Length ];

            for (var index = 0; index < props.Length; index++) {
                properties[ index ] = sp.FindPropertyRelative( props[ index ] );
            }
        }

            
        public void Draw() {
                
            if (space > 0) { EditorGUILayout.Space( space ); }
            
            if (collapsible) {

                isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup( isExpanded, header );

                if (isExpanded) { indentLevel++; }
            }

            if (isExpanded) {
                    
                foreach (var prop in properties) { EditorGUILayout.PropertyField( prop ); }
                if (collapsible) { indentLevel--; }
            }
                
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }


    public static class EditorUtils {


        /// <summary>Draw a label and serialized property value.</summary>
        public static void DrawLabelledProperty( string label, SerializedProperty property, Rect rect, float labelWidth,
                                                 bool reverse=false ) {

            var (leftRect, rightRect) = Utils.SplitRectHorizontal( rect, labelWidth );

            PrefixLabel( reverse ? rightRect : leftRect, new GUIContent( label ) );
            PropertyField( reverse ? leftRect : rightRect, property, GUIContent.none );
        }


        // note: when using this, GetPropertyHeight must also be overridden to take isExpanded into account
        public static void DrawShiftedProperty( Rect  position, SerializedProperty property, GUIContent label,
                                                float leftShift = 12f ) {

            Rect topLine   = position;
            topLine.height = singleLineHeight;
            label = BeginProperty( position, label, property );

            if (!property.isExpanded) { property.isExpanded = Foldout( topLine, property.isExpanded, label, true ); }

            if (property.isExpanded) {

                position.xMin -= leftShift;
                labelWidth    += leftShift;
                PropertyField( position, property, property.isExpanded );
            }

            EndProperty();
        }


        public static string[] GetPropertyDetailStrings( SerializedProperty property ) => new [] {
            $"name: {property.name}",
            $"type: {property.type}",
            $"depth: {property.depth}",
            $"propertyPath: {property.propertyPath}",
            $"propertyType: {property.propertyType}",
            // ReSharper disable once Unity.NoNullPropagation
            $"serializedObject: {property.serializedObject?.targetObject?.name ?? "??"}", 
            $"editable: {property.editable}",
            $"isExpanded: {property.isExpanded}",
            $"tooltip: {property.tooltip}",
            $"hasChildren: {property.hasChildren}",
            $"hasVisibleChildren: {property.hasVisibleChildren}",
            $"hasMultipleDifferentValues: {property.hasMultipleDifferentValues}",
            $"isAnimated: {property.isAnimated}"
        };


        public static SerializedProperty Find( this SerializedProperty prop, string name )
            => prop.FindPropertyRelative( name );


        public static (Rect controlRect, int indent) BeginSingleLineProperty( Rect position, GUIContent label,
                                                                              SerializedProperty property,
                                                                              bool usePrefixLabel=true ) {

            var propLabel = BeginProperty( position, label, property );

            position.height = singleLineHeight;

            if (label != GUIContent.none) {
                
                if (usePrefixLabel) {
                    position = PrefixLabel( position, propLabel );
                }
                else {
                    Rect labelRect  = position;
                    labelRect.width = labelWidth;
                    LabelField( labelRect, propLabel );
                    position.xMin += labelWidth;
                }
            }

            var indent  = indentLevel;
            indentLevel = 0;

            return (position, indent);
        }


        public static void EndSingleLineProperty( int previousIndent ) {

            indentLevel = previousIndent;
            EndProperty();
        }


        public static List< Rect > LayoutRectGrid( int width, int height ) {

            var lines = new List< Rect >();
            for (int i = 0; i < height; i++) { lines.Add( EditorGUILayout.GetControlRect( false ) ); }
            float[] divisions = new float[ width - 1 ];
            for (int i = 1; i < width; i++) { divisions[ i - 1 ] = (float) i / width; }
            var rects = new List< Rect >();
            foreach (var l in lines) { rects.AddRange( Utils.SplitRectHorizontal( l, divisions, true, 2f ) ); }
            return rects;
        }


        public static void DrawCentredLabelAndData( string label, string data ) {

            var      rects        = Utils.SplitRectHorizontal( EditorGUILayout.GetControlRect(), 0.5f, true, 8f );
            GUIStyle rightAligned = new GUIStyle( EditorStyles.label ) { alignment = TextAnchor.MiddleRight };
            GUI.Label( rects.left,  $"{label}:", rightAligned );
            GUI.Label( rects.right, data,        EditorStyles.boldLabel );
        }


        public static void DrawRangeControl( Rect rect, SerializedProperty minProperty, SerializedProperty maxProperty,
                                             float cellWidth=72, string label=null, float labelWidth=0 ) {

            if (labelWidth > 0.0f && label != null) {
                
                var rects = Utils.SplitRectHorizontal( rect, labelWidth, false );
                rect      = rects.right;
                PrefixLabel( rects.left, new GUIContent( label ) );
            }

            const float toWidth = 20;

            float controlWidth = cellWidth * 2f + toWidth;
            if (rect.width < controlWidth) { cellWidth -= (controlWidth - rect.width) * 0.5f; }

            Rect minRect = new Rect( rect.x,           rect.y, cellWidth,   rect.height );
            Rect toRect  = new Rect( minRect.xMax + 4, rect.y, toWidth - 4, rect.height );
            Rect maxRect = new Rect( toRect.xMax,      rect.y, cellWidth,   rect.height );

            PropertyField( minRect, minProperty, GUIContent.none );
            PrefixLabel( toRect, new GUIContent( "to" ) );
            PropertyField( maxRect, maxProperty, GUIContent.none );
        }


        public static void DrawPropertyDetails( SerializedProperty sp ) {

            foreach (var str in GetPropertyDetailStrings( sp )) {
                EditorGUILayout.SelectableLabel( str, GUILayout.Height( singleLineHeight * 0.8f ) );
            }
        }

        
        /// <summary>Draw a label and two serialized property values representing min and max of a range.</summary>
        public static void DrawRangeProperties( string label, SerializedProperty propertyA, SerializedProperty propertyB,
                                                Rect rect, float labelWidth, float valueWidth ) {

            Rect[] rects = Utils.SplitRectHorizontal( rect, new[] {
                labelWidth + valueWidth + 4, labelWidth + valueWidth * 2 + 32
            }, false, 8 );
            
            DrawLabelledProperty( label, propertyA, rects[ 0 ], labelWidth );
            DrawLabelledProperty( "to", propertyB, rects[ 1 ], 20 );
        }

        
        /// <summary>Find all assets of a given type matching the given partial or exact name.</summary>
        public static T[] FindAssets< T >( string name, bool exactMatch = true ) where T: Object
            => AssetDatabase.FindAssets( $"\"{name}\" t:{typeof( T ).Name}" )
                            .Select( AssetDatabase.GUIDToAssetPath )
                            .Select( AssetDatabase.LoadAssetAtPath< T > )
                            .Where( asset => !exactMatch || asset.name == name ).ToArray();

        
        /// <summary>Find the path to an asset of a given type matching the given partial or exact name.</summary>
        public static string FindAssetPath< T >( string name, bool exactMatch = true ) where T: Object
            => AssetDatabase.FindAssets( $"\"{name}\" t:{typeof( T ).Name}" )
                            .Select( AssetDatabase.GUIDToAssetPath ).FirstOrDefault( n => !exactMatch || n == name );

        
        /// <summary>Find an asset of a given type matching the given partial or exact name.</summary>
        public static T FindAsset< T >( string name, bool exactMatch = true ) where T: Object
            => FindAssets< T >( name, exactMatch ).FirstOrDefault();
    }
}
