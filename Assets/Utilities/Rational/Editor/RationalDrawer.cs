using UnityEngine;
using UnityEditor;


namespace Lx {

    
    [CustomPropertyDrawer( typeof( Rational ) )]
    public class RationalDrawer: PropertyDrawer {


        public override void OnGUI( Rect position, SerializedProperty property, GUIContent label ) {

            SerializedProperty nProp = property.FindPropertyRelative( "numerator" );
            SerializedProperty dProp = property.FindPropertyRelative( "denominator" );

            string currStr  = new Rational( nProp.intValue, dProp.intValue ).ToStringNoParens();
            Rect   rect     = EditorGUI.PrefixLabel( position, EditorGUI.BeginProperty( position, label, property ) );
            string strValue = EditorGUI.TextField( rect, GUIContent.none, currStr );

            if (Rational.TryParse( strValue, out Rational r )) {
                
                nProp.intValue = r.Numerator;
                dProp.intValue = r.Denominator;
            }
            
            EditorGUI.EndProperty();
        }
    }
}
