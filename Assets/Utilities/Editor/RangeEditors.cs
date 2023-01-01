using UnityEngine;
using UnityEditor;

namespace Lx {

    
    [CustomPropertyDrawer( typeof( IntRange ) ), CustomPropertyDrawer( typeof( FloatRange ) )]
    public class NumberRangeDrawer: SingleLinePropertyDrawer {

        public override void DrawControl( Rect position, SerializedProperty property )
            => EditorUtils.DrawRangeControl( position, property.Find( "min" ), property.Find( "max" ) );
    }
}
