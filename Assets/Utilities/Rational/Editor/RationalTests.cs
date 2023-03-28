using System;
using UnityEditor;
using UnityEngine;
using static Lx.RationalTests.TestOperation;
using static UnityEngine.Debug;
using Random = UnityEngine.Random;


namespace Lx {
    
    
    public static class RationalTests {


        internal enum TestOperation { Equal, Unequal, Less, Greater, LessEqual, GreaterEqual }

        static TestOperation randomTestOp
            => (TestOperation) Random.Range( 0, Enum.GetValues( typeof( TestOperation ) ).Length );
        
        
        enum TestType { Rational, Float, Double, Int, Short }

        static TestType randomTestType
            => (TestType) Random.Range( 0, Enum.GetValues( typeof( TestType ) ).Length );
        
        
        enum FractionalTestType { Float, Double, Decimal }

        static FractionalTestType randomFractionalType
            => (FractionalTestType) Random.Range( 0, Enum.GetValues( typeof( FractionalTestType ) ).Length );


        static int randomNumerator           => Random.Range( -12, 13 );
        static int randomDenominator         => Random.Range(   0, 13 );
        static int randomPositiveDenominator => Random.Range(   1, 13 );

        static float randomWeightedFloat => Random.value * Random.Range( -12f, 12f );

        static Rational randomRational => new( randomNumerator, randomDenominator );
        
        
        [MenuItem( "Lx/Rational type tests/Arithmetic operations (randomised values)" )]
        public static void TestRationalArithmetic() {

            for (int i = 0; i < 1000; i++) {

                var (a, b) = (randomRational, randomRational);

                Log( Random.Range( 0, 4 ) switch {
                    0 => $"{a} + {b} = {a + b}",
                    1 => $"{a} - {b} = {a - b}",
                    2 => $"{a} * {b} = {a * b}",
                    3 => $"{a} / {b} = {a / b}",
                    _ => throw new ArgumentOutOfRangeException()
                } );
            }
        }
        
        
        [MenuItem( "Lx/Rational type tests/Conversions to non-integer numeric types (randomised values)" )]
        public static void TestConversions() {

            for (int i = 0; i < 100; i++) {

                Rational rational = new Rational( randomNumerator, randomPositiveDenominator );

                if (Random.value < 0.5f) switch (randomFractionalType) {

                    case FractionalTestType.Float: {
                        float f = (float) rational.Numerator / rational.Denominator;
                        Log( $"{rational} == {f}? {rational == f}" );
                    } break;

                    case FractionalTestType.Double: {
                        double d = (double) rational.Numerator / rational.Denominator;
                        Log( $"{rational} == {d}? {rational == d}" );
                    } break;
                
                    case FractionalTestType.Decimal: {
                        decimal m = (decimal) rational.Numerator / rational.Denominator;
                        Log( $"{rational} == {m}? {rational == m}" );
                    } break;

                    default: { throw new ArgumentOutOfRangeException(); }
                }
                else switch (randomFractionalType) {

                    case FractionalTestType.Float: {
                        float f = (float) rational.Numerator / rational.Denominator;
                        Log( $"(float) {rational} == {f}? {(float) rational == f}" );
                    } break;

                    case FractionalTestType.Double: {
                        double d = (double) rational.Numerator / rational.Denominator;
                        Log( $"(double) {rational} == {d}? {(double) rational == d}" );
                    } break;
                
                    case FractionalTestType.Decimal: {
                        decimal m = (decimal) rational.Numerator / rational.Denominator;
                        Log( $"(decimal) {rational} == {m}? {(decimal) rational == m}" );
                    } break;

                    default: { throw new ArgumentOutOfRangeException(); }
                }
            }
        }


        [MenuItem( "Lx/Rational type tests/Equality and comparisons (randomised values)" )]
        public static void TestRational() {

            for (int i = 0; i < 1000; i++) {

                Rational r = randomRational;

                switch (randomTestType) {

                    case TestType.Rational: {
                        
                        Rational other = randomRational;
                        
                        Log( randomTestOp switch {
                            Equal        => $"{r} == {other}: {r == other}",
                            Unequal      => $"{r} != {other}: {r != other}",
                            Less         => $"{r} <  {other}: {r <  other}",
                            Greater      => $"{r} >  {other}: {r >  other}",
                            LessEqual    => $"{r} <= {other}: {r <= other}",
                            GreaterEqual => $"{r} >= {other}: {r >= other}",
                            _            => throw new ArgumentOutOfRangeException()
                        } );
                    } break;

                    case TestType.Float: {
                        
                        float f = randomWeightedFloat;
                        
                        Log( randomTestOp switch {
                            Equal        => $"{r} == float {f}: {r == f}",
                            Unequal      => $"{r} != float {f}: {r != f}",
                            Less         => $"{r} <  float {f}: {r <  f}",
                            Greater      => $"{r} >  float {f}: {r >  f}",
                            LessEqual    => $"{r} <= float {f}: {r <= f}",
                            GreaterEqual => $"{r} >= float {f}: {r >= f}",
                            _            => throw new ArgumentOutOfRangeException()
                        } );
                    } break;

                    case TestType.Double: {
                        
                        double d = randomWeightedFloat;
                        
                        Log( randomTestOp switch {
                            Equal        => $"{r} == double {d}: {r == d}",
                            Unequal      => $"{r} != double {d}: {r != d}",
                            Less         => $"{r} <  double {d}: {r <  d}",
                            Greater      => $"{r} >  double {d}: {r >  d}",
                            LessEqual    => $"{r} <= double {d}: {r <= d}",
                            GreaterEqual => $"{r} >= double {d}: {r >= d}",
                            _            => throw new ArgumentOutOfRangeException()
                        } );
                    } break;

                    case TestType.Int: {

                        int n = randomNumerator;
                        
                        Log( randomTestOp switch {
                            Equal        => $"{r} == int {n}: {r == n}",
                            Unequal      => $"{r} != int {n}: {r != n}",
                            Less         => $"{r} <  int {n}: {r <  n}",
                            Greater      => $"{r} >  int {n}: {r >  n}",
                            LessEqual    => $"{r} <= int {n}: {r <= n}",
                            GreaterEqual => $"{r} >= int {n}: {r >= n}",
                            _            => throw new ArgumentOutOfRangeException()
                        } );
                    } break;

                    case TestType.Short: {

                        short s = (short) randomNumerator;
                        
                        Log( randomTestOp switch {
                            Equal        => $"{r} == short {s}: {r == s}",
                            Unequal      => $"{r} != short {s}: {r != s}",
                            Less         => $"{r} <  short {s}: {r <  s}",
                            Greater      => $"{r} >  short {s}: {r >  s}",
                            LessEqual    => $"{r} <= short {s}: {r <= s}",
                            GreaterEqual => $"{r} >= short {s}: {r >= s}",
                            _            => throw new ArgumentOutOfRangeException()
                        } );
                    } break;

                    default: { throw new ArgumentOutOfRangeException(); }
                }
            }
        }
        
        
        [MenuItem( "Lx/Rational type tests/Random value generators" )]
        public static void TestRationalRandom() {

            for (int i = 0; i < 100; i++) { Log( Rational.Random );   }
            for (int i = 0; i < 100; i++) { Log( Rational.Random01 ); }
        }
    }
}
