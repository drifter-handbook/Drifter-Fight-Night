using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerDamageFloater : MonoBehaviour
{

    [SerializeField] private TextMeshPro DamageDisplayText;
    private int persistTick = 0;
    private float displayValue = 0;
    private readonly int tickDelta = 10;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = gameObject.transform.position + new Vector3(0 , 0.01f, 0);
        gameObject.transform.position = pos;
        Color32 alpha = DamageDisplayText.faceColor;
        if (persistTick > 255)
            alpha.a = 255;
        else
            alpha.a = (byte)persistTick;
        DamageDisplayText.faceColor = alpha;

        persistTick--;

        if (persistTick <= (displayValue * tickDelta) && persistTick % tickDelta == 0) {
            displayValue--;
            DamageDisplayText.text = displayValue.ToString();
        }

        if (persistTick <= 0)
            Destroy(gameObject);
    }

    public void InitializeValues(float damage, bool isTrue) {
        DamageDisplayText.text = damage.ToString();
        displayValue = damage;
        gameObject.transform.localScale = Vector3.one * (1 + displayValue / 128);
        persistTick = (int)(255 + displayValue * tickDelta);
        if (isTrue) 
            DamageDisplayText.faceColor = Color.red;
        else
            DamageDisplayText.faceColor = Color.yellow;
    }
}
