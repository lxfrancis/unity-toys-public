using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Lx {

   public static partial class Utils {

      public static float CatmullRom( float a, float b, float c, float d, float t ) {
         return .5f*((-a+3f*b-3f*c+d)*(t*t*t)+(2f*a-5f*b+4f*c-d)*(t*t)+(-a+c)*t+2f*b);
      }

      public static Vector2 CatmullRom( Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t ) {
         return .5f*((-a+3f*b-3f*c+d)*(t*t*t)+(2f*a-5f*b+4f*c-d)*(t*t)+(-a+c)*t+2f*b);
      }

      public static Vector3 CatmullRom( Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t ) {
         return .5f*((-a+3f*b-3f*c+d)*(t*t*t)+(2f*a-5f*b+4f*c-d)*(t*t)+(-a+c)*t+2f*b);
      }

      public static Vector4 CatmullRom( Vector4 a, Vector4 b, Vector4 c, Vector4 d, float t ) {
         return .5f*((-a+3f*b-3f*c+d)*(t*t*t)+(2f*a-5f*b+4f*c-d)*(t*t)+(-a+c)*t+2f*b);
      }

      static Vector2 ExtrapolatedPoint( Vector2 end, Vector2 e2, Vector2 e3 ) {
         
         Vector2 normal = (e2 - end).normalized;
         Vector2 extra  = Vector2.Reflect( e3 - e2, normal );

         // if (debugPreviewLine) {
         //    Debug.Log( "ext end: " + end + ", e2: " + e2 + ", e3: " + e3 + "; out: " + (2.0f * end - e2 + extra) );
         // }
         
         return 2.0f * end - e2 + extra;
      }

      static Vector3 ExtrapolatedPoint( Vector3 end, Vector3 e2, Vector3 e3 ) {
         
         Vector3 normal = (e2 - end).normalized;
         Vector3 extra  = Vector3.Reflect( e3 - e2, normal );

         // if (debugPreviewLine) {
         //    Debug.Log( "ext end: " + end + ", e2: " + e2 + ", e3: " + e3 + "; out: " + (2.0f * end - e2 + extra) );
         // }
         
         return 2.0f * end - e2 + extra;
      }
   }
}