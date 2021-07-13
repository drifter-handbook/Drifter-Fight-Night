using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TrainingUIManager : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI ComboDisplayText;
    [SerializeField] private TextMeshProUGUI FrameDisplayText;
        
    public void WriteCombo(int combo) {
        string text = combo.ToString("00");
        ComboDisplayText.text = text;
    }

    public void WriteFrame(int frame) {
        string text = frame.ToString();
        if (frame >= 0)
            text = "+" + text;
        FrameDisplayText.text = text;
    }
}
