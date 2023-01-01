// Unity string utility methods
// by Lexa Francis, 2014-2021

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

namespace Lx {

    public static partial class Utils {

        public static IEnumerable< TResult > SplitTo< TResult >( this string str, char separator,
                                                                 Func< string, TResult > operation )
            => str.Split( separator ).Select( operation );

        public static T ToEnum< T >( this string str ) => (T) Enum.Parse( typeof( T ), str, true );

        public static bool IsNullOrWhitespace( this string str ) => string.IsNullOrWhiteSpace( str );

        public static string NonEmpty( this string str )
            => str == null ? "[null]" : str.Length == 0 ? "[empty]" : str.Trim().Length == 0 ? "[blank]" : str;

        public static string ToColor( this string str, Color col )
            => "<color=#" + ColorUtility.ToHtmlStringRGBA( col ) + ">" + str + "</color>";

        public static string ToUpperDoubleSpaced( this string str ) => str.Replace( " ", "  " ).ToUpper();

        public static string Bold( this string str ) => "<b>" + str + "</b>";

        public static string Truncated( this string str, int max, string suffix = "...", int display_max = -1 )
            => str?.Length > max ? str.Substring( 0, Mathf.Max( display_max, 0 ) ) + suffix : str;

        public static string MaxSubstring( this string str, int startIndex, int length )
            => str.Substring( startIndex, Mathf.Clamp( length, 0, str.Length - startIndex ) );

        public static string[] BoldNames( this IEnumerable< UnityEngine.Object > objects )
            => objects.Select( o => o.BoldName() ).ToArray();

        public static string BoldName( this UnityEngine.Object ob ) => "<b>" + ob.name + "</b>";

        public static string StripStyleTags( this string str ) => new[] {
            "<b>", "</b>", "<i>", "</i>"
        }.Aggregate( str, ( current, tag ) => current.Replace( tag, "" ) );

        public static string ToKey( this string str ) => str?.ToLower().Replace( " ", "" );

        public static string TrimTrailingDigits( this string str ) {

            int length = str.Length;
            while (char.IsDigit( str[ length - 1 ] )) { length--; }
            return str[ ..length ];
        }

        public static bool MatchesKey( this string str, string key ) => str.ToKey() == key.ToKey();

        public static string CommaJoin( this IEnumerable< string > items, bool oxfordComma = true ) {

            string[]      array = items.ToArray();
            StringBuilder sb    = new StringBuilder();
            string        final = array.Length > 2 && oxfordComma ? ", and " : " and ";

            for (int i = 0; i < array.Length; i++) {

                if (i == 0) { sb.Append( array[ i ] ); }
                else if (i == array.Length - 1) { sb.Append( final + array[ i ] ); }
                else { sb.Append( ", " + array[ i ] ); }
            }
            
            return sb.ToString();
        }

        public static string IndefiniteArticle( this string thing, bool lowercase = false )
            => thing != null && thing.Length > 1 && "aeiou".Contains( thing.ToLower()[ 0 ] )
                   ? lowercase ? "an" : "An"
                   : lowercase
                       ? "a"
                       : "A";

        public static int? ParseInt( this string str ) => int.TryParse( str, out var n ) ? n : default;
        
        public static float? ParseFloat( this string str ) => float.TryParse( str, out var n ) ? n : default;
    }

}
