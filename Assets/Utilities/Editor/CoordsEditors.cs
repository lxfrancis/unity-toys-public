using UnityEngine;
using UnityEditor;

namespace Lx {

    [CustomPropertyDrawer( typeof( Coord2 ) )]
    public class Coord2Drawer: SingleLinePropertyDrawer {

        const float cellWidth    = 72f;
        const float controlWidth = cellWidth * 2f;

        static readonly float[] divisionPoints = { cellWidth, cellWidth * 2f };

        protected override bool controlHasPrefixLabel => true;

        
        public override void DrawControl( Rect position, SerializedProperty property ) {

            Rect[] rects = Utils.SplitRectHorizontal( position, divisionPoints, false, 5, controlWidth, true );

            EditorUtils.DrawLabelledProperty( "X", property.FindPropertyRelative( "x" ), rects[ 0 ], 12 );
            EditorUtils.DrawLabelledProperty( "Y", property.FindPropertyRelative( "y" ), rects[ 1 ], 12 );
        }
    }

    
    [CustomPropertyDrawer( typeof( Coord3 ) )]
    public class Coord3Drawer: SingleLinePropertyDrawer {

        const float cellWidth    = 72f;
        const float controlWidth = cellWidth * 3f;

        static readonly float[] divisionPoints = { cellWidth, cellWidth * 2f, cellWidth * 3f };

        protected override bool controlHasPrefixLabel => true;


        public override void DrawControl( Rect position, SerializedProperty property ) {

            Rect[] rects = Utils.SplitRectHorizontal( position, divisionPoints, false, 5, controlWidth, true );

            EditorUtils.DrawLabelledProperty( "X", property.FindPropertyRelative( "x" ), rects[ 0 ], 12 );
            EditorUtils.DrawLabelledProperty( "Y", property.FindPropertyRelative( "y" ), rects[ 1 ], 12 );
            EditorUtils.DrawLabelledProperty( "Z", property.FindPropertyRelative( "z" ), rects[ 2 ], 12 );
        }
    }

    
    [CustomPropertyDrawer( typeof( Coord2Range ) )]
    public class Coord2RangeDrawer: SingleLinePropertyDrawer {

        const float cellWidth      = 120f;
        const float gap            = 10f;
        const float axisLabelWidth = 12f;

        static readonly float[] divs = { cellWidth, cellWidth * 2f };

        protected override bool controlHasPrefixLabel => true;

        
        public override void DrawControl( Rect position, SerializedProperty property ) {
            
            var rects = Utils.SplitRectHorizontal( position, divs, false, gap, cellWidth * 2f, true );
            
            DrawAxis( rects[ 0 ], "x" );
            DrawAxis( rects[ 1 ], "y" );

            void DrawAxis( Rect rect, string axis ) {

                var (labelRect, rangeRect) = Utils.SplitRectHorizontal( rect, axisLabelWidth );

                EditorGUI.HandlePrefixLabel( position, labelRect, new GUIContent( axis.ToUpper() ) );
                EditorUtils.DrawRangeControl( rangeRect, property.FindPropertyRelative( $"{axis}Min" ),
                                              property.FindPropertyRelative( $"{axis}Max" ) );
            }
        }
    }

    
    [CustomPropertyDrawer( typeof( Coord3Range ) )]
    public class Coord3RangeDrawer: PropertyDrawer {

        const float axisLabelWidth = 12f;

        public override float GetPropertyHeight( SerializedProperty property, GUIContent label )
            => EditorGUIUtility.singleLineHeight * 3f + 2f;

        public override void OnGUI( Rect position, SerializedProperty property, GUIContent label ) {

            EditorGUI.BeginProperty( position, label, property );
            position = EditorGUI.PrefixLabel( position, GUIUtility.GetControlID( FocusType.Passive ), label );
            
            int indent            = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            
            var lineRect    = position;
            lineRect.height = EditorGUIUtility.singleLineHeight;

            DrawAxis( "x" );
            DrawAxis( "y" );
            DrawAxis( "z" );

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();

            void DrawAxis( string axis ) {

                EditorUtils.DrawRangeControl( lineRect, property.FindPropertyRelative( $"{axis}Min" ),
                                              property.FindPropertyRelative( $"{axis}Max" ),
                                              label: axis.ToUpper(), labelWidth: axisLabelWidth );

                lineRect.y += lineRect.height + 1f;
            }
        }
    }
}
