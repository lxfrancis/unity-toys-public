// Methods for elegant pair and elegant unpair, and test code
// by Lexa Francis, 2014-2017

using System;

namespace Lx {
   
   public static class ElegantPair {

      public static long Pair( int x, int y, IntRange? overrideRange = null ) {

         checked {
            int max  =  23169;
            int min  = -23169;
            int flip =  46339;

            if (overrideRange.HasValue) {

               max  = overrideRange.Value.max;
               min  = overrideRange.Value.min;
               flip = max - min + 1;
            }

            if (x > max || x < min) {
               throw new ArgumentOutOfRangeException( nameof( x ),
                   $"Arguments must not have an absolute value greater than {max}. Value given: {x}" );
            }
            
            if (y > max || y < min) {
               throw new ArgumentOutOfRangeException( nameof( y ),
                   $"Arguments must not have an absolute value greater than {max}. Value given: {y}" );
            }
            
            if (max >= 4000000 || -min > 4000000) {
               throw new ArgumentOutOfRangeException( nameof( overrideRange ),
                   $"Unable to unpair values with argument ranges over 4000000. Value given: {overrideRange}" );
            }
            
            long nx = x < 0 ? x + flip : x;
            long ny = y < 0 ? y + flip : y;
            return (nx >= ny) ? (nx * nx + nx + ny) : (ny * ny + nx);
         }
      }

      public static Coord2 Unpair( long z, IntRange? overrideRange = null ) {

         checked {
            int max  = 23169;
            int flip = 46339;

            if (overrideRange.HasValue) {

               max  = overrideRange.Value.max;
               flip = max - overrideRange.Value.min + 1;
            }

            int sqrtz = (int) Math.Sqrt( z );
            int sqz   = sqrtz * sqrtz;
            long x, y;

            if (z - sqz >= sqrtz) {
               x = sqrtz;
               y = z - sqz - sqrtz;
            }
            else {
               x = z - sqz;
               y = sqrtz;
            }
            
            if (x > max) { x -= flip; }
            if (y > max) { y -= flip; }

            return new Coord2( (int) x, (int) y );
         }
      }
   }
}
