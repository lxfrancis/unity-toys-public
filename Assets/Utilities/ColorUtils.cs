using UnityEngine;
using static Lx.ColorYUV.ColorStandard;
using static UnityEngine.Mathf;

namespace Lx {

   public static partial class Utils {

       public static Color With( this Color color, float? r=null, float? g=null, float? b=null, float? a=null )
           => new Color( r ?? color.r, g ?? color.g, b ?? color.b, a ?? color.a );

       public static Color WithRGB( this Color color, Color other ) {

           other.a = color.a;
           return other;
       }

       public static Color ColorMap( float input, float inMin, float inMax, Color outMin, Color outMax ) {

         return new Color( ValueMap( input, inMin, inMax, outMin.r, outMax.r ),
                           ValueMap( input, inMin, inMax, outMin.g, outMax.g ),
                           ValueMap( input, inMin, inMax, outMin.b, outMax.b ),
                           ValueMap( input, inMin, inMax, outMin.a, outMax.a ) );
      }

      public static Color LinearToGamma( this Color color, float gamma=2.2f ) {

         float power = 1.0f / gamma;
         return new Color( Pow( color.r, power ), Pow( color.g, power ), Pow( color.b, power ), color.a );
      }

      public static Color GammaToLinear( this Color color, float gamma =2.2f )
         => new Color( Pow( color.r, gamma ), Pow( color.g, gamma ), Pow( color.b, gamma ), color.a );

      public static Color EvaluateLinear( this Gradient gradient, float time, float gamma=2.2f ) {

         //float alpha = gradient.Evaluate( time ).a;

         for (int i = 0; i < gradient.colorKeys.Length - 1; i++) {

            float t = InverseLerp( gradient.colorKeys[ i ].time, gradient.colorKeys[ i+1 ].time, time );

            if (t >= 0.0f && t <= 1.0f) {

               return Color.Lerp( gradient.colorKeys[ i   ].color.GammaToLinear( gamma ),
                                  gradient.colorKeys[ i+1 ].color.GammaToLinear( gamma ), t ).LinearToGamma( gamma );
            }
         }

         return Color.clear;
      }

      public static ColorYUV ToYUV( this Color rgb, float gamma=2.2f,
                                    ColorYUV.ColorStandard standard=BT_601 ) {

         return new ColorYUV( rgb, gamma, standard );
      }

      public static Color ShiftLuma( this Color color, float shift, float gamma=2.2f,
                                     ColorYUV.ColorStandard standard=BT_601 ) {

         return new ColorYUV( color, gamma, standard ).ShiftLuma( shift ).ToRGB( gamma );
      }

      public static Color ScaleLuma( this Color color, float shift, float gamma,
                                     ColorYUV.ColorStandard standard=BT_601 ) {

         return new ColorYUV( color, gamma, standard ).ScaleLuma( shift ).ToRGB( gamma );
      }

      public static Color CatmullRom( Color a, Color b, Color c, Color d, float t, bool snapToNearest ) {

         Vector4 aV = a, bV = b, cV = c, dV = d;  // do calculations with vectors to avoid clamping of rgb values
         
         Vector4 result = .5f*((-aV+3f*bV-3f*cV+dV)*(t*t*t)+(2f*aV-5f*bV+4f*cV-dV)*(t*t)+(-aV+cV)*t+2f*bV);

         if (!snapToNearest) { return result; }

         float distToB = Vector4.Distance( result, bV ), distToC = Vector4.Distance( result, cV );
         // float distBetween = Vector4.Distance( bV, cV );

         //if (distToB > distBetween || distToC > distBetween) { return result; }

         return distToB < distToC ? bV : cV;
      }

      public static Color ExtrapolatedColor( Color end, Color e2 ) {

         return 2.0f * (Vector4) end - (Vector4) e2;
      }

      public static Color Bicubic( Flat2DArray< Color > colors, Vector2 uv, bool snapToNearest ) {
      
         while (uv.x < 0.0f)           { uv.x += colors.width;  }
         while (uv.x >= colors.width)  { uv.x -= colors.width;  }
      
         while (uv.y < 0.0f)           { uv.y += colors.height; }
         while (uv.y >= colors.height) { uv.y -= colors.height; }
         
         int x1 = (int) uv.x,                    x0 = (x1 - 1).Ring( colors.width ),
             x2 = (x1 + 1).Ring( colors.width ), x3 = (x1 + 2).Ring( colors.width );
             
         int y1 = (int) uv.y,                      y0 = (y1 - 1).Ring( colors.height ),
             y2 = ( y1 + 1).Ring( colors.height ), y3 = (y1 + 2).Ring( colors.height );
      
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