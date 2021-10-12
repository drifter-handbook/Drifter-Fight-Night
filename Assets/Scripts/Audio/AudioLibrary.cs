using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "AudioLibrary", menuName = "VirtuaDrifter/AudioLibrary", order = 100)]
public class AudioLibrary : ScriptableObject
{


    [Serializable] public struct StringClipPair {
        public string name;
        public AudioClip clip;
    }

    //THIS BREAKS THE BUILD IF LEFT IN I DONT KNOW WHAT IT DOES

    // // IngredientDrawer
    // [CustomPropertyDrawer(typeof(StringClipPair))]
    // public class StringClipDrawer : PropertyDrawer
    // {
    //     // Draw the property inside the given rect
    //     public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    //     {
    //         // Using BeginProperty / EndProperty on the parent property means that
    //         // prefab override logic works on the entire property.
    //         EditorGUI.BeginProperty(position, label, property);

    //         // Draw label
    //         position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

    //         // Don't make child fields be indented
    //         var indent = EditorGUI.indentLevel;
    //         EditorGUI.indentLevel = 0;

    //         // Calculate rects
    //         var nameRect = new Rect(position.x + 0, position.y, position.width - 120, position.height);
    //         var clipRect = new Rect(position.x + position.width - 120, position.y, 120, position.height);

    //         // Draw fields - passs GUIContent.none to each so they are drawn without labels
    //         EditorGUI.PropertyField(nameRect, property.FindPropertyRelative("name"), GUIContent.none);
    //         EditorGUI.PropertyField(clipRect, property.FindPropertyRelative("clip"), GUIContent.none);

    //         // Set indent back to what it was
    //         EditorGUI.indentLevel = indent;

    //         EditorGUI.EndProperty();
    //     }
    // }

    [SerializeField] private StringClipPair[] library;
    private Dictionary<string, short> map;

    public void BuildLibrary() {
        map = new Dictionary<string, short>();
        for (short i = 0; i < library.Length; i++)
            map.Add(library[i].name, i);
    }

    public short FetchID(string name) {
        return map[name];
    }

    public AudioClip FetchClip(short id) {
        return library[id].clip;
    }

    public AudioClip FetchClip(string name) {
        return library[map[name]].clip;
    }
}



