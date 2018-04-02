using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Lx {

   public struct ColorYUV {

      const float wr601 = 0.299f,  wg601 = 0.587f,  wb601 = 0.114f;
      const float wr709 = 0.2126f, wg709 = 0.7152f, wb709 = 0.0722f;
      const float uMax  = 0.436f, vMax = 0.615f;

      public enum ColorStandard { BT_601, BT_709 }

      public float y, u, v, a, wr, wg, wb;

      public ColorYUV( float y, float u, float v, float a, ColorStandard standard=ColorStandard.BT_601 ) {

         this.y = y;
         this.u = u;
         this.v = v;
         this.a = a;

         bool use709 = standard == ColorStandard.BT_709;
         wr = use709 ? wr709 : wr601;
         wg = use709 ? wg709 : wg601;
         wb = use709 ? wb709 : wb601;
      }

      public ColorYUV( Color rgb, float gamma=2.2f, ColorStandard standard=ColorStandard.BT_601 ) {

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

      public Color ToRGB( float gamma=2.2f, bool outOfGamutPreserveLuma=true ) {

         y = Mathf.Pow( y, gamma );

         float r = y + v * ((1.0f - wr) / vMax);
         float g = y - u * ((wb * (1.0f - wb)) / (uMax * wg)) - v * ((wr * (1.0f - wr)) / (vMax * wg));
         float b = y + u * ((1.0f - wb) / uMax);

         Color rgb = new Color( Mathf.Clamp01( r ), Mathf.Clamp01( g ), Mathf.Clamp01( b ), Mathf.Clamp01( a ) );

         if (outOfGamutPreserveLuma) {

            if (y < 0.0f) {
               rgb = Color.black;
            }
            else if (y > 1.0f) {
               rgb = Color.white;
            }
            else {
               float clampedY = wr * rgb.r + wg * rgb.g + wb * rgb.b;

               if (y > clampedY) {

                  float t = 1.0f - (1.0f - y) / (1.0f - clampedY);
                  rgb     = Color.Lerp( rgb, Color.white, t );
               }
               else if (y < clampedY) {

                  float t = 1.0f - y / clampedY;
                  rgb     = Color.Lerp( rgb, Color.black, t );
               }

               rgb.a = a;
            }
         }

         return gamma != 1.0f ? rgb.LinearToGamma( gamma ) : rgb;
      }

      public Color MaintainOutOfGamutLightness( Vector4 rawColor ) {

         Color result = default( Color );
         float outerDistance = ManhattanDistanceToGamut( rawColor );
         if (outerDistance < 0.0f) {
            float innerDistance = 0.0f;
            for (int i = 0; i < 3; i++) {
               if (rawColor[ i ] > 0.0f) { innerDistance += rawColor[ i ]; }
            }
            if (innerDistance < -outerDistance) { result = Color.black; }
            else {
               float t = -outerDistance / innerDistance;
               result = Color.Lerp( rawColor, Color.black, t );
            }
         }
         else if (outerDistance > 0.0f) {
            float innerDistance = 0.0f;
            for (int i = 0; i < 3; i++) {
               if (rawColor[ i ] < 1.0f) { innerDistance += rawColor[ i ] - 1.0f; }
            }
            if (innerDistance > -outerDistance) { result = Color.white; }
            else {
               float t = outerDistance / -innerDistance;
               result = Color.Lerp( rawColor, Color.white, t );
            }
         } else {
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

      public override string ToString() {

         return "(" + y.ToString( "0.000" ) + ", " + u.ToString( "0.000" ) + ", " + v.ToString( "0.000" ) + ", " + a.ToString( "0.000" ) + ")";
      }
   }

   public static partial class Utils {

      public static Color ColorMap( float input, float inMin, float inMax, Color outMin, Color outMax ) {

         return new Color( ValueMap( input, inMin, inMax, outMin.r, outMax.r ),
                           ValueMap( input, inMin, inMax, outMin.g, outMax.g ),
                           ValueMap( input, inMin, inMax, outMin.b, outMax.b ),
                           ValueMap( input, inMin, inMax, outMin.a, outMax.a ) );
      }

      public static Color LinearToGamma( this Color color, float gamma=2.2f ) {

         float power = 1.0f / gamma;
         return new Color( Mathf.Pow( color.r, power ), Mathf.Pow( color.g, power ),
                           Mathf.Pow( color.b, power ), color.a );
      }

      public static Color GammaToLinear( this Color color, float gamma=2.2f ) {

         return new Color( Mathf.Pow( color.r, gamma ), Mathf.Pow( color.g, gamma ),
                           Mathf.Pow( color.b, gamma ), color.a );
      }

      public static Color EvaluateLinear( this Gradient gradient, float time, float gamma=2.2f ) {

         float alpha = gradient.Evaluate( time ).a;

         for (int i = 0; i < gradient.colorKeys.Length - 1; i++) {

            float t = Mathf.InverseLerp( gradient.colorKeys[ i ].time, gradient.colorKeys[ i+1 ].time, time );

            if (t >= 0.0f && t <= 1.0f) {

               return Color.Lerp( gradient.colorKeys[ i   ].color.GammaToLinear( gamma ),
                                  gradient.colorKeys[ i+1 ].color.GammaToLinear( gamma ), t ).LinearToGamma( gamma );
            }
         }

         return Color.clear;
      }

      public static ColorYUV ToYUV( this Color rgb, float gamma=2.2f,
                                    ColorYUV.ColorStandard standard=ColorYUV.ColorStandard.BT_601 ) {

         return new ColorYUV( rgb, gamma, standard );
      }

      public static Color ShiftLuma( this Color color, float shift, float gamma=2.2f,
                                     ColorYUV.ColorStandard standard=ColorYUV.ColorStandard.BT_601 ) {

         return new ColorYUV( color, gamma, standard ).ShiftLuma( shift ).ToRGB( gamma );
      }

      public static Color ScaleLuma( this Color color, float shift, float gamma,
                                     ColorYUV.ColorStandard standard=ColorYUV.ColorStandard.BT_601 ) {

         return new ColorYUV( color, gamma, standard ).ScaleLuma( shift ).ToRGB( gamma );
      }

      public static Color CatmullRom( Color a, Color b, Color c, Color d, float t, bool snapToNearest ) {

         Vector4 aV = a, bV = b, cV = c, dV = d;  // do calculations with vectors to avoid clamping of rgb values
         
         Vector4 result = .5f*((-aV+3f*bV-3f*cV+dV)*(t*t*t)+(2f*aV-5f*bV+4f*cV-dV)*(t*t)+(-aV+cV)*t+2f*bV);

         if (!snapToNearest) { return result; }

         float distToB     = Vector4.Distance( result, bV ), distToC = Vector4.Distance( result, cV );
         float distBetween = Vector4.Distance( bV, cV );

         //if (distToB > distBetween || distToC > distBetween) { return result; }

         return distToB < distToC ? bV : cV;
      }

      public static Color ExtrapolatedColor( Color end, Color e2 ) {

         return 2.0f * (Vector4) end - (Vector4) e2;
      }

      // todo: UV parameter should be renamed to 'coordinate' since it's not in normalized space
      public static Color Bicubic( Flat2DArray< Color > colors, Vector2 uv, bool snapToNearest ) {

         while (uv.x < 0.0f)           { uv.x += colors.width;  }
         while (uv.x >= colors.width)  { uv.x -= colors.width;  }

         while (uv.y < 0.0f)           { uv.y += colors.height; }
         while (uv.y >= colors.height) { uv.y -= colors.height; }
         
         int x1 = (int) uv.x, x0 = x1.Ring( colors.width,  -1 ),
             x2 = x1.Ring( colors.width,  1 ), x3 = x1.Ring( colors.width,  2 );
             
         int y1 = (int) uv.y, y0 = y1.Ring( colors.height, -1 ),
             y2 = y1.Ring( colors.height, 1 ), y3 = y1.Ring( colors.height, 2 );

         Vector2 t = new Vector2( uv.x - x1, uv.y - y1 );
         
         Color h0 = CatmullRom( colors[ x0, y0 ], colors[ x0, y1 ], colors[ x0, y2 ], colors[ x0, y3 ], t.y, false );
         Color h1 = CatmullRom( colors[ x1, y0 ], colors[ x1, y1 ], colors[ x1, y2 ], colors[ x1, y3 ], t.y, false );
         Color h2 = CatmullRom( colors[ x2, y0 ], colors[ x2, y1 ], colors[ x2, y2 ], colors[ x2, y3 ], t.y, false );
         Color h3 = CatmullRom( colors[ x3, y0 ], colors[ x3, y1 ], colors[ x3, y2 ], colors[ x3, y3 ], t.y, false );

         Color result = CatmullRom( h0, h1, h2, h3, t.x, false );

         if (!snapToNearest) { return result; }

         return new[] { colors[ x1, y1 ], colors[ x2, y1 ], colors[ x1, y2 ], colors[ x2, y2 ] }
                   .MinBy( c => Vector4.Distance( result, c ) );
      }

      public static Color NearestNeighbour( Flat2DArray< Color > colors, Vector2 uv ) {

         int x = (int) uv.x;
         int y = (int) uv.y;
         
         while (x < 0) { x += colors.width; }
         while (x >= colors.width) { x -= colors.width; }
         
         while (y < 0) { y += colors.height; }
         while (y >= colors.height) { y -= colors.height; }

         return colors[ x, y ];
      }
   }
}