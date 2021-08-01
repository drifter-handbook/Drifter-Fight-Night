using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerDamageNumbers : MonoBehaviour
{

    [SerializeField] private TextMeshPro DamageDisplayText;
    [SerializeField] private GameObject floater;
    private float accumilatedDamage;
    private float persistTick;
    private bool isTrue;

    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameController.Instance.IsPaused)
            return;
            
        if (persistTick > 0) {
            persistTick -= Time.deltaTime;
            if (persistTick <= 0) {
                GameObject floaterClone = Instantiate(floater, gameObject.transform.position, gameObject.transform.rotation);
                floaterClone.GetComponent<PlayerDamageFloater>().InitializeValues(accumilatedDamage, isTrue);
                gameObject.SetActive(false);
            }
        }
    }

    public void Increment(float damage, bool isCombo, float hitstun) {
        gameObject.SetActive(true);
        if (persistTick > 0) {
            if (isTrue && !isCombo)
            {
                DamageDisplayText.faceColor = Color.yellow;
                isTrue = false;
            }
            accumilatedDamage += damage;
            gameObject.transform.localScale = Vector3.one * (1 + accumilatedDamage / 128);
        } else {
            gameObject.transform.localScale = Vector3.one;
            isTrue = true;
            DamageDisplayText.faceColor = Color.red;
            accumilatedDamage = damage;
        }
        persistTick = 1 + hitstun;
        DamageDisplayText.text = accumilatedDamage.ToString();
    }

    public void Reset() {
        gameObject.SetActive(false);
        persistTick = 0;
        accumilatedDamage = 0;
    }
}
