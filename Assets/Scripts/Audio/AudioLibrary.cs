using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioLibrary", menuName = "VirtuaDrifter/AudioLibrary", order = 100)]
public class AudioLibrary : ScriptableObject
{
    [Serializable] public struct StringClipPair {
        public string name;
        public AudioClip clip;
    }

    [SerializeField] private StringClipPair[] library;
}



