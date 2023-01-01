using System;
using UnityEngine;

namespace Lx {
    
    public struct ColorYUV : IEquatable< ColorYUV > {

        public enum ColorStandard { BT_601, BT_709 }

        const float wr601 = 0.299f,  wg601 = 0.587f,  wb601 = 0.114f;
        const float wr709 = 0.2126f, wg709 = 0.7152f, wb709 = 0.0722f;
        const float uMax  = 0.436f,  vMax  = 0.615f;

        public float y, u, v, a;
      
        readonly float wr, wg, wb;

        
        public ColorYUV( float y, float u, float v, float a, ColorStandard standard =ColorStandard.BT_601 ) {

            this.y = y;
            this.u = u;
            this.v = v;
            this.a = a;

            bool use709 = standard == ColorStandard.BT_709;
            wr = use709 ? wr709 : wr601;
            wg = use709 ? wg709 : wg601;
            wb = use709 ? wb709 : wb601;
        }

        
        public ColorYUV( Color rgb, float gamma =2.2f, ColorStandard standard =ColorStandard.BT_601 ) {

            if (gamma != 1.0f) { rgb = rgb.GammaToLinear( gamma ); }

            bool use709 = standard == ColorStandard.BT_709;
            wr = use709 ? wr709 : wr601;
            wg = use709 ? wg709 : wg601;
            wb = use709 ? wb709 : wb601;

            y = wr   * rgb.r + wg * rgb.g + wb * rgb.b;
            u = uMax * ((rgb.b - y) / (1.0f - wb));
            v = vMax * ((rgb.r - y) / (1.0f - wr));
            a = rgb.a;

            if (gamma != 1.0f) { y = Mathf.Pow( y, 1.0f / gamma ); }
        }

        
        public static ColorYUV YUVFromHSL( float hue, float saturation, float luma, float alpha,
                                        ColorStandard standard =ColorStandard.BT_601 ) {
         
            hue = Mathf.Deg2Rad * hue;
            
            return new ColorYUV( luma, Mathf.Cos( hue ) * saturation, Mathf.Sin( hue ) * saturation,
                                 alpha, standard );
        }

        
        public static Color RGBFromHSL( float hue, float saturation, float luma, float alpha,
                                        ColorStandard standard =ColorStandard.BT_601, float gamma=2.2f ) {

            return YUVFromHSL( hue, saturation, luma, alpha, standard ).ToRGB( gamma );
        }

        
        public static implicit operator Color( ColorYUV yuv ) => yuv.ToRGB();

        
        public static implicit operator ColorYUV( Color rgb ) => new ColorYUV( rgb );

        
        public Color ToRGB( float gamma=2.2f, bool outOfGamutPreserveLuma=true, bool errorOutOfGamut=false,
                            bool zeroAlphaOutOfGamut=false ) {

            y = Mathf.Pow( y, gamma );

            float r = y + v * ((1.0f - wr) / vMax);
            float g = y - u * (wb * (1.0f - wb) / (uMax * wg)) - v * (wr * (1.0f - wr) / (vMax * wg));
            float b = y + u * ((1.0f - wb) / uMax);

            Color rgb = new Color( Mathf.Clamp01( r ), Mathf.Clamp01( g ), Mathf.Clamp01( b ), Mathf.Clamp01( a ) );

            bool outOfGamut = false;

            if (y < 0.0f) {
                outOfGamut = true;
                if (outOfGamutPreserveLuma) { rgb = Color.black; }
            }
            else if (y > 1.0f) {
                outOfGamut = true;
                if (outOfGamutPreserveLuma) { rgb = Color.white; }
            }
            else {
                float clampedY = wr * rgb.r + wg * rgb.g + wb * rgb.b;

                if (y > clampedY) {

                    outOfGamut = true;

                    if (outOfGamutPreserveLuma) {
                  
                        float t = 1.0f - (1.0f - y) / (1.0f - clampedY);
                        rgb = Color.Lerp( rgb, Color.white, t );
                    }
                }
                else if (y < clampedY) {

                    outOfGamut = true;

                    if (outOfGamutPreserveLuma) {
                  
                        float t = 1.0f - y / clampedY;
                        rgb = Color.Lerp( rgb, Color.black, t );
                    }
                }

                rgb.a = a;
            }
         
            if (outOfGamut) {
            
                if (errorOutOfGamut) { Debug.Log( $"{this} is out of gamut" ); }
                if (zeroAlphaOutOfGamut) { rgb.a = 0f; }
            }

            return gamma != 1.0f ? rgb.LinearToGamma( gamma ) : rgb;
        }

        
        public Color MaintainOutOfGamutLightness( Vector4 rawColor ) {

            Color result;
            
            float outerDistance = ManhattanDistanceToGamut( rawColor );
            
            if (outerDistance < 0.0f) {
                
                float innerDistance = 0.0f;
                
                for (int i = 0; i < 3; i++) {
                    if (rawColor[ i ] > 0.0f) { innerDistance += rawColor[ i ]; }
                }
                
                if (innerDistance < -outerDistance) {
                    result = Color.black;
                } else {
                    float t = -outerDistance / innerDistance;
                    result = Color.Lerp( rawColor, Color.black, t );
                }
            }
            else if (outerDistance > 0.0f) {
                
                float innerDistance = 0.0f;
                
                for (int i = 0; i < 3; i++) {
                    if (rawColor[ i ] < 1.0f) { innerDistance += rawColor[ i ] - 1.0f; }
                }
                
                if (innerDistance > -outerDistance) {
                    result = Color.white;
                } else {
                    float t = outerDistance / -innerDistance;
                    result = Color.Lerp( rawColor, Color.white, t );
                }
            }
            else {
                result = rawColor;
            }
            
            result.a = rawColor.w;
            return result;
        }

        
        float ManhattanDistanceToGamut( Vector4 rawColor ) {

            float distance = 0.0f;

            for (int i = 0; i < 3; i++) {

                if (rawColor[ i ] < 0.0f) { distance += rawColor[ i ]; }
                else if (rawColor[ i ] > 1.0f) { distance += rawColor[ i ] - 1.0f; }
            }

            return distance;
        }

        
        public ColorYUV ShiftLuma( float shift ) {

            ColorYUV shifted = this;
            shifted.y += shift;
            return shifted;
        }

        
        public ColorYUV ScaleLuma( float scale ) {

            ColorYUV scaled = this;
            scaled.y *= scale;
            return scaled;
        }

        
        public ColorYUV ScaleChroma( float scale ) {

            ColorYUV scaled = this;
            scaled.u *= scale;
            scaled.v *= scale;
            return scaled;
        }

        
        public override string ToString() => $"ColorYUV( {y:0.000}, {u:0.000}, {v:0.000}, {a:0.000} )";
        
        
        public bool Equals( ColorYUV other )
            => y.Equals( other.y ) 
               && u.Equals( other.u ) 
               && v.Equals( other.v ) 
               && a.Equals( other.a )
               && wr.Equals( other.wr )
               && wg.Equals( other.wg ) 
               && wb.Equals( other.wb );

        public override bool Equals( object obj ) => obj is ColorYUV other && Equals( other );

        public override int GetHashCode() {
            unchecked {
                var hashCode = y.GetHashCode();
                hashCode = (hashCode * 397) ^ u.GetHashCode();
                hashCode = (hashCode * 397) ^ v.GetHashCode();
                hashCode = (hashCode * 397) ^ a.GetHashCode();
                hashCode = (hashCode * 397) ^ wr.GetHashCode();
                hashCode = (hashCode * 397) ^ wg.GetHashCode();
                hashCode = (hashCode * 397) ^ wb.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==( ColorYUV left, ColorYUV right ) => left.Equals( right );

        public static bool operator !=( ColorYUV left, ColorYUV right ) => !left.Equals( right );
    }
}
