using System;


namespace Lx {


    public static class RationalExtensions {

        public static Rational Over( this int numerator, int denominator ) => new( numerator, denominator );
    }
    

    [Serializable]
    public struct Rational: IEquatable< Rational >, IComparable, IComparable< Rational >, IConvertible {


        public static Rational PositiveInfinity = new(  1, 0, true );
        public static Rational NegativeInfinity = new( -1, 0, true );
        public static Rational NaN              = new(  0, 0, true );
        public static Rational Zero             = new(  0, 1, true );


#if UNITY_5_3_OR_NEWER
        [UnityEngine.SerializeField]
#endif
        int numerator, denominator;

        public int Numerator   => numerator;
        public int Denominator => denominator;
        
        
#region Constructors & fraction simplification
        
        Rational( int numerator, int denominator, bool checkless ) {  // :) programming sucks

            this.numerator   = numerator;
            this.denominator = denominator;
        }


        public Rational( int numerator, int denominator ) {

            if (numerator == 0 && denominator == 0) {  // NaN

                this.numerator   = 0;
                this.denominator = 0;
                return;
            }

            if (numerator == 0) {  // zero

                this.numerator   = 0;
                this.denominator = 1;
                return;
            }

            if (denominator == 0) {  // infinity

                this.numerator   = numerator > 0 ? 1 : -1;
                this.denominator = 0;
                return;
            }

            if (denominator < 0) { // make sure the numerator carries the sign and we don't have two negatives
                
                numerator   = -numerator;
                denominator = -denominator;
            }

            int gcf = GCF( numerator, denominator );

            this.numerator   = numerator   / gcf;
            this.denominator = denominator / gcf;
        }


        // Euclid's algorithm. slow!
        static int Euclid( int larger, int smaller ) {

            while (true) {

                larger -= smaller;
                if (larger == smaller) { return larger; }
                if (larger < smaller) { (larger, smaller) = (smaller, larger); }
            }
        }
        
        
        static int IntegerDivision( int larger, int smaller ) {

            while (true) {

                int remainder = larger % smaller;
                if (remainder == 0) { return smaller; }
                (smaller, larger) = (remainder, smaller);
            }
        }


        // Integer division
        static int GCF( int a, int b ) {

            if (a == 1 || b == 1) { return 1; }

            if (a < 0) { a = -a; }
            if (b < 0) { b = -b; }

            if (a == b) { return a; }

            (int smaller, int larger) = a < b ? (a, b) : (b, a);

            return IntegerDivision( larger, smaller );
        }


        public Rational( int n ) {

            numerator   = n;
            denominator = 1;
        }
        
#endregion

        
#region String parsing

        public static Rational Parse( string str ) {

            if (str == null) { throw new ArgumentNullException( nameof( str ) ); }

            if (str.Length == 0) { throw new ArgumentException( "String is empty", nameof( str ) ); }
            
            string[] tokens = str.Split( '/' );

            if (tokens.Length == 2
                && int.TryParse( tokens[ 0 ].Trim(), out int a )
                && int.TryParse( tokens[ 1 ].Trim(), out int b )) {
                
                return new Rational( a, b ); // get simplified components
            }

            if (tokens.Length == 1) {

                if (int.TryParse( str.Trim(), out int i )) { return new Rational( i, 1, true ); }
                if (double.TryParse( str.Trim(), out double d )) { return (Rational) d; } // help
            }

            throw new FormatException( $"Could not parse {str} as a Rational" );
        }


        public static bool TryParse( string str, out Rational r ) {

            if (string.IsNullOrWhiteSpace( str )) {

                r = Zero;
                return false;
            }
            
            string[] tokens = str.Split( '/' );

            if (tokens.Length == 2
                && int.TryParse( tokens[ 0 ].Trim(), out int a )
                && int.TryParse( tokens[ 1 ].Trim(), out int b )) {
                
                r = new Rational( a, b ); // get simplified components
                return true;
            }

            if (tokens.Length == 1) {

                if (int.TryParse( str.Trim(), out int i )) {

                    r = new Rational( i, 1, true );
                    return true;
                }
                
                if (double.TryParse( str.Trim(), out double d )) {

                    r = (Rational) d; // help
                    return true;
                }
            }

            r = Zero;
            return false;
        }
        
#endregion


        public static bool IsInteger         ( Rational r ) => r is { denominator: 1 };
        public static bool IsInfinity        ( Rational r ) => r is { denominator: 0, numerator  : not 0 };
        public static bool IsPositiveInfinity( Rational r ) => r is { denominator: 0, numerator  : >   0 };
        public static bool IsNegativeInfinity( Rational r ) => r is { denominator: 0, numerator  : <   0 };
        public static bool IsNaN             ( Rational r ) => r is { denominator: 0, numerator  :     0 };
        public static bool IsZero            ( Rational r ) => r is { numerator  : 0, denominator: not 0 };

        
        public bool isInteger          => denominator == 1;
        public bool isInfinity         => denominator == 0 && numerator != 0;
        public bool isPositiveInfinity => denominator == 0 && numerator  > 0;
        public bool isNegativeInfinity => denominator == 0 && numerator  < 0;
        public bool isNaN              => denominator == 0 && numerator == 0;
        public bool isZero             => denominator != 0 && numerator == 0;

        
#region Approximations to floating point values

        public static Rational ClosestOrUnderTolerance( float value, int max=1000000,
                                                        float relativeTolerance=0.000001f ) {

            if (float.IsPositiveInfinity( value )) { return PositiveInfinity; }
            if (float.IsNegativeInfinity( value )) { return NegativeInfinity; }
            if (float.IsNaN             ( value )) { return NaN;              }

            if (value == 0f) { return Zero; }

            if (value > int.MaxValue) { return new Rational( int.MaxValue, 1 ); }
            if (value < int.MinValue) { return new Rational( int.MinValue, 1 ); }

            int   closestNum = int.MaxValue, closestDenom = int.MinValue;
            float smallestError = float.PositiveInfinity;

            for (int a = 1; a <= max; a++) {

                float bFrac = a / value;

                int bFloor = (int) Math.Floor( bFrac );
                int bCeil  = (int) Math.Ceiling( bFrac );

                Rational candidate = default;

                if (check( bFloor )) { return candidate; }
                if (check( bCeil  )) { return candidate; }

                bool check( int bInt ) {
                    
                    float AoverBerror = Math.Abs( (float) a / bInt - value );

                    if (AoverBerror < relativeTolerance) {
                        
                        candidate = new Rational( a, bInt );
                        return true;
                    }

                    if (AoverBerror < smallestError) {

                        smallestError = AoverBerror;
                        closestNum    = a;
                        closestDenom  = bInt;
                    }

                    float BoverAerror = Math.Abs( (float) bInt / a - value );

                    if (BoverAerror < relativeTolerance) {
                        
                        candidate = new Rational( bInt, a );
                        return true;
                    }

                    if (BoverAerror < smallestError) {

                        smallestError = BoverAerror;
                        closestNum    = bInt;
                        closestDenom  = a;
                    }

                    return false;
                }
            }

            if (Math.Abs( (int) value - value ) < smallestError) { return new Rational( (int) value, 1 ); }
            return new Rational( closestNum, closestDenom );
        }


        public static Rational ClosestOrUnderTolerance( double value, int max=1000000,
                                                        double relativeTolerance=0.000000000001 ) {

            if (double.IsPositiveInfinity( value )) { return PositiveInfinity; }
            if (double.IsNegativeInfinity( value )) { return NegativeInfinity; }
            if (double.IsNaN             ( value )) { return NaN;              }

            if (value == 0f) { return Zero; }

            if (value > int.MaxValue) { return new Rational( int.MaxValue, 1 ); }
            if (value < int.MinValue) { return new Rational( int.MinValue, 1 ); }

            int    closestNum    = int.MaxValue, closestDenom = int.MinValue;
            double smallestError = double.PositiveInfinity;

            for (int a = 1; a <= max; a++) {

                double bFrac = a / value;

                int bFloor = (int) Math.Floor( bFrac );
                int bCeil  = (int) Math.Ceiling( bFrac );

                Rational candidate = default;

                if (check( bFloor )) { return candidate; }
                if (check( bCeil  )) { return candidate; }

                bool check( int bInt ) {
                    
                    double AoverBerror = Math.Abs( (double) a / bInt - value );

                    if (AoverBerror < relativeTolerance) {
                        
                        candidate = new Rational( a, bInt );
                        return true;
                    }

                    if (AoverBerror < smallestError) {

                        smallestError = AoverBerror;
                        closestNum    = a;
                        closestDenom  = bInt;
                    }

                    double BoverAerror = Math.Abs( (double) bInt / a - value );

                    if (BoverAerror < relativeTolerance) {
                        
                        candidate = new Rational( bInt, a );
                        return true;
                    }

                    if (BoverAerror < smallestError) {

                        smallestError = BoverAerror;
                        closestNum    = bInt;
                        closestDenom  = a;
                    }

                    return false;
                }
            }

            if (Math.Abs( (int) value - value ) < smallestError) { return new Rational( (int) value, 1 ); }
            return new Rational( closestNum, closestDenom );
        }
        
#endregion


        public static Rational Random {
            get {
                int t = UnityEngine.Random.Range( int.MinValue, int.MaxValue );
                int d = UnityEngine.Random.Range( 1,            int.MaxValue );
                return new Rational( t, d );
            }
        }


        public static Rational Random01 {
            get {
                int d = UnityEngine.Random.Range( 1, int.MaxValue );
                int t = UnityEngine.Random.Range( 1, d + 1        );
                return new Rational( t, d );
            }
        }


        public static implicit operator Rational( int n ) => new( n );

        
#region Explicit casts

        public static explicit operator Rational( float  f ) => ClosestOrUnderTolerance( f ); // yick

        public static explicit operator Rational( double d ) => ClosestOrUnderTolerance( d );


        public static explicit operator decimal( Rational r ) => r.numerator == 0 ? 0m : r.denominator switch {
            1 => r.numerator,
            _ => (decimal) r.numerator / r.denominator
        };


        public static explicit operator float( Rational r ) => r.numerator switch {
            0 when r.denominator == 0 => float.NaN,
            0                         => 0f,
            _ => r.denominator switch {
                0 => r.numerator > 0 ? float.PositiveInfinity : r.numerator < 0 ? float.NegativeInfinity : float.NaN,
                1 => r.numerator,
                _ => (float) ((double) r.numerator / r.denominator)
            }
        };


        public static explicit operator double( Rational r ) => r.numerator switch {
            0 when r.denominator == 0 => double.NaN,
            0                         => 0,
            _ => r.denominator switch {
                0 => r.numerator > 0 ? double.PositiveInfinity : r.numerator < 0 ? double.NegativeInfinity : double.NaN,
                1 => r.numerator,
                _ => (double) r.numerator / r.denominator
            }
        };


        static DivideByZeroException infiniteException => new( "Rational value is infinite" );


        public static explicit operator int( Rational r ) => r.numerator == 0 ? 0 : r.denominator switch {
            0 => throw infiniteException,
            1 => r.numerator,
            _ => r.numerator / r.denominator
        };


        public static explicit operator uint( Rational r ) => r.numerator == 0 ? 0 : r.denominator switch {
            0 => throw infiniteException,
            1 => (uint) r.denominator,
            _ => (uint) r.numerator / (uint) r.denominator
        };


        public static explicit operator long( Rational r ) => r.numerator == 0 ? 0 : r.denominator switch {
            0 => throw infiniteException,
            1 => r.numerator,
            _ => (long) r.numerator / r.denominator
        };


        public static explicit operator ulong( Rational r ) => r.numerator   == 0 ? 0 : r.denominator switch {
            0 => throw infiniteException,
            1 => (ulong) r.denominator,
            _ => (ulong) r.numerator / (ulong) r.denominator
        };

#endregion

        
#region Arithmetic operators

        public static Rational operator +( Rational l, Rational r )
            => new ( l.numerator * r.denominator + r.numerator * l.denominator, l.denominator * r.denominator );


        public static Rational operator -( Rational l, Rational r )
            => new ( l.numerator * r.denominator - r.numerator * l.denominator, l.denominator * r.denominator );


        public static Rational operator *( Rational l, Rational r ) => new( l.numerator   * r.numerator,
                                                                            l.denominator * r.denominator );

        public static Rational operator /( Rational l, Rational r ) => new( l.numerator   * r.denominator,
                                                                            l.denominator * r.numerator );


        public static Rational operator *( Rational r, int s ) => new( r.numerator * s, r.denominator );
        
        public static Rational operator *( int s, Rational r ) => new( r.numerator * s, r.denominator );
        

        public static Rational operator /( Rational r, int s ) => new( r.numerator, r.denominator * s );

        public static Rational operator /( int s, Rational r ) => new( r.denominator * s, r.numerator );
        

        public static Rational operator +( Rational r, int n ) => new( r.numerator + n * r.denominator, r.denominator );
        
        public static Rational operator +( int n, Rational r ) => new( r.numerator + n * r.denominator, r.denominator );
        

        public static Rational operator -( Rational r, int n ) => new( r.numerator - n * r.denominator, r.denominator );

        public static Rational operator -( int n, Rational r ) => new( n * r.denominator - r.numerator, r.denominator );
        
#endregion


#region Equality and comparison operators

        public static bool operator ==( Rational l, Rational r ) =>  l.Equals( r );
        public static bool operator !=( Rational l, Rational r ) => !l.Equals( r );

        public static bool operator > ( Rational l, Rational r ) => l.CompareTo( r ) ==  1;
        public static bool operator < ( Rational l, Rational r ) => l.CompareTo( r ) == -1;

        public static bool operator >=( Rational l, Rational r ) => l.CompareTo( r ) >=  0;
        public static bool operator <=( Rational l, Rational r ) => l.CompareTo( r ) <=  0;
        

        public static bool operator ==( Rational r, float   s ) => (float) r == s;
        public static bool operator !=( Rational r, float   s ) => (float) r != s;
        
        public static bool operator ==( Rational r, double  s ) => (double) r == s;
        public static bool operator !=( Rational r, double  s ) => (double) r != s;
        
        public static bool operator ==( Rational r, decimal s ) => (decimal) r == s;
        public static bool operator !=( Rational r, decimal s ) => (decimal) r != s;
        
        public static bool operator ==( Rational r, int     n ) => r.denominator == 1 && r.numerator == n;
        public static bool operator !=( Rational r, int     n ) => r.denominator != 1 || r.numerator != n;
        
        public static bool operator ==( Rational r, object  o ) => r.Equals( o );
        public static bool operator !=( Rational r, object  o ) => r.Equals( o );
        

        public static bool operator ==( float   s, Rational r ) => (float) r == s;
        public static bool operator !=( float   s, Rational r ) => (float) r != s;
         
        public static bool operator ==( double  s, Rational r ) => (double) r == s;
        public static bool operator !=( double  s, Rational r ) => (double) r != s;
        
        public static bool operator ==( decimal s, Rational r ) => (decimal) r == s;
        public static bool operator !=( decimal s, Rational r ) => (decimal) r != s;
        
        public static bool operator ==( int     n, Rational r ) => r.denominator == 1 && r.numerator == n;
        public static bool operator !=( int     n, Rational r ) => r.denominator != 1 || r.numerator != n;
        
        public static bool operator ==( object  o, Rational r ) => r.Equals( o );
        public static bool operator !=( object  o, Rational r ) => r.Equals( o );
        

        public static bool operator > ( Rational r, object   o ) => r.CompareTo( o ) ==  1;
        public static bool operator < ( Rational r, object   o ) => r.CompareTo( o ) == -1;
  
        public static bool operator >=( Rational r, object   o ) => r.CompareTo( o ) >=  0;
        public static bool operator <=( Rational r, object   o ) => r.CompareTo( o ) <=  0;
          
        public static bool operator > ( object   o, Rational r ) => r.CompareTo( o ) == -1;
        public static bool operator < ( object   o, Rational r ) => r.CompareTo( o ) ==  1;
           
        public static bool operator >=( object   o, Rational r ) => r.CompareTo( o ) <=  0;
        public static bool operator <=( object   o, Rational r ) => r.CompareTo( o ) >=  0;
        
#endregion
        
        
#region Equality and comparison logic

        public bool Equals( Rational other ) => numerator switch {
            0 => denominator == 0 == (other.denominator == 0),
            _ => denominator switch {
                0 => numerator > 0 == (other.numerator == 0),
                _ => numerator == other.numerator && denominator == other.denominator
            }
        };


        public override bool Equals( object obj ) => obj switch {
            Rational r => Equals( r ),
            uint and > int.MaxValue => false,
            uint u => denominator == 1 && u == numerator,
            long l => l switch {
                > int.MaxValue => false,
                < int.MinValue => false,
                _              => denominator == 1 && l == numerator
            },
            ulong and > int.MaxValue => false,
            ulong   u => denominator == 1 && (int) u == numerator,
            decimal m => ((decimal) this).Equals( m ),
            double  d => ((double)  this).Equals( d ),
            float   f => ((float)   this).Equals( f ),
            int     i => denominator == 1 && i == numerator,
            short   s => denominator == 1 && s == numerator,
            ushort  s => denominator == 1 && s == numerator,
            sbyte   b => denominator == 1 && b == numerator,
            byte    b => denominator == 1 && b == numerator,
            _         => throw new InvalidOperationException()
        };
        

        public int CompareTo( Rational other ) {

            if (isNaN)       { return other.isNaN ? 0 : -1; }
            if (other.isNaN) { return 1; }
            
            if (isPositiveInfinity)       { return other.isPositiveInfinity ? 0 :  1; }
            if (other.isPositiveInfinity) { return -1; }
            
            if (isNegativeInfinity)       { return other.isNegativeInfinity ? 0 : -1; }
            if (other.isNegativeInfinity) { return 1; }

            if (isZero)       { return -other.numerator.CompareTo( 0 ); }
            if (other.isZero) { return        numerator.CompareTo( 0 ); }

            return Equals( other ) ? 0 : ((double) this).CompareTo( (double) other );
        }


        public int CompareTo( object obj ) => obj switch {
            Rational r => CompareTo( r ),
            uint and > int.MaxValue => -1,
            uint u => CompareTo( new Rational( (int) u, 1 ) ),
            long l => l switch {
                > int.MaxValue => -1,
                < int.MinValue =>  1,
                _              => CompareTo( new Rational( (int) l, 1 ) )
            },
            ulong and > int.MaxValue => -1,
            ulong   u => CompareTo( new Rational( (int) u, 1 ) ),
            decimal m => ((decimal) this).CompareTo( m ),
            double  d => ((double)  this).CompareTo( d ),
            float   f => ((float)   this).CompareTo( f ),
            int     i => CompareTo( new Rational( i, 1 ) ),
            short   s => CompareTo( new Rational( s, 1 ) ),
            ushort  s => CompareTo( new Rational( s, 1 ) ),
            sbyte   b => CompareTo( new Rational( b, 1 ) ),
            byte    b => CompareTo( new Rational( b, 1 ) ),
            _         => throw new InvalidOperationException()
        };


        public override int GetHashCode() => HashCode.Combine( numerator, denominator );
        
#endregion


        public override string ToString() {
            
            if (isPositiveInfinity) { return "∞";   }
            if (isNegativeInfinity) { return "-∞";  }
            
            if (isZero) { return "0";   }
            if (isNaN)  { return "NaN"; }

            if (denominator == 1) { return numerator.ToString(); }
            
            return numerator < 0 ? $"-({-numerator} / {denominator})" : $"({numerator} / {denominator})";
        }


        public string ToStringNoParens() {
            
            if (isPositiveInfinity) { return "∞";   }
            if (isNegativeInfinity) { return "-∞";  }
            
            if (isZero) { return "0";   }
            if (isNaN)  { return "NaN"; }

            if (denominator == 1) { return numerator.ToString(); }
            
            return numerator < 0 ? $"-{-numerator} / {denominator}" : $"{numerator} / {denominator}";
        }


#region IConvertible implementation

        public string ToString( IFormatProvider provider ) => ToString();

        public TypeCode GetTypeCode() => TypeCode.Object;

        public byte    ToByte   ( IFormatProvider provider ) => (byte)   (int) this;
        public char    ToChar   ( IFormatProvider provider ) => (char)   (int) this;
        public decimal ToDecimal( IFormatProvider provider ) => (decimal)      this;
        public double  ToDouble ( IFormatProvider provider ) => (double)       this;
        public short   ToInt16  ( IFormatProvider provider ) => (short)  (int) this;
        public int     ToInt32  ( IFormatProvider provider ) => (int)          this;
        public long    ToInt64  ( IFormatProvider provider ) => (long)         this;
        public sbyte   ToSByte  ( IFormatProvider provider ) => (sbyte)        this;
        public float   ToSingle ( IFormatProvider provider ) => (float)        this;
        public ushort  ToUInt16 ( IFormatProvider provider ) => (ushort) (int) this;
        public uint    ToUInt32 ( IFormatProvider provider ) => (uint)         this;
        public ulong   ToUInt64 ( IFormatProvider provider ) => (ulong)        this;

        public object ToType( Type conversionType, IFormatProvider provider ) => throw new NotImplementedException();

        public DateTime ToDateTime( IFormatProvider provider ) => throw new InvalidOperationException();
        public bool     ToBoolean ( IFormatProvider provider ) => throw new InvalidOperationException();
        
#endregion
    }
}