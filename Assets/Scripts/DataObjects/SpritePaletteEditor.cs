#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpritePalette))]
public class SpritePaletteEditor : Editor
{
    public override void OnInspectorGUI()
    {
        
        base.OnInspectorGUI();
        var script = (SpritePalette)target;

            if(GUILayout.Button("Generate Palette", GUILayout.Height(40)))
            {
                    script.GeneratePalette();
            }
        
    }
    
}
#endif