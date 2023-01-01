using Lx;
using UnityEngine;
using UnityEditor;


[CustomPropertyDrawer( typeof( Motion ) )]
public class MotionDrawer: SingleLinePropertyDrawer {

    const float durationWidth = 56f, curveStart = 80f;

    public override void DrawControl( Rect position, SerializedProperty property ) {

        var rects = Utils.SplitRectHorizontal( position, new []{ durationWidth, curveStart }, false, 2f );
            
        EditorGUI.PropertyField( rects[ 2 ], property.Find( nameof( Motion.curve    ) ), GUIContent.none );
        EditorGUI.HandlePrefixLabel( position, rects[ 1 ], new GUIContent( "sec" ) );
        EditorGUI.PropertyField( rects[ 0 ], property.Find( nameof( Motion.duration ) ), GUIContent.none );
    }
}


[CustomPropertyDrawer( typeof( Pair<,> ) )]
public class PairDrawer: SingleLinePropertyDrawer {

    const float leftValWidth = 60f;

    public override void DrawControl( Rect position, SerializedProperty property ) {
        
        var rects = Utils.SplitRectHorizontal( position, leftValWidth, false, 3f );

        EditorGUI.PropertyField( rects.left,  property.Find( nameof( Pair< int, int >.leftVal  ) ), GUIContent.none );
        EditorGUI.PropertyField( rects.right, property.Find( nameof( Pair< int, int >.rightVal ) ), GUIContent.none );
    }
}


[CustomPropertyDrawer( typeof( InOutPair<> ) ), CustomPropertyDrawer( typeof( DownUpPair<> ) )]
public class InOutPairDrawer: PropertyDrawer {

    const float smallLabelWidth = 40f;

    static readonly float lineGap = EditorGUIUtility.standardVerticalSpacing * 0.5f;


    public override void OnGUI( Rect position, SerializedProperty property, GUIContent label ) {
        
        (string aName, string bName) = property.type.Contains( "InOut" ) ? ("in", "out") : ("down", "up");
        var (a, b)                   = (property.Find( aName ), property.Find( bName ));

        int  oldIndentLevel = EditorGUI.indentLevel;
        Rect mainLabelRect  = position;
        mainLabelRect.width = EditorGUIUtility.labelWidth - smallLabelWidth * 0.5f;
        
        GUIContent mainLabel = EditorGUI.BeginProperty( position, label, property ); {

            EditorGUI.LabelField( mainLabelRect, mainLabel );
            EditorGUI.indentLevel = 0;

            position.height = EditorGUIUtility.singleLineHeight;
            position.y     += lineGap;
            position.xMin   = mainLabelRect.xMax;

            GUIContent aLabel = EditorGUI.BeginProperty( position, null, a ); {
                float oldLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = smallLabelWidth;
                EditorGUI.PropertyField( position, a, aLabel );
                EditorGUIUtility.labelWidth = oldLabelWidth;
            } EditorGUI.EndProperty();

            position.y += EditorGUI.GetPropertyHeight( a ) + lineGap;

            GUIContent bLabel = EditorGUI.BeginProperty( position, null, b ); {
                float oldLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = smallLabelWidth;
                EditorGUI.PropertyField( position, b, bLabel );
                EditorGUIUtility.labelWidth = oldLabelWidth;
            } EditorGUI.EndProperty();

        } EditorGUI.EndProperty();
        
        EditorGUI.indentLevel = oldIndentLevel;
    }

    
    public override float GetPropertyHeight( SerializedProperty property, GUIContent label ) {
        
        string aName = property.type.Contains( "InOut" ) ? "in" : "down";
        var    a     = property.Find( aName );
        return EditorGUI.GetPropertyHeight( a ) * 2f + lineGap * 3f;
    }
}


[CustomPropertyDrawer( typeof( RectTransformPair ) )]
public class RectTransformPairDrawer: SingleLinePropertyDrawer {

    const float labelWidth = 12f, gapSize = 6f;

    protected override bool controlHasPrefixLabel => true;

    
    public override void DrawControl( Rect position, SerializedProperty property ) {
        
        var rects  = Utils.SplitRectHorizontal( position,    0.5f, true, gapSize );
        var aRects = Utils.SplitRectHorizontal( rects.left,  labelWidth );
        var bRects = Utils.SplitRectHorizontal( rects.right, labelWidth );
            
        EditorGUI.PrefixLabel( aRects.left, new GUIContent( "A" ) );
        EditorGUI.PropertyField( aRects.right, property.Find( "a" ), GUIContent.none );
        
        EditorGUI.PrefixLabel( bRects.left, new GUIContent( "B" ) );
        EditorGUI.PropertyField( bRects.right, property.Find( "b" ), GUIContent.none );
    }
}
