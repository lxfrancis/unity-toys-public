// Unity string utility methods
// by Lexa Francis, 2014-2017

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

namespace Lx {

   public static partial class Utils {
      
      public static IEnumerable< TResult > SplitTo< TResult >( this string str, char separator,
                                                               Func< string, TResult > operation ) {

         return str.Split( separator ).Select( s => operation( s ) );
      }

      public static T ToEnum< T >( this string str ) {
         return (T) Enum.Parse( typeof( T ), str, true );
      }

      public static bool IsNullOrWhitespace( this string str ) {

         if (str == null || str.Trim().Length == 0) { return true; }
         return false;
      }

      public static string StringOrNullText( this string str ) {

         if (str == null) { return "null"; }
         if (str.Trim().Length == 0) { return "empty"; }
         return str;
      }

      public static string ToColor( this string str, Color col ) {

         return "<color=#" + ColorUtility.ToHtmlStringRGBA( col ) + ">" + str + "</color>";
      }

      public static string Bold( this string str ) { return "<b>" + str + "</b>"; }

      public static string Truncated( this string str, int max, string suffix = "...", int display_max = -1 ) {

         if (str == null) { return null; }
         if (display_max < 0) { display_max = max; }
         return str.Length > max ? str.Substring( 0, display_max ) + suffix : str;
      }

      public static string MaxSubstring( this string str, int startIndex, int length ) {

         length = Mathf.Clamp( length, 0, str.Length - startIndex );
         return str.Substring( startIndex, length );
      }

      public static string[] BoldNames< T >( this IEnumerable< T > objects ) where T: UnityEngine.Object {
         return objects.Select( o => o.BoldName() ).ToArray();
      }

      public static string BoldName( this UnityEngine.Object ob ) { return "<b>" + ob.name + "</b>"; }

      public static string StripStyleTags( this string str ) {

         foreach (string tag in new[] { "<b>", "</b>", "<i>", "</i>" }) { str = str.Replace( tag, "" ); }
         return str;
      }

      public static string ToKey( this string str ) {

         if (str == null) { return null; }
         return str.ToLower().Replace( " ", "" );
      }

      public static string CommaJoin( this IEnumerable< string > items, bool oxfordComma = false ) {

         string[]      array = items.ToArray();
         StringBuilder sb    = new StringBuilder();
         string        final = items.Count() > 2 && oxfordComma ? ", and " : " and ";

         for (int i = 0; i < array.Length; i++) {

            if (i == 0) { sb.Append( array[ i ] ); }
            else if (i == array.Length - 1) { sb.Append( final + array[ i ] ); }
            else { sb.Append( ", " + array[ i ] ); }
         }
         return sb.ToString();
      }

      public static string IndefiniteArticle( string thing, bool lowercase = false ) {

         string result = thing != null && thing.Length > 1 && "aeiou".Contains( thing.ToLower()[0] ) ? "An" : "A";
         if (lowercase) { result = "a" + result.Substring( 1 ); }
         return result;
      }

      public static int ParseInt( this String str ) {

         int n = 0;
         int.TryParse (str, out n);
         return n;
      }

      public static float ParseFloat( this String str ) {

         float n = 0;
         float.TryParse (str, out n);
         return n;
      }
   }
}
