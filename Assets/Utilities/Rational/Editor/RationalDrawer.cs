using System;
using UnityEngine;
using UnityEditor;
using Random = UnityEngine.Random;


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


    public static class RationalTests {
        
#if UNITY_EDITOR
        [MenuItem( "Lx/Test Rational arithmetic operations" )]
        public static void TestRationalArithmetic() {

            for (int i = 0; i < 1000; i++) {

                var a = new Rational( Random.Range( -12, 13 ), Random.Range( 0, 13 ) );
                var b = new Rational( Random.Range( -12, 13 ), Random.Range( 0, 13 ) );

                Debug.Log( Random.Range( 0, 4 ) switch {
                    0 => $"{a} + {b} = {a + b}",
                    1 => $"{a} - {b} = {a - b}",
                    2 => $"{a} * {b} = {a * b}",
                    3 => $"{a} / {b} = {a / b}",
                    _ => throw new ArgumentOutOfRangeException()
                } );
            }
        }
        
        
        [MenuItem( "Lx/Test Rational random generators" )]
        public static void TestRationalRandom() {

            for (int i = 0; i < 100; i++) { Debug.Log( Rational.Random );   }
            for (int i = 0; i < 100; i++) { Debug.Log( Rational.Random01 ); }
        }
        
        
        enum TestFractionalType { Float, Double, Decimal }
        
        
        [MenuItem( "Lx/Test Rational conversions" )]
        public static void TestConversions() {

            for (int i = 0; i < 100; i++) {

                var r  = new Rational( Random.Range( -12, 13 ), Random.Range( 1, 13 ) );
                var ft = (TestFractionalType) Random.Range( 0, 3 );

                if (Random.value < .5f) {

                    switch (ft) {

                        case TestFractionalType.Float: {
                            var fl = (float) r.Numerator / r.Denominator;
                            Debug.Log( $"{r} == {fl}? {r == fl}" );
                        } break;

                        case TestFractionalType.Double: {
                            var dl = (double) r.Numerator / r.Denominator;
                            Debug.Log( $"{r} == {dl}? {r == dl}" );
                        } break;
                    
                        case TestFractionalType.Decimal: {
                            var m = (decimal) r.Numerator / r.Denominator;
                            Debug.Log( $"{r} == {m}? {r == m}" );
                        } break;

                        default: { throw new ArgumentOutOfRangeException(); }
                    }
                }
                else {
                    
                    switch (ft) {

                        case TestFractionalType.Float: {
                            var fl = (float) r.Numerator / r.Denominator;
                            Debug.Log( $"(float) {r} == {fl}? {(float) r == fl}" );
                        } break;

                        case TestFractionalType.Double: {
                            var dl = (double) r.Numerator / r.Denominator;
                            Debug.Log( $"(double) {r} == {dl}? {(double) r == dl}" );
                        } break;
                    
                        case TestFractionalType.Decimal: {
                            var m = (decimal) r.Numerator / r.Denominator;
                            Debug.Log( $"(decimal) {r} == {m}? {(decimal) r == m}" );
                        } break;

                        default: { throw new ArgumentOutOfRangeException(); }
                    }
                }
            }
        }
        
        
        enum TestType { Rational, Float, Double, Int, Short }

        enum TestOperation { Equal, Unequal, Less, Greater, LessEqual, GreaterEqual }


        [MenuItem( "Lx/Test Rational equality and comparisons" )]
        public static void TestRational() {

            for (int i = 0; i < 1000; i++) {

                var r    = new Rational( Random.Range( -12, 13 ), Random.Range( 0, 13 ) );
                var type = (TestType) Random.Range( 0, 5 );
                var op   = (TestOperation) Random.Range( 0, 6 );

                switch (type) {

                    case TestType.Rational: {
                        
                        var or = new Rational( Random.Range( -12, 13 ), Random.Range( 0, 13 ) );
                        
                        switch (op) {
                            case TestOperation.Equal:        { Debug.Log( $"{r} == {or}: {r == or}" ); } break;
                            case TestOperation.Unequal:      { Debug.Log( $"{r} != {or}: {r != or}" ); } break;
                            case TestOperation.Less:         { Debug.Log( $"{r} < {or}: {r < or}" ); } break;
                            case TestOperation.Greater:      { Debug.Log( $"{r} > {or}: {r > or}" ); } break;
                            case TestOperation.LessEqual:    { Debug.Log( $"{r} <= {or}: {r <= or}" ); } break;
                            case TestOperation.GreaterEqual: { Debug.Log( $"{r} >= {or}: {r >= or}" ); } break;
                            default:                         { throw new ArgumentOutOfRangeException(); }
                        }
                    } break;

                    case TestType.Float: {
                        
                        float f = Random.value * Random.Range( -12f, 12f );
                        
                        switch (op) {
                            case TestOperation.Equal:        { Debug.Log( $"{r} == float {f}: {r == f}" ); } break;
                            case TestOperation.Unequal:      { Debug.Log( $"{r} != float {f}: {r != f}" ); } break;
                            case TestOperation.Less:         { Debug.Log( $"{r} < float {f}: {r < f}" ); } break;
                            case TestOperation.Greater:      { Debug.Log( $"{r} > float {f}: {r > f}" ); } break;
                            case TestOperation.LessEqual:    { Debug.Log( $"{r} <= float {f}: {r <= f}" ); } break;
                            case TestOperation.GreaterEqual: { Debug.Log( $"{r} >= float {f}: {r >= f}" ); } break;
                            default:                         { throw new ArgumentOutOfRangeException(); }
                        }
                    } break;

                    case TestType.Double: {
                        
                        double d = Random.value * Random.Range( -12f, 12f );
                        
                        switch (op) {
                            case TestOperation.Equal:        { Debug.Log( $"{r} == double {d}: {r == d}" ); } break;
                            case TestOperation.Unequal:      { Debug.Log( $"{r} != double {d}: {r != d}" ); } break;
                            case TestOperation.Less:         { Debug.Log( $"{r} < double {d}: {r < d}" ); } break;
                            case TestOperation.Greater:      { Debug.Log( $"{r} > double {d}: {r > d}" ); } break;
                            case TestOperation.LessEqual:    { Debug.Log( $"{r} <= double {d}: {r <= d}" ); } break;
                            case TestOperation.GreaterEqual: { Debug.Log( $"{r} >= double {d}: {r >= d}" ); } break;
                            default:                         { throw new ArgumentOutOfRangeException(); }
                        }
                    } break;

                    case TestType.Int: {

                        int n = Random.Range( 0, 13 );
                        
                        switch (op) {
                            case TestOperation.Equal:        { Debug.Log( $"{r} == int {n}: {r == n}" ); } break;
                            case TestOperation.Unequal:      { Debug.Log( $"{r} != int {n}: {r != n}" ); } break;
                            case TestOperation.Less:         { Debug.Log( $"{r} < int {n}: {r < n}" ); } break;
                            case TestOperation.Greater:      { Debug.Log( $"{r} > int {n}: {r > n}" ); } break;
                            case TestOperation.LessEqual:    { Debug.Log( $"{r} <= int {n}: {r <= n}" ); } break;
                            case TestOperation.GreaterEqual: { Debug.Log( $"{r} >= int {n}: {r >= n}" ); } break;
                            default:                         { throw new ArgumentOutOfRangeException(); }
                        }
                    } break;

                    case TestType.Short: {

                        short s = (short) Random.Range( 0, 13 );
                        
                        switch (op) {
                            case TestOperation.Equal:        { Debug.Log( $"{r} == short {s}: {r == s}" ); } break;
                            case TestOperation.Unequal:      { Debug.Log( $"{r} != short {s}: {r != s}" ); } break;
                            case TestOperation.Less:         { Debug.Log( $"{r} < short {s}: {r < s}" ); } break;
                            case TestOperation.Greater:      { Debug.Log( $"{r} > short {s}: {r > s}" ); } break;
                            case TestOperation.LessEqual:    { Debug.Log( $"{r} <= short {s}: {r <= s}" ); } break;
                            case TestOperation.GreaterEqual: { Debug.Log( $"{r} >= short {s}: {r >= s}" ); } break;
                            default:                         { throw new ArgumentOutOfRangeException(); }
                        }
                    } break;

                    default: { throw new ArgumentOutOfRangeException(); }
                }
            }
        }
#endif
    }
}
