using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static class CreateEditorScriptMenu {
    
    [MenuItem( "Assets/Create/Editor Script", isValidateFunction: true )]
    public static bool CanCreateEditorScript() => Selection.activeObject
                                                  && AssetDatabase.Contains( Selection.activeObject )
                                                  && Selection.activeObject.GetType() == typeof( MonoScript );

    [MenuItem( "Assets/Create/Editor Script", priority = 0)]
    public static void CreateEditorScript() {

        Object selected  = Selection.activeObject;
        string path      = AssetDatabase.GetAssetPath( selected );
        Type   monoClass = ((MonoScript) selected).GetClass();
     
        if (monoClass.IsAbstract && monoClass.IsSealed) {
         
            Debug.LogError( $"Cannot create editor script for static class '{monoClass.Name}'" );
            return;
        }
        
        string editorDirectoryPath = path.Substring( 0, path.LastIndexOf( '/' ) + 1 ) + "Editor/";
        if (!Directory.Exists( editorDirectoryPath )) { Directory.CreateDirectory( editorDirectoryPath ); }
        string filePath = editorDirectoryPath + monoClass.Name + "Editor.cs";

        if (File.Exists( filePath )) {
            
            Debug.LogError( $"Looks like an editor script for {monoClass.Name} already exists at {filePath}" );
            return;
        }
        
        Debug.Log( $"Creating editor script at: {filePath}" );
     
        using StreamWriter sw = new StreamWriter( filePath );
        string indent = "";
     
        sw.WriteLine( "using UnityEngine;" );
        sw.WriteLine( "using UnityEditor;" );
        sw.WriteLine( "using System.Collections;" );
        sw.WriteLine( "using System.Collections.Generic;" );
        sw.WriteLine( "" );
     
        if (!string.IsNullOrWhiteSpace( monoClass.Namespace )) {
         
            indent = "    ";
            sw.WriteLine( $"namespace {monoClass.Namespace} {{" );
            sw.WriteLine( "" );
        }
     
        sw.WriteLine( $"{indent}[CustomEditor( typeof( {monoClass.Name} ) )]" );
        sw.WriteLine( $"{indent}public class {monoClass.Name}Editor: Editor {{" );
        sw.WriteLine( $"{indent}    " );
        sw.WriteLine( $"{indent}    {monoClass.Name} script;" );
        sw.WriteLine( $"{indent}    " );
        sw.WriteLine( $"{indent}    public override void OnInspectorGUI() {{" );
        sw.WriteLine( $"{indent}    " );
        sw.WriteLine( $"{indent}        script ??= target as {monoClass.Name};" );
        sw.WriteLine( $"{indent}        " );
        sw.WriteLine( $"{indent}        base.OnInspectorGUI();" );
        sw.WriteLine( $"{indent}    }}" );
        sw.WriteLine( $"{indent}}}" );
     
        if (!string.IsNullOrWhiteSpace( monoClass.Namespace )) { sw.WriteLine( "}" ); }
     
        AssetDatabase.Refresh();
    }
}
