using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnAwakeSingleSound : SingleSound
{
    void Awake()
    {
        PlayAudio();
    }
}
