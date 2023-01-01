// Classes for multidimensional arrays that are internally flat arrays, allowing for easy conversion
// by Lexa Francis, 2014-2017

using System.Linq;
using static Lx.OutOfRangeBehaviour;

namespace Lx {

   public enum OutOfRangeBehaviour {

      Strict,
      Cycle
   }

   /// <summary>
   /// A generic container that behaves like a multidimensional array but uses a flat array internally.
   /// </summary>
   /// <typeparam name="T">Array type.</typeparam>
   public class FlatMDArray< T > {

      /// <summary>
      /// Factory method that creates a Flat2DArray or Flat3DArray if applicable.
      /// </summary>
      /// <param name="dimensions">Size in each dimension.</param>
      /// <returns></returns>
      public static FlatMDArray< T > Create( params int[] dimensions ) {

         switch (dimensions.Length) {
         
            case 2:  { return new Flat2DArray< T >( dimensions[ 0 ], dimensions[ 1 ] ); }
            case 3:  { return new Flat3DArray< T >( dimensions[ 0 ], dimensions[ 1 ], dimensions[ 2 ] ); }
            default: { return new FlatMDArray< T >( dimensions ); }
         }
      }

      /// <summary>Underlying flat array.</summary>
      public T[]    flat             { get { return m_array;            } }
      /// <summary>Size in each dimension.</summary>
      public int[]  dimensions       { get { return m_dimensions;       } }
      /// <summary>String representing the size in each dimension.</summary>
      public string dimensionsString { get { return m_dimensionsString; } }
      public OutOfRangeBehaviour outOfRangeBehaviour;

      protected T[]    m_array;
      protected int[]  m_dimensions;
      protected string m_dimensionsString;
      
      protected FlatMDArray( params int[] dimensions ) {

         m_dimensions = dimensions;
         int length   = 1;
         foreach (int d in dimensions) { length *= d; }
         m_array            = new T[ length ];
         m_dimensionsString = "[" + string.Join( ", ", dimensions.Select( d => d.ToString() ).ToArray() ) + "]";
      }
      
      /// <summary>Convert the given multidimensional indices into an appropriate flat index.</summary>
      public virtual int FlatIndex( params int[] indices ) {

         if (indices.Length != m_dimensions.Length) {

            throw new System.ArgumentException( "Expected " + m_dimensions.Length + " indices; only "
                                                   + indices.Length + " were specified." );
         }
         int index = 0;

         for (int i = 0; i < indices.Length; i++) {

            int multiplier = 1;

            if (outOfRangeBehaviour == Cycle) {
               indices[ i ] = indices[ i ].Ring( m_dimensions[ i ] );
            }

            if (indices[ i ] < 0 || indices[ i ] >= m_dimensions[ i ]) {

               throw new System.ArgumentException( "Index #" + i + ": " + indices[ i ]
                                                      + " is out of bounds. Dimensions: " + m_dimensionsString );
            }
            for (int j = i - 1; j >= 0; j--) { multiplier *= m_dimensions[ j ]; }

            index += indices[ i ] * multiplier;
         }
         return index;
      }

      // this should not be used - exists to be overridden in Flat2DArray
      public virtual int FlatIndex( int x, int y ) {

         return FlatIndex( new[] { x, y } );
      }
   
      // this should not be used - exists to be overridden in Flat3DArray
      public virtual int FlatIndex( int x, int y, int z ) {

         return FlatIndex( new[] { x, y, z } );
      }
      
      /// <summary>Access an element by multidimensional indices.</summary>
      public T this[ params int[] indices ] {
         get { return m_array[ FlatIndex( indices ) ];  }
         set { m_array[ FlatIndex( indices ) ] = value; }
      }

      public static implicit operator T[]( FlatMDArray< T > array ) { return array.flat; }
   }

   /// <summary>
   /// A generic container that behaves like a 2D array but uses a flat array internally.
   /// </summary>
   /// <typeparam name="T">Array type.</typeparam>
   public class Flat2DArray< T >: FlatMDArray< T > {

      public int width  { get { return m_width;  } }
      public int height { get { return m_height; } }

      protected int m_width, m_height;

      /// <summary>
      /// Construct a new Flat2DArray with the given dimensions.
      /// </summary>
      /// <param name="width">X-size.</param>
      /// <param name="height">Y-size.</param>
      /// <param name="flat">An existing array to use as the flat backing array.</param>
      public Flat2DArray( int width, int height, T[] flat =null, OutOfRangeBehaviour outOfRangeBehaviour=Strict ):
         base( width, height ) {

         m_width  = width;
         m_height = height;
         if (flat != null) { m_array = flat; }
         this.outOfRangeBehaviour = outOfRangeBehaviour;
      }

      public Flat2DArray( Coord2 dimensions, T[] flat =null, OutOfRangeBehaviour outOfRangeBehaviour=Strict ):
                          this( dimensions.x, dimensions.y, flat, outOfRangeBehaviour ) { }
      
      /// <summary>Convert the given 2D indices into an appropriate flat index.</summary>
      public override int FlatIndex( int x, int y ) {

         if (outOfRangeBehaviour == Cycle) {

            x = x.Ring( width );
            y = y.Ring( height );
         }

         if (x < 0 || x > m_width || y < 0 || y > m_height) {

            throw new System.ArgumentException( "Index (x: " + x + ", y: " + y
                                                   + ") out of bounds. Dimensions: " + m_dimensionsString );
         }

         return x + y * m_width;
      }
   
      /// <summary>Applies an action to each valid set of coordinates within the array.
      /// Note that this method itself does nothing with the array and only iterates over indices.</summary>
      /// <param name="buffer">Stop short of the last index in each dimension by this amount.</param>
      public void Indexer( System.Action< int, int > action, int buffer=0 ) {

         for (int x = 0; x < width - buffer; x++) {
            for (int y = 0; y < height - buffer; y++) {
               action( x, y );
            }
         }
      }

      /// <summary>Returns a copy of this Flat2DArray which is padded by the specified amount around the minima and
      /// maxima in each dimension, using the specified pad value in each new cell.</summary>
      /// <param name="atMin">Number of extra cells to add at the beginning of each dimension.</param>
      /// <param name="atMax">Number of extra cells to add at the end of each dimension.</param>
      /// <param name="padValue">Value to use in each newly added cell.</param>
      public Flat2DArray< T > Padded( Coord2 atMin, Coord2 atMax, T padValue=default( T ) ) {

         Flat2DArray< T > padded = new Flat2DArray< T >( width + atMin.x + atMax.x, height + atMin.y + atMax.y );

         padded.Indexer( (x, y) => {
            Coord2 source = new Coord2( x, y ) - atMin;
            if (source.x < 0 || source.x >= width || source.y < 0 || source.y >= height) {
               padded[ x, y ] = padValue;
            } else {
               padded[ x, y ] = this[ source.x, source.y ];
            }
         } );

         return padded;
      }
   }
   
   /// <summary>
   /// A generic container that behaves like a 3D array but uses a flat array internally.
   /// </summary>
   /// <typeparam name="T">Array type.</typeparam>
   public class Flat3DArray< T >: FlatMDArray< T > {

      public int width  { get { return m_width;  } }
      public int height { get { return m_height; } }
      public int depth  { get { return m_depth;  } }

      protected int m_width, m_height, m_depth, zMultiple;
      
      /// <summary>
      /// Construct a new Flat3DArray with the given dimensions.
      /// </summary>
      /// <param name="width">X-size.</param>
      /// <param name="height">Y-size.</param>
      /// <param name="depth">Z-size.</param>
      /// <param name="flat">An existing array to use as the flat backing array.</param>
      public Flat3DArray( int width, int height, int depth, T[] flat=null,
                          OutOfRangeBehaviour outOfRangeBehaviour=Strict ): base( width, height, depth ) {

         m_width   = width;
         m_height  = height;
         m_depth   = depth;
         zMultiple = width * height;
         if (flat != null) { m_array = flat; }
         this.outOfRangeBehaviour = outOfRangeBehaviour;
      }

      public Flat3DArray( Coord3 dimensions, T[] flat=null,
                          OutOfRangeBehaviour outOfRangeBehaviour=Strict ):
                          this( dimensions.x, dimensions.y, dimensions.z, flat, outOfRangeBehaviour ) { }
      
      /// <summary>Convert the given 3D indices into an appropriate flat index.</summary>
      public override int FlatIndex( int x, int y, int z ) {

         if (outOfRangeBehaviour == Cycle) {
            
            x = x.Ring( width );
            y = y.Ring( height );
            z = z.Ring( depth );
         }

         if (x < 0 || x >= m_width || y < 0 || y >= m_height || z < 0 || z >= m_depth ) {

            throw new System.ArgumentException( "Index (x: " + x + ", y: " + y + ", z: " + z
                                                   + ") out of bounds. Dimensions: " + m_dimensionsString );
         }
         return x + y * m_width + z * zMultiple;
      }

      public T this[ Coord3 coords ] {
         get { return flat[ FlatIndex( coords.x, coords.y, coords.z ) ]; }
         set { flat[ FlatIndex( coords.x, coords.y, coords.z ) ] = value; }
      }
   
      /// <summary>Applies an action to each valid set of coordinates within the array.
      /// Note that this method itself does nothing with the array and only iterates over indices.</summary>
      /// <param name="buffer">Stop short of the last index in each dimension by this amount.</param>
      public void Indexer( System.Action< int, int, int > action, int buffer=0 ) {

         for (int x = 0; x < width - buffer; x++) {
            for (int y = 0; y < height - buffer; y++) {
               for (int z = 0; z < depth - buffer; z++) {
                  action( x, y, z );
               }
            }
         }
      }
   
      /// <summary>Applies an action to each valid set of coordinates within the array.
      /// Note that this method itself does nothing with the array and only iterates over indices.</summary>
      /// <param name="buffer">Stop short of the last index in each dimension by this amount.</param>
      public void Indexer( System.Action< Coord3 > action, int buffer=0 ) {

         for (int x = 0; x < width - buffer; x++) {
            for (int y = 0; y < height - buffer; y++) {
               for (int z = 0; z < depth - buffer; z++) {
                  action( new Coord3( x, y, z ) );
               }
            }
         }
      }

      /// <summary>Returns a copy of this Flat3DArray which is padded by the specified amount around the minima and
      /// maxima in each dimension, using the specified pad value in each new cell.</summary>
      /// <param name="atMin">Number of extra cells to add at the beginning of each dimension.</param>
      /// <param name="atMax">Number of extra cells to add at the end of each dimension.</param>
      /// <param name="padValue">Value to use in each newly added cell.</param>
      public Flat3DArray< T > Padded( Coord3 atMin, Coord3 atMax, T padValue=default( T ), Flat3DArray< T > padded=null ) {

         padded 
            = padded ?? new Flat3DArray< T >( width + atMin.x + atMax.x, height + atMin.y + atMax.y, depth + atMin.z + atMax.z );

         padded.Indexer( (x, y, z) => {
            Coord3 source = new Coord3( x, y, z ) - atMin;
            if (source.x < 0 || source.x >= width || source.y < 0 || source.y >= height
                || source.z < 0 || source.z >= depth) {
               padded[ x, y, z ] = padValue;
            } else {
               padded[ x, y, z ] = this[ source.x, source.y, source.z ];
            }
         } );

         return padded;
      }
   }
}