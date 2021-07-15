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

    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (persistTick > 0) {
            persistTick -= Time.deltaTime;
            if (persistTick <= 0) {
                GameObject floaterClone = Instantiate(floater, gameObject.transform.position, gameObject.transform.rotation);
                floaterClone.GetComponent<PlayerDamageFloater>().InitializeValues(accumilatedDamage);
                gameObject.SetActive(false);
            }
        }
    }

    public void Increment(float damage, bool isCombo, float hitstun) {
        gameObject.SetActive(true);
        if (isCombo) {
            accumilatedDamage += damage;
        } else {
            accumilatedDamage = damage;
        }
        persistTick = 1 + hitstun;
        DamageDisplayText.text = accumilatedDamage.ToString();
    }

    public void Reset() {
        gameObject.SetActive(false);
        accumilatedDamage = 0;
    }
}
