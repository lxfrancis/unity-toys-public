/***************************************
 *                                     *
 *   ,_,          EnumSet type         *
 *  (o,o)    by Lexa Francis, 2022     *
 *  {`"'}     Uploaded 2022-12-22      *
 *  -"-"- Tested in 2020.3 and 2022.1  *
 *                                     *
 ***************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static UnityEditor.MessageType;

namespace Lx {

    [CustomPropertyDrawer( typeof( EnumSetList<> ))]
    public class EnumSetListDrawer: PropertyDrawer {

        public override void OnGUI( Rect position, SerializedProperty property, GUIContent label ) {
            
            EditorGUI.BeginProperty( position, GUIContent.none, property );

            property.isExpanded = true;

            EditorGUI.PropertyField( position, property.FindPropertyRelative( "items" ), new GUIContent( label ) );
            
            EditorGUI.EndProperty();
        }

        
        public override float GetPropertyHeight( SerializedProperty property, GUIContent label ) {

            SerializedProperty listProperty = property.FindPropertyRelative( "items" );
            
            return !listProperty.isExpanded ? base.GetPropertyHeight( property, label )
                                            : EditorGUI.GetPropertyHeight( listProperty );
        }
    }
    
    
    // TODO: allow alt-clicking to collapse or expand all
    [CustomPropertyDrawer( typeof( EnumSet<,> ))]
    public class EnumSetDrawer: PropertyDrawer {

        static readonly float spacing           = EditorGUIUtility.standardVerticalSpacing;
        static readonly float lineHeight        = EditorGUIUtility.singleLineHeight;
        static readonly float warningsHeight    = lineHeight * 1.5f;
        static readonly float suggestionsHeight = lineHeight * 2.0f;

        string[] keyNames;

        bool               showingWarnings, showingSuggestion;
        SerializedProperty property;

        
        void SetKeyNames() {
            
            Type fieldType = fieldInfo.FieldType;

            if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof( EnumSet<,> )) {
                keyNames = Enum.GetNames( fieldType.GenericTypeArguments[ 0 ] );
            }
            else if (fieldType.IsArray) {
                keyNames = Enum.GetNames( fieldType.GetElementType()!.GenericTypeArguments[ 0 ] );
            }
        }


        static void HelpBox( ref Rect rect, float height, string message, MessageType type ) {

            Rect boxRect   = rect;
            boxRect.height = height;
            EditorGUI.HelpBox( boxRect, message, type );
            rect.y += height + spacing;
        }


        void ShowWarnings( Rect controlRect ) {
            
            Type TValueType = fieldInfo.FieldType.GenericTypeArguments[ 1 ];

            controlRect.y += lineHeight;
            
            HelpBox( ref controlRect, warningsHeight,
                     $"EnumSet values of type {TValueType.Name} cannot be serialised!", Error );

            if (TValueType.IsArray) {

                string arrayElementType = TValueType.GetElementType()?.Name;

                showingSuggestion = true;

                HelpBox( ref controlRect, suggestionsHeight,
                         $"Use EnumSetList< {arrayElementType} > instead of {arrayElementType}[] " +
                         "as EnumSet TValue type", Warning );
                return;
            }

            Type TValueGenericType = TValueType.IsGenericType ? TValueType.GetGenericTypeDefinition() : null;

            if (!typeof( List<> ).IsAssignableFrom( TValueGenericType )) { return; }
            
            string listElementType = TValueType.GenericTypeArguments[ 0 ].Name;

            showingSuggestion = true;
                
            HelpBox( ref controlRect, lineHeight * 2f,
                     $"Use EnumSetList< {listElementType} > instead of List< {listElementType} > " +
                     "as EnumSet TValue type", Warning );
        }


        void ShowElements( Rect position, SerializedProperty valuesProperty ) {
            
            Rect containerRect  = position;
            containerRect.xMin -= spacing;
            containerRect.yMin += lineHeight + spacing;

            EditorGUI.HelpBox( containerRect, "", None );

            // only check for less than - don't kill serialized values in case the enum member lookup failed
            if (valuesProperty?.arraySize < keyNames?.Length) {

                if (valuesProperty.arraySize == 0) {
                    Debug.LogWarning( "EnumSet field is empty (probably uninitialised); " +
                                      $"filling with default values (at {property.propertyPath}" );
                } else {
                    Debug.LogWarning( $"EnumSet field serialised length ({valuesProperty.arraySize}) " +
                                      $"is lower than enum type member count ({keyNames.Length}); " +
                                      $"filling with default values (at {property.propertyPath})" );
                }

                for (int i = valuesProperty.arraySize; i < keyNames.Length; i++) {
                    valuesProperty.InsertArrayElementAtIndex( i );
                }
            }

            position.height = lineHeight;
            position.width -= spacing * 2.5f;
            position.y     += lineHeight + spacing * 2.5f;

            for (int i = 0; i < valuesProperty?.arraySize && i < keyNames?.Length; i++) {

                SerializedProperty elementProperty = valuesProperty.GetArrayElementAtIndex( i );
                GUIContent         elementLabel    = new GUIContent( keyNames[ i ] );
                EditorGUI.PropertyField( position, elementProperty, elementLabel, elementProperty.isExpanded );
                position.y += EditorGUI.GetPropertyHeight( elementProperty, elementLabel, true ) + 1;
            }
        }

        
        public override void OnGUI( Rect position, SerializedProperty serializedProperty, GUIContent label ) {

            property = serializedProperty;

            EditorGUI.BeginProperty( position, label, property );
            
            Rect lineRect   = position;
            lineRect.height = lineHeight;

            var countRect  = lineRect;
            countRect.xMin = countRect.xMax - 48f;

            property.isExpanded = EditorGUI.BeginFoldoutHeaderGroup( lineRect, property.isExpanded, label );
                
            // due to a bug in unity serialisation for enum properties, we have to do this crap
            SetKeyNames();

            SerializedProperty valuesProperty = property.FindPropertyRelative( "m_values" );

            EditorGUI.IntField( countRect, valuesProperty?.arraySize ?? 0 );

            if (valuesProperty == null) { showingWarnings = true; }

            if (property.isExpanded) {
            
                EditorGUI.indentLevel++;

                if (showingWarnings) { ShowWarnings( position ); }
                else { ShowElements( position, valuesProperty ); }

                EditorGUI.indentLevel--;
            }
            
            EditorGUI.EndFoldoutHeaderGroup();
            
            EditorGUI.EndProperty();
        }

        
        public override float GetPropertyHeight( SerializedProperty serializedProperty, GUIContent label ) {
            
            float height = base.GetPropertyHeight( serializedProperty, label );
            
            if (!serializedProperty.isExpanded) { return height; }

            if (showingWarnings) {
                
                height += warningsHeight + spacing;
                if (showingSuggestion) { height += suggestionsHeight + spacing; }
                return height;
            }
            
            SetKeyNames();

            SerializedProperty listProperty = serializedProperty.FindPropertyRelative( "m_values" );
            
            for (int i = 0; i < listProperty?.arraySize && i < keyNames.Length; i++) {
                height += EditorGUI.GetPropertyHeight( listProperty.GetArrayElementAtIndex( i ) ) + 1;
            }
            
            return height + spacing * 3.5f;  // for the spacing around the list
        }
    }
}