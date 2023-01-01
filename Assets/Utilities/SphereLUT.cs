using UnityEngine;
using System.Collections.Generic;

namespace Lx {

   public static class SphereLUT {
   
      public static Coord2[] Circular( int coverage, float ringWidth=1.0f ) {

         int lutSpan  = coverage * 2 - 1;
         int numRings = (int) (Mathf.Sqrt( 2 ) * (lutSpan / ringWidth)) + 1;
         List< Coord2 >[] rings = new List< Coord2 >[ numRings ];
         for (int ring = 0; ring < numRings; ring++) { rings[ ring ] = new List< Coord2 >(); }
      
         for (int x = -coverage + 1; x < coverage; x++) {
            for (int y = -coverage + 1; y < coverage; y++) {
               rings[ (int) (Mathf.Sqrt( x * x + y * y ) / ringWidth) ].Add( new Coord2( x, y ) );
            }
         }
      
         Coord2[] lut = new Coord2[ lutSpan * lutSpan ];
         int      i   = 0;

         for (int ring = 0; ring < numRings; ring++) {
            for (int j = 0; j < rings[ ring ].Count; j++) {
               lut[ i ] = rings[ ring ][ j ];
               i++;
            }
         }
         return lut;
      }
   
      public static Coord3[] Spherical( int coverage, float ringWidth=1.0f, List< float > distances=null ) {

         int lutSpan   = coverage * 2 - 1;
         int numShells = (int) (Mathf.Sqrt( 3 ) * (lutSpan / ringWidth)) + 2;
         List< Coord3 >[] shells = new List< Coord3 >[ numShells ];
         for (int shell = 0; shell < numShells; shell++) { shells[ shell ] = new List< Coord3 >(); }

         for (int x = -coverage + 1; x < coverage; x++) {
            for (int y = -coverage + 1; y < coverage; y++) {
               for (int z = -coverage + 1; z < coverage; z++) {
                  Coord3 ic = new Coord3( x, y, z );
                  shells[ (int) (Mathf.Sqrt( x * x + y * y + z * z ) / ringWidth) ].Add( ic );
               }
            }
         }
      
         Coord3[] lut = new Coord3[ lutSpan * lutSpan * lutSpan ];
         int      i   = 0;

         for (int shell = 0; shell < numShells; shell++) {
            for (int j = 0; j < shells[ shell ].Count; j++) {
               lut[ i ] = shells[ shell ][ j ];
               if (distances != null) { distances.Add( ((Vector3) lut[ i ]).magnitude ); }
               i++;
            }
         }
         return lut;
      }
   }
}
