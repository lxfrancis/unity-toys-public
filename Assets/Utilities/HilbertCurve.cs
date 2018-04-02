using UnityEngine;
using System.Linq;

namespace Lx {

   // adapted from http://www.tiac.net/~sw/2008/10/Hilbert/hilbert.py
   // original python code by Steve Witham http://www.tiac.net/~sw/2008/10/Hilbert
   public static class HilbertCurve {

      /// <summary>Convert the given index to coordinates on the curve with the given number of dimensions.</summary>
      public static int[] IntToHilbert( int i, int nd ) {

         int[] indexChunks = UnpackIndex( i, nd );
         int   mask        = (1 << nd) - 1;
         int   start       = 0;
         int   end         = InitialEnd( indexChunks.Length, nd );
         int[] coordChunks = new int[ indexChunks.Length ];

         for (int j = 0; j < indexChunks.Length; j++) {

            i                = indexChunks[ j ];
            coordChunks[ j ] = GrayEncodeTravel( start, end, mask, i );
            ChildStartEnd( ref start, ref end, mask, i );
         }

         return PackCoords( coordChunks, nd );
      }
      
      /// <summary>Convert the given coordinates on the curve into an index.</summary>
      public static int HilbertToInt( params int[] coords ) {

         int[] coordChunks = UnpackCoords( coords );
         int   mask        = (1 << coords.Length) - 1;
         int   start       = 0;
         int   end         = InitialEnd( coordChunks.Length, coords.Length );
         int[] indexChunks = new int[ coordChunks.Length ];

         for (int j = 0; j < coordChunks.Length; j++) {

            int i            = GrayDecodeTravel( start, end, mask, coordChunks[ j ] );
            indexChunks[ j ] = i;
            ChildStartEnd( ref start, ref end, mask, i );
         }

         return PackIndex( indexChunks, coords.Length );
      }

      static int[] UnpackIndex( int i, int nd ) {

         int   p       = 1 << nd;
         int   nChunks = Mathf.Max( 1, Mathf.CeilToInt( Mathf.Log( i + 1, p ) ) );
         int[] chunks  = new int[ nChunks ];

         for (int j = nChunks - 1; j > -1; j--) {

            chunks[ j ] = TrueMod( i, p );
            i /= p;
         }

         return chunks;
      }

      static int PackIndex( int[] chunks, int nd ) {  // TODO: check this

         int p = 1 << nd;
         return chunks.Aggregate( 0, (acc, x) => acc * p + x );
      }

      static int InitialEnd( int nChunks, int nd ) {

         return 1 << TrueMod( -nChunks - 1, nd );
      }

      static int GrayEncodeTravel( int start, int end, int mask, int i ) {
      
         int g = GrayEncode( i ) * (start ^ end) * 2;
         return ((g | (g / (mask + 1)) ) & mask) ^ start;
      }

      static int GrayDecodeTravel( int start, int end, int mask, int g ) {
      
         int modulus = mask + 1;
         int rg      = (g ^ start) * (modulus / ((start ^ end) * 2) );
         return GrayDecode( (rg | (rg / modulus)) & mask );
      }

      static int GrayEncode( int bn ) {

         return bn ^ (bn / 2);
      }

      static int GrayDecode( int n ) {

         int sh = 1;

         while (true) {

            int div = n >> sh;
            n      ^= div;
            if (div <= 1) { return n; }
            sh <<= 1;
         }
      }

      static void ChildStartEnd( ref int start, ref int end, int mask, int i ) {

         int parentStart = start, parentEnd = end;
         int startI = Mathf.Max( 0,    (i - 1) & ~1);
         int endI   = Mathf.Min( mask, (i + 1) |  1);
         start      = GrayEncodeTravel( parentStart, parentEnd, mask, startI );
         end        = GrayEncodeTravel( parentStart, parentEnd, mask, endI   );
      }

      static int[] TransposeBits( int[] srcs, int nDests ) {

         int[] dests = new int[ nDests ];

         for (int j = nDests - 1; j > -1; j--) {

            int dest = 0;

            for (int k = 0; k < srcs.Length; k++) {

               dest = dest * 2 + TrueMod( srcs[ k ], 2 );
               srcs[ k ] /= 2;
            }

            dests[ j ] = dest;
         }

         return dests;
      }

      static int[] PackCoords( int[] chunks, int nd ) {

         return TransposeBits( chunks, nd );
      }

      static int[] UnpackCoords( int[] coords ) {
      
         return TransposeBits( coords, Mathf.Max( 1, Mathf.CeilToInt( Mathf.Log( Mathf.Max( coords ) + 1, 2 ) ) ) );
      }

      static int TrueMod( int x, int m ) {

         if (m < 0) { m = -m; }
         return (x % m + m) % m;
      }
   }
}