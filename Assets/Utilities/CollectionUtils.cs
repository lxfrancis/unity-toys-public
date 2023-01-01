// Collection classes and utility methods for Unity
// by Lexa Francis, 2014-2017

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Lx {

   /// <summary>A dictionary with string keys that ignore whitespace and letter case.</summary>
   public class LaxStringDict< T >: Dictionary< string, T >, IDictionary< string, T > {

      public new T this[ string key ] {
         get => base[ key.ToKey() ];
         set => base[ key.ToKey() ] = value;
      }

      public new void Add( string key, T value ) => base.Add( key.ToKey(), value );
      
      public new void Remove( string key ) => base.Remove( key.ToKey() );
      
      public new bool ContainsKey( string key ) => base.ContainsKey( key.ToKey() );

      public LaxStringDict() { }

      public LaxStringDict( IDictionary< string, T > dict ) {
         foreach (var entry in dict.Keys) { this[ entry ] = dict[ entry ]; }
      }
   }

   /// <summary>A simple dictionary for keeping a running tally of items,
   /// where items don't need to be explicitly added.</summary>
   public class CountTable: Dictionary< string, int > {

      public new int this[ string key ] {
         get {
            if (!ContainsKey( key )) { this[ key ] = 0; }
            return base[ key ];
         }
         set => base[ key ] = value;
      }

      public override string ToString() => ToString( false );

      public string ToString( bool bold )
          => string.Join( "\n", this.Select( kvp => (bold ? "<b>" : "")    + kvp.Key + ": "
                                                    + (bold ? "</b>" : "") + kvp.Value ).ToArray() );

      public int total => Values.Sum();
   }

   public interface IHasFrequency {

      float frequency { get; set; }
   }
   
   /// <summary>Allows key collections to include duplicate keys.</summary>
   public class DuplicateKeyComparer< TKey >: IComparer< TKey > where TKey: IComparable {

      readonly bool descending;

      public DuplicateKeyComparer( bool descending = false ) => this.descending = descending;

      public int Compare( TKey x, TKey y ) {

         int result = (descending ? y?.CompareTo( x ) : x?.CompareTo( y )) ?? 0;
         return result == 0 ? 1 : result;
      }
   }

   public class DescendingComparer< T >: IComparer< T > {

      public int Compare( T x, T y ) => Comparer< T >.Default.Compare( y, x );
   }
   
   /// <summary>A queue with a fixed capacity that jettisons the oldest elements as new ones are added.</summary>
   public class CappedQueue< T >: Queue< T > {

      int       _maxCount;
      public int MaxCount {
         get => _maxCount;
         set {
            _maxCount = value;
            Trim();
         }
      }

      public CappedQueue( int maxCount ) { _maxCount = maxCount; }

      public new void Enqueue( T item ) {

         base.Enqueue( item );
         Trim();
      }

      void Trim() { while (Count > _maxCount) { Dequeue(); } }
   }
   
   /// <summary>A list that won't include duplicate items
   /// (or close-enough items, if a tolerace function is provided).</summary>
   public class ListSet< T >: List< T > {

      readonly bool               addAtEnd;
      readonly Func< T, T, bool > tolerance;

      public ListSet( bool addAtEnd = false, Func< T, T, bool > toleranceFunc = null ) {

         this.addAtEnd = addAtEnd;
         tolerance     = toleranceFunc;
      }

      public ListSet( IEnumerable< T > collection, bool addAtEnd = false,
                      Func< T, T, bool > toleranceFunc = null ): base( collection.Count() ) {

         this.addAtEnd = addAtEnd;
         tolerance     = toleranceFunc;

         foreach (T item in collection) { Add( item ); }
      }

      public new void Add( T item ) {
          
         if (!Contains( item )) {
            if (tolerance == null || Count == 0 || this.All( x => tolerance( x, item ) )) { base.Add( item ); }
         }
         else if (addAtEnd) {

            Remove( item );
            base.Add( item );
         }
      }

      public new void AddRange( IEnumerable< T > items ) { foreach (T item in items) { Add( item ); } }
   }
   
   /// <summary>A sorted list, where elements are sorted based on the time they were added (according to Time.time),
   /// and covering only a fixed period of time into the past.</summary>
   public class TimedList< T >: SortedList< float, T > {

      bool          interruptThreads;
      float         lastModifiedTime;

      List< float > intervals;
      float         lastIntervalsRetrievalTime;

      float       _duration;
      public float Duration {
         get => _duration;
         set {
            _duration = value;
            Trim();
         }
      }
      
      /// <summary><para>Initialise the TimedList with the given duration.</para>
      /// <para>If T is System.Threading.Thread and interruptThreads is true,
      /// threads will be interrupted when popped off the end of the list.</para></summary>
      public TimedList( float duration, bool interruptThreads=false ): base( new DuplicateKeyComparer< float >() ) {

         _duration = duration;
         this.interruptThreads = interruptThreads && typeof( T ) == typeof( System.Threading.Thread );
         lastModifiedTime = -1.0f;
      }

      public void Add( T item ) {

         base.Add( Time.time, item );
         lastModifiedTime = Time.time;
         Trim();
      }

      public new void Add( float time, T item ) {

         base.Add( time, item );
         lastModifiedTime = Time.time;
         Trim();
      }

      public void Trim() {

         while (this.Any() && this.First().Key < Time.time - _duration) {

            if (interruptThreads) { (this.First().Value as System.Threading.Thread)?.Interrupt(); }
            RemoveAt( 0 );
            lastModifiedTime = Time.time;
         }
      }

      /// <summary>Returns a list of the time intervals between each item.</summary>
      [SuppressMessage( "ReSharper", "CompareOfFloatsByEqualityOperator" )]
      public List< float > Intervals {
         get {
            Trim();
            if (intervals != null && lastModifiedTime == lastIntervalsRetrievalTime) { return intervals; }

            intervals                  = new List< float >();
            lastIntervalsRetrievalTime = lastModifiedTime;
            float lastKey              = -1.0f;

            foreach (float time in Keys) {
               if (lastKey == -1.0f) { lastKey = time; } else {
                  if (time != lastKey) { intervals.Add( time - lastKey ); }
                  lastKey = time;
               }
            }
            return intervals;
         }
      }
   }
   
   /// <summary>Spits out integers from .next below a fixed value,
   /// and won't give the same value twice within some specified minimum number of calls.</summary>
   public class VariantRandomiser {

       readonly CappedQueue< int > recent;
      int                          variants;

      public VariantRandomiser( int minBeforeRepeat, int variants ) {

         minBeforeRepeat = Mathf.Min( minBeforeRepeat, variants - 1 );
         this.variants   = variants;
         recent          = new CappedQueue< int >( minBeforeRepeat );
      }

      public int next {
         get {
            int selection = Random.Range( 0, variants - recent.Count );
            for (int i = 0; i < selection; i++) { if (recent.Contains( i )) { selection++; } }
            recent.Enqueue( selection );
            return selection;
         }
      }

      public void Reset() => recent.Clear();
   }

   public static partial class Utils {

       public static List< T > Shuffled< T >( this IEnumerable< T > collection ) {
           
           var shuffled = collection.ToList();

           for (int i = 0; i < shuffled.Count - 1; i++) {

               int r = Random.Range( i, shuffled.Count );
               (shuffled[ i ], shuffled[ r ]) = (shuffled[ r ], shuffled[ i ]);
           }
            
           return shuffled;
       }

       public static string ToListString( this IEnumerable< string > strings, string prefix = "" ) {

         return string.Join( "\n", strings.Select( s => prefix + s ).ToArray() );
      }
      
      /// <summary>Destroy all Unity Objects in this collection, optionally destroying the associated GameObject
      /// (if there is one) and clearing the collection.</summary>
      public static void DestroyAll( this ICollection<Object> collection, bool destroyGameObject =true,
                                     bool clear =true ) {
         
         foreach (Object item in collection) {

            if (!item) { continue; }
            if (destroyGameObject) { item.DestroyGameObject(); } else { Object.Destroy( item ); }
         }

         if (clear) { collection.Clear(); }
      }
      
      /// <summary>Destroy the Unity Object and remove it from the collection,
      /// optionally destroying the associated GameObject (if there is one).</summary>
      public static void Destroy( this ICollection< Object > collection, Object obj,
                                  bool destroyGameObject =true ) {

         collection.Remove( obj );
         if (!obj) { return; }
         if (destroyGameObject) { obj.DestroyGameObject(); } else { Object.Destroy( obj ); }
      }
      
      /// <summary>Destroy the Unity Object at the given key and remove it from the dictionary,
      /// optionally destroying the associated GameObject (if there is one).</summary>
      public static void Destroy< T, U >( this IDictionary< T, U > collection, T key, bool destroyGameObject=true )
                               where U: Object {

         Object o = collection.GetOrNull( key );

         if (!o) {

            Debug.LogWarning( "Key not found in IDictionary.Destroy()" );
            return;
         }

         collection.Remove( key );
         if (destroyGameObject) { o.DestroyGameObject(); } else { Object.Destroy( o ); }
      }

      // modified from http://stackoverflow.com/a/653602
      /// <summary>Removes all elements matched by the given predicate. Returns the number of elements removed.</summary>
      public static int RemoveAll< TKey, TValue >( this SortedList< TKey, TValue > collection,
                                                   Func< KeyValuePair< TKey, TValue >, bool > predicate ) {
          
         int numRemoved = 0;

         for (int i = 0; i < collection.Count; i++) {

            if (predicate( collection.ElementAt( i ) )) {

               collection.RemoveAt( i-- );
               numRemoved++;
            }
         }
         return numRemoved;
      }
      
      /// <summary>Removes all elements with the given value. Returns the number of elements removed.</summary>
      public static int RemoveValue< TKey, TValue >( this SortedList< TKey, TValue > collection, TValue value )
                                     where TValue: IEquatable< TValue > {

          int numRemoved = 0;

         for (int i = 0; i < collection.Count; i++) {

             if (collection.ElementAt( i ).Value.Equals( value )) {

               collection.RemoveAt( i-- );
               numRemoved++;
             }
         }
         return numRemoved;
      }
      
      /// <summary>Returns a random element.</summary>
      public static T RandomElement< T >( this IEnumerable< T > collection ) {

          IEnumerable< T > list = collection.ToList();
          return list.ElementAt( Random.Range( 0, list.Count() ) );
      }
      
      /// <summary>Returns a random element.</summary>
      public static T RandomOrDefault< T >( this IEnumerable< T > collection )
          => collection.DefaultIfEmpty().RandomElement();

      /// <summary>Returns the item at the given key if it is present, otherwise null.</summary>
      public static T GetOrNull< U, T >( this IDictionary< U, T > dictionary, U key ) where T: class
          => dictionary?.ContainsKey( key ) ?? false ? dictionary[ key ] : null;

      /// <summary>Returns the first item matched by the given predicate, otherwise null.</summary>
      public static T? FirstOrNull< T >( this IEnumerable< T > collection, Func< T, bool > predicate ) where T: struct {
          IEnumerable< T > enumerable = collection as T[] ?? collection.ToArray();
          return enumerable.Any( predicate ) ? (T?) enumerable.First( predicate ) : null;
      }

      /// <summary>Returns an element from the collection where each element has an associated frequency
      /// (relative probability of occurring).</summary>
      public static T WeightedRandom< T >( this IEnumerable< T > items ) where T: IHasFrequency {

          IEnumerable< T > list        = items.ToList();
          float            randomPoint = Random.Range( 0.0f, list.Sum( i => i.frequency ) );

         foreach (T item in list) {

            if (randomPoint < item.frequency) { return item; }
            randomPoint -= item.frequency;
         }

         Debug.LogWarning( "THIS SHOULD NEVER HAPPEN: iterated over entire weighted item list; returning last. List contents:\n" + string.Join( "\n", list.Select( i => i + "; frequency: " + i.frequency ).ToArray() ) );
         throw new InvalidOperationException( "Iterated over whole weighted item list :(" );
      }
      
      /// <summary>Returns a random subset of num items from the collection.</summary>
      public static IEnumerable< T > RandomSelection< T >( this IEnumerable< T > items, int num ) { 
          
         var list = items.ToList();
         if (num >= list.Count) { return list; }
         int sourceIndex = 0, selectedIndex = 0;

         return list.Where( item => Random.value < (num - selectedIndex) / (double) (list.Count - sourceIndex++) )
                    .Select( item => (selectedIndex++, item).item );
      }
      
      public static void TestRandomSelection() {

          Debug.Log( "Testing random selection routine..." );
          var array = new []{ 0, 10, 20, 30, 40, 50, 60, 70, 80, 90 };

          for (int i = 1; i <= 5; i++) {
              
              Debug.Log( $"{i} elements" );

              for (int j = 0; j < 5; j++) {

                  var selected = array.RandomSelection( i ).ToArray();
                  Debug.Log( PrintVals( selected ) );
              }
          }
      }
      
      /// <summary>Returns the item for which the selector gives the lowest value.</summary>
      public static T MinBy< T, TBy >( this IEnumerable< T > items, Func< T, TBy > selector )
                          where TBy: IComparable {

          var list        = items.ToList();
          T   currentItem = list.First();
          TBy currentMin  = selector( currentItem );

         foreach (T item in list) {

            TBy value = selector( item );

            if (value.CompareTo( currentMin ) < 0) {

               currentItem = item;
               currentMin  = value;
            }
         }

         return currentItem;
      }
      
      /// <summary>Returns the item for which the selector gives the highest value.</summary>
      public static T MaxBy< T, TBy >( this IEnumerable< T > items, Func< T, TBy > selector )
                          where TBy: IComparable {

          var list        = items.ToList();
          T   currentItem = list.First();
          TBy currentMin  = selector( currentItem );

         foreach (T item in list) {

            TBy value = selector( item );

            if (value.CompareTo( currentMin ) > 0) {

               currentItem = item;
               currentMin  = value;
            }
         }

         return currentItem;
      }

      public static LaxStringDict< T > ToLaxStringDict< T >( this IEnumerable< T > collection,
                                                             Func< T, string > keySelector ) {

         LaxStringDict< T > dict = new LaxStringDict< T >();
         foreach (T item in collection) { dict[ keySelector( item ) ] = item; }
         return dict;
      }

      public static LaxStringDict< TValue > ToLaxStringDict< TEntry, TValue >( this IEnumerable< TEntry > collection,
                                                                               Func< TEntry, string > keySelector,
                                                                               Func< TEntry, TValue > valueSelector ) {
         LaxStringDict< TValue > dict = new LaxStringDict< TValue >();
         foreach (TEntry item in collection) { dict[ keySelector( item ) ] = valueSelector( item ); }
         return dict;
      }
      
      /// <summary>Returns a string with all values in an array, optionally along with their type.</summary>
      public static string PrintVals< T >( IEnumerable< T > data, bool type =false, bool newline =false ) {

         if (data == null) { return "<null collection>"; }

         return string.Join( newline ? "\n" : ", ",
                             data.Select( val => (type ? val?.GetType().Name + ": " : "") + val ).ToArray() );
      }
      
      /// <summary>Returns a string with all keys and values in a dictionary, separated by newlines.</summary>
      public static string PrintVals< TKey, TValue >( IDictionary< TKey, TValue > data, bool boldKeys =true,
                                                      bool boldVals =false )
      
          => string.Join( "\n", data.Select( kvp => (boldKeys ? (kvp.Key + ": ").Bold() : kvp.Key + ": ")
                                                    + (boldVals ? kvp.Value.ToString().Bold()
                                                                : kvp.Value.ToString()) ).ToArray() );

      struct PrintableValue {

         public string name, val;
      }

      public static PrintValList PrintVals() => new PrintValList();

      /// <summary>Start a PrintValList of name-value pairs, beginning with the first element.</summary>
      public static PrintValList PrintVals< T >( string n, T v ) => new PrintValList().Add( n, v );

      /// <summary>Start a PrintValList of name-value pairs, beginning with the first element.
      /// Value is the result of the given function.</summary>
      public static PrintValList PrintVals< T >( string n, Func< T > f ) => new PrintValList().Add( n, f );

      /// <summary>A list of name-value pairs, which implicitly converts to a string.</summary>
      public class PrintValList {

          readonly List< PrintableValue > vals = new List< PrintableValue >();
         
         /// <summary>Add a new name-value element to the PrintValList.</summary>
         public PrintValList Add< T >( string n, T v ) {

            string val = v?.ToString() ?? "null";
            vals.Add( new PrintableValue { name = n, val = val } );
            return this;
         }
         
         /// <summary>Add a new name-value element to the PrintValList. Value is the result of the given function.</summary>
         public PrintValList Add< T >( string n, Func< T > f ) {

            string val = "null exception";
            try { val = f().ToString(); }
            catch {
                // ignored
            }
            vals.Add( new PrintableValue { name = n, val = val } );
            return this;
         }
         
         /// <summary>Output all name-value pairs to a string with the given separator.</summary>
         public string ToString( string separator = "\n", bool boldNames =true, bool boldValues =false )
             => string.Join( separator, vals.Select( v
                                             => (boldNames ? (v.name + ": ").Bold() : v.name + ": ")
                                              + (boldValues ? (v.val != null ? v.val.ToString() : "null").Bold()
                                                            :  v.val != null ? v.val.ToString() : "null") ).ToArray() );

         /// <summary>Output all name-value pairs to a string with a newline separator.</summary>
         public static implicit operator string( PrintValList list ) => list.ToString();

         /// <summary>Output all name-value pairs to the console with the given separator.</summary>
         public void Log( bool boldNames =true, bool boldValues =false )
             => Debug.Log( ToString( "\n", boldNames, boldValues ) );
      }
   }
}