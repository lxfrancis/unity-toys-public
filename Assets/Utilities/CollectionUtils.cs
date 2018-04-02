// Collection classes and utility methods for Unity
// by Lexa Francis, 2014-2017

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lx {

   /// <summary>A dictionary with string keys that ignore whitespace and letter case.</summary>
   public class LaxStringDict< T >: Dictionary< string, T >, IDictionary< string, T > {

      public new T this[ string key ] {
         get { return base[ key.ToKey() ]; }
         set { base[ key.ToKey() ] = value; }
      }

      public new void Add( string key, T value ) { base.Add( key.ToKey(), value ); }
      public new void Remove( string key ) { base.Remove( key.ToKey() ); }
      public new bool ContainsKey( string key ) { return base.ContainsKey( key.ToKey() ); }

      public LaxStringDict(): base() { }

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
         set {
            base[ key ] = value;
         }
      }

      public override string ToString() { return ToString( false ); }

      public string ToString( bool bold = false ) {

         return string.Join( "\n", this.Select( kvp => (bold ? "<b>" : "") + kvp.Key + ": "
                                                         + (bold ? "</b>" : "") + kvp.Value ).ToArray() );
      }

      public int total { get { return Values.Sum(); } }
   }

   public interface IHasFrequency {

      float frequency { get; set; }
   }
   
   /// <summary>Allows key collections to include duplicate keys.</summary>
   public class DuplicateKeyComparer< TKey >: IComparer< TKey > where TKey: IComparable {

      bool descending = false;

      public DuplicateKeyComparer( bool descending = false ) {

         this.descending = descending;
      }

      public int Compare( TKey x, TKey y ) {

         int result = descending ? y.CompareTo( x ) : x.CompareTo( y );
         return result == 0 ? 1 : result;
      }
   }

   class DescendingComparer< T >: IComparer< T > {

      public int Compare( T x, T y ) { return Comparer< T >.Default.Compare( y, x ); }
   }
   
   /// <summary>A queue with a fixed capacity that jettisons the oldest elements as new ones are added.</summary>
   public class CappedQueue< T >: Queue< T > {

      int       _maxCount;
      public int MaxCount {
         get { return _maxCount; }
         set {
            _maxCount = value;
            Trim();
         }
      }

      public CappedQueue( int maxCount ): base() { _maxCount = maxCount; }

      public new void Enqueue( T item ) {

         base.Enqueue( item );
         Trim();
      }

      void Trim() { while (Count > _maxCount) { Dequeue(); } }
   }
   
   /// <summary>A list that won't include duplicate items
   /// (or close-enough items, if a tolerace function is provided).</summary>
   public class ListSet< T >: List< T > {

      bool               addAtEnd;
      Func< T, T, bool > tolerance;

      public ListSet( bool addAtEnd = false, Func< T, T, bool > toleranceFunc = null ) : base() {

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
            if (tolerance == null || Count == 0 || !this.Any( x => !tolerance( x, item ) )) { base.Add( item ); }
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
         get { return _duration; }
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

            if (interruptThreads) { (this.First().Value as System.Threading.Thread).Interrupt(); }
            RemoveAt( 0 );
            lastModifiedTime = Time.time;
         }
      }

      /// <summary>Returns a list of the time intervals between each item.</summary>
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

      CappedQueue< int > recent;
      int                variants = 1;

      public VariantRandomiser( int minBeforeRepeat, int variants ) {

         minBeforeRepeat = Mathf.Min( minBeforeRepeat, variants - 1 );
         this.variants   = variants;
         recent          = new CappedQueue< int >( minBeforeRepeat );
      }

      public int next {
         get {
            int selection = UnityEngine.Random.Range( 0, variants - recent.Count );
            for (int i = 0; i < selection; i++) { if (recent.Contains( i )) { selection++; } }
            recent.Enqueue( selection );
            return selection;
         }
      }

      public void Reset() {

         recent.Clear();
      }
   }

   public static partial class Utils {
      
      public static string ToListString( this IEnumerable< string > strings, string prefix = "" ) {

         return string.Join( "\n", strings.Select( s => prefix + s ).ToArray() );
      }
      
      /// <summary>Destroy all Unity Objects in this collection, optionally destroying the associated GameObject
      /// (if there is one) and clearing the collection.</summary>
      public static void DestroyAll< T >( this ICollection<T> collection, bool destroyGameObject=true,
                                          bool clear=true ) where T: UnityEngine.Object {
         
         foreach (T item in collection) {

            if (!item) { continue; }
            if (destroyGameObject) { item.DestroyGameObject(); } else { UnityEngine.Object.Destroy( item ); }
         }

         if (clear) { collection.Clear(); }
      }
      
      /// <summary>Destroy all Unity Object values in this collection, optionally destroying the associated GameObject
      /// (if there is one) and clearing the collection.</summary>
      public static void DestroyAllValues< T, U >( this IDictionary< T, U > collection, bool destroyGameObject=true )
                                        where U: UnityEngine.Object {

         foreach (U item in collection.Values) {

            if (!item) { continue; }
            if (destroyGameObject) { item.DestroyGameObject(); } else { UnityEngine.Object.Destroy( item ); }
         }

         collection.Clear();
      }
      
      /// <summary>Destroy the Unity Object and remove it from the collection,
      /// optionally destroying the associated GameObject (if there is one).</summary>
      public static void Destroy< T >( this ICollection< T > collection, T obj, bool destroyGameObject=true )
                            where T: UnityEngine.Object {

         collection.Remove( obj );
         if (!obj) { return; }
         if (destroyGameObject) { obj.DestroyGameObject(); } else { UnityEngine.Object.Destroy( obj ); }
      }
      
      /// <summary>Destroy the Unity Object at the given key and remove it from the dictionary,
      /// optionally destroying the associated GameObject (if there is one).</summary>
      public static void Destroy< T, U >( this IDictionary< T, U > collection, T key, bool destroyGameObject=true )
                               where U: UnityEngine.Object {

         UnityEngine.Object o = collection.GetOrNull( key );

         if (!o) {

            Debug.LogWarning( "Key not found in IDictionary.Destroy()" );
            return;
         }

         collection.Remove( key );
         if (destroyGameObject) { o.DestroyGameObject(); } else { UnityEngine.Object.Destroy( o ); }
      }

      // modified from http://stackoverflow.com/a/653602
      /// <summary>Removes all elements matched by the given predicate. Returns the number of elements removed.</summary>
      public static int RemoveAll< TKey, TValue >( this SortedList< TKey, TValue > collection,
                                                   Func< KeyValuePair< TKey, TValue >, bool > predicate ) {

         KeyValuePair< TKey, TValue > element;
         int                          numRemoved = 0;

         for (int i = 0; i < collection.Count; i++) {

            element = collection.ElementAt( i );

            if (predicate( element )) {

               collection.RemoveAt( i );
               numRemoved++;
               i--;
            }
         }
         return numRemoved;
      }
      
      /// <summary>Removes all elements with the given value. Returns the number of elements removed.</summary>
      public static int RemoveValue< TKey, TValue >( this SortedList< TKey, TValue > collection, TValue value )
                                     where TValue: IEquatable< TValue > {

         KeyValuePair< TKey, TValue > element;
         int                          numRemoved = 0;

         for (int i = 0; i < collection.Count; i++) {

            element = collection.ElementAt( i );

            if (element.Value.Equals( value )) {

               collection.RemoveAt( i );
               numRemoved++;
               i--;
            }
         }
         return numRemoved;
      }
      
      /// <summary>Returns a random element.</summary>
      public static T Random< T >( this IEnumerable< T > collection ) {

         return collection.ElementAt( UnityEngine.Random.Range( 0, collection.Count() ) );
      }
      
      /// <summary>Returns the item at the given key if it is present, otherwise null.</summary>
      public static T GetOrNull< U, T >( this IDictionary< U, T > dictionary, U key ) where T: class {

         if (!dictionary.ContainsKey( key )) { return null; }
         return dictionary[ key ];
      }
      
      /// <summary>Returns the first item matched by the given predicate, otherwise null.</summary>
      public static T? FirstOrNull< T >( this ICollection< T > collection, Func< T, bool > predicate ) where T: struct {

         if (collection.Any( i => predicate( i ) )) { return collection.First( i => predicate( i ) ); }
         return null;
      }
      
      /// <summary>Returns an element from the collection where each element has an associated frequency
      /// (relative probability of occurring).</summary>
      public static T WeightedRandom< T >( this IEnumerable< T > items ) where T: IHasFrequency {

         float randomPoint = UnityEngine.Random.Range( 0.0f, items.Sum( i => i.frequency ) );

         foreach (T item in items) {

            if (randomPoint < item.frequency) { return item; }
            randomPoint -= item.frequency;
         }

         Debug.LogWarning( "THIS SHOULD NEVER HAPPEN: iterated over entire weighted item list; returning last. List contents:\n" + string.Join( "\n", items.Select( i => i.ToString() + "; frequency: " + i.frequency ).ToArray() ) );
         throw new InvalidOperationException( "Iterated over whole weighted item list :(" );
      }
      
      /// <summary>Returns a random subset of num items from the collection.</summary>
      public static IEnumerable< T > RandomSelection< T >( this IEnumerable< T > items, int num ) {

         int total = items.Count();
         if (num >= total) { return items; }
         T[] selected = new T[ num ];
         var random   = new System.Random();
         int sourceIndex = 0, selectedIndex = 0;

         foreach (T item in items) {

            if (random.NextDouble() < (num - selectedIndex) / (double) (total - sourceIndex++)) {
               selected[ selectedIndex++ ] = item;
            }
         }

         Debug.Log( "RandomSelection()".Bold() + " returned " + selectedIndex + " * " + typeof( T ) );
         return selected;
      }
      
      /// <summary>Returns the item for which the selector gives the lowest value.</summary>
      public static T MinBy< T, TBy >( this IEnumerable< T > items, Func< T, TBy > selector )
                          where TBy: IComparable {

         T   currentItem = items.First();
         TBy currentMin  = selector( currentItem );

         foreach (T item in items) {

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
         
         T   currentItem = items.First();
         TBy currentMin  = selector( currentItem );

         foreach (T item in items) {

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
      public static string PrintVals< T >( T[] data, bool type=true, bool newline=false ) {

         return string.Join( newline ? "\n" : ", ",
                             data.Select( val => (type ? val.GetType().Name + ": " : "") + val.ToString() ).ToArray() );
      }
      
      /// <summary>Returns a string with all keys and values in a dictionary, separated by newlines.</summary>
      public static string PrintVals< TKey, TValue >( IDictionary< TKey, TValue > data,
                                                      bool boldKeys=true, bool boldVals=false ) {

         return string.Join( "\n", data.Select( kvp =>
              (boldKeys ? (kvp.Key.ToString() + ": ").Bold() : (kvp.Key.ToString() + ": "))
            + (boldVals ? kvp.Value.ToString().Bold() : kvp.Value.ToString()) ).ToArray() );
      }

      struct PrintableValue {

         public string name;
         public string val;
      }

      public static PrintValList PrintVals() { return new PrintValList(); }
      
      /// <summary>Start a PrintValList of name-value pairs, beginning with the first element.</summary>
      public static PrintValList PrintVals< T >( string n, T v ) { return new PrintValList().Add( n, v ); }
      
      /// <summary>Start a PrintValList of name-value pairs, beginning with the first element.
      /// Value is the result of the given function.</summary>
      public static PrintValList PrintVals< T >( string n, Func< T > f ) { return new PrintValList().Add( n, f ); }
      
      /// <summary>A list of name-value pairs, which implicitly converts to a string.</summary>
      public class PrintValList {

         List< PrintableValue > vals = new List< PrintableValue >();
         
         /// <summary>Add a new name-value element to the PrintValList.</summary>
         public PrintValList Add< T >( string n, T v ) {

            string val = v == null ? val = "null" : v.ToString();
            vals.Add( new PrintableValue { name = n, val = val } );
            return this;
         }
         
         /// <summary>Add a new name-value element to the PrintValList. Value is the result of the given function.</summary>
         public PrintValList Add< T >( string n, Func< T > f ) {

            string val = "null exception";
            try { val = f().ToString(); } catch { }
            vals.Add( new PrintableValue { name = n, val = val } );
            return this;
         }
         
         /// <summary>Output all name-value pairs to a string with the given separator.</summary>
         public string ToString( string separator = "\n", bool boldNames=true, bool boldValues=false ) {

            return string.Join( separator, vals.Select( v => (boldNames ? (v.name + ": ").Bold()
                                                                        :  v.name + ": ")
                                                  + (boldValues ? (v.val != null ? v.val.ToString() : "null").Bold()
                                                                :  v.val != null ? v.val.ToString() : "null") ).ToArray() );
         }
         
         /// <summary>Output all name-value pairs to a string with a newline separator.</summary>
         public static implicit operator string( PrintValList list ) { return list.ToString(); }
         
         /// <summary>Output all name-value pairs to the console with the given separator.</summary>
         public void Log( bool boldNames=true, bool boldValues=false ) {

            Debug.Log( ToString( "\n", boldNames, boldValues ) );
         }
      }
   }
}