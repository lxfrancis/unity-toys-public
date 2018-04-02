using UnityEngine;

namespace Lx {

   /// <summary>
   /// A three-dimensional array that behaves as a compact sliding window over an infinitely large indexable space.
   /// </summary>
   public class Partial3DArray< T > {

	   /// <summary>Addressable window dimensions.</summary>
	   public Coord3 dimensions { get; private set; }

	   /// <summary>Current offset of the addressable space.</summary>
	   public Coord3 offset { get; set; }

	   /// <summary>Cyclic partial 3D arrays will allow accessing indices outside the current range, behaving as an
	   /// infinite repetition of the addressable space.
	   /// Non-cyclic partial 3D arrays will not allow such access.</summary>
	   public bool cyclic { get; set; }

	   public readonly T[,,] array;

	   /// <summary>Construct a 3D array with the given dimensions and starting offset.</summary>
	   public Partial3DArray( Coord3 dimensions, Coord3? offset=null, bool cyclic=false ) {

		   if (dimensions.x < 1 || dimensions.y < 1 || dimensions.z < 1) {
			   throw new System.ArgumentOutOfRangeException();
		   }

		   array  		    = new T[ dimensions.x, dimensions.y, dimensions.z ];
		   this.offset     = offset ?? Coord3.zero;
		   this.dimensions = dimensions;
		   this.cyclic     = cyclic;
	   }

	   /// <summary>Construct a 3D array with the given dimensions and starting offset.</summary>
	   public Partial3DArray( int xSize, int ySize, int zSize, Coord3? offset=null, bool cyclic=false )
	          : this( new Coord3( xSize,     ySize,     zSize ),       offset,           cyclic ) { }

	   /// <summary>Access the element at the given index.</summary>
	   public T this[ Coord3 index ] {
		   get { return this[ index.x, index.y, index.z ];         }
		   set { 		 this[ index.x, index.y, index.z ] = value; }
	   }

	   /// <summary>Access the element at the given indices.</summary>
	   public T this[ int x, int y, int z ] {
		   get {
			   if (!cyclic && (x < offset.x || x >= dimensions.x + offset.x
			                || y < offset.y || y >= dimensions.y + offset.y
			                || z < offset.z || z >= dimensions.z + offset.z)) {
				
				   throw new System.ArgumentOutOfRangeException();
			   }
			   return array[ x.Ring( dimensions.x, 0 ),
			                 y.Ring( dimensions.y, 0 ),
			                 z.Ring( dimensions.z, 0 ) ];
		   }
		   set {
			   if (!cyclic && (x < offset.x || x >= dimensions.x + offset.x
			                || y < offset.y || y >= dimensions.y + offset.y
			                || z < offset.z || z >= dimensions.z + offset.z)) {

				   throw new System.ArgumentOutOfRangeException( "x: " + x + "; y: " + y + "; z: " + z
				                                                     + "; current range: " + currentRangeString );
			   }
			   array[ x.Ring( dimensions.x, 0 ),
			          y.Ring( dimensions.y, 0 ),
			          z.Ring( dimensions.z, 0 ) ] = value;

			   if (x == 0 && z == 0) {
				   Debug.Log( "Setting new value at external y: " + y + "; internal y: " + y.Ring( dimensions.y, offset.y ) + "; curr y offset: " + offset.y );
			   }
		   }
	   }

	   /// <summary>Shift the offset by the given delta, causing existing elements to wrap to the opposite sides.</summary>
	   public void Rotate( Coord3 offsetDelta ) {

		   Coord3 offset = this.offset;
		   for (int i = 0; i < 3; i++) { offset[ i ] += offsetDelta[ i ]; }
		   this.offset = offset;
	   }

	   /// <summary>Shift the offset by the given delta, causing existing elements to wrap to the opposite sides.</summary>
	   public void Rotate( int x, int y, int z ) {

		   Rotate( new Coord3( x, y, z ) );
	   }

	   /// <summary>Shift the offset by the given delta, pushing existing elements off the ends of the array.</summary>
	   public void Shift( Coord3 offsetDelta, T fillValue=default( T ) ) {

		   Debug.Log( "Shifting from offset " + this.offset + " by " + offsetDelta );

		   for (int x = 0; x < dimensions.x; x++) {
			   for (int y = 0; y < dimensions.y; y++) {
				   for (int z = 0; z < dimensions.z; z++) {

					   // if the fields are about to be 'shifted out', replace them with the default value
					   if (   (offsetDelta.x > 0 && x <  offsetDelta.x)
					       || (offsetDelta.x < 0 && x >= dimensions.x + offsetDelta.x)
					       || (offsetDelta.y > 0 && y <  offsetDelta.y)
					       || (offsetDelta.y < 0 && y >= dimensions.y + offsetDelta.y)
					       || (offsetDelta.z > 0 && z <  offsetDelta.z)
					       || (offsetDelta.z < 0 && z >= dimensions.z + offsetDelta.z)) {

						   if (x == 0 && z == 0) {

							   Debug.Log( "Erasing value at iteration y: " + y + "; mapped to y: " + y.Ring( dimensions.y, this.offset.y ) );
						   }

						   array[ x.Ring( dimensions.x, this.offset.x ),
						          y.Ring( dimensions.y, this.offset.y ),
						          z.Ring( dimensions.z, this.offset.z ) ] = fillValue;
					   }
				   }
			   }
		   }

		   Coord3 offset = this.offset;
		   for (int i = 0; i < 3; i++) { offset[ i ] += offsetDelta[ i ]; }
		   this.offset = offset;
	   }

	   /// <summary>Shift the offset by the given delta, pushing existing elements off the ends of the array.</summary>
	   public void Shift( int x, int y, int z, T fillValue=default( T ) ) {

		   Shift( new Coord3( x, y, z ), fillValue );
	   }

	   string currentRangeString {
		   get {
			   return "x: " + offset.x + " - " + (offset.x + dimensions.x - 1)
			      + "; y: " + offset.y + " - " + (offset.y + dimensions.y - 1)
			      + "; z: " + offset.z + " - " + (offset.z + dimensions.z - 1);
		   }
	   }
   }
}
