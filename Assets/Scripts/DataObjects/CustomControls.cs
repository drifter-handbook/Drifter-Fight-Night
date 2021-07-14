using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CustomControls", menuName = "VirtuaDrifter/CustomControls", order = 52)]
public class CustomControls : ScriptableObject
{
    [SerializeField] public KeyCode upKey;
    [SerializeField] public KeyCode downKey;
    [SerializeField] public KeyCode leftKey;
    [SerializeField] public KeyCode rightKey;

    [SerializeField] public KeyCode guard1Key;
    [SerializeField] public KeyCode guard2Key;
    [SerializeField] public KeyCode jumpKey;
    [SerializeField] public KeyCode jumpKeyAlt;

    [SerializeField] public KeyCode lightKey;
    [SerializeField] public KeyCode specialKey;
    [SerializeField] public KeyCode superKey;
}
