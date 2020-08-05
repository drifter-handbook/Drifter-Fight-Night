using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerStatusEffect
{
    END_LAG, KNOCKBACK, INVULN
}

public class PlayerStatus : MonoBehaviour
{
    Dictionary<PlayerStatusEffect, int> statusEffects = new Dictionary<PlayerStatusEffect, int>();

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public bool HasInulvernability(PlayerStatusEffect ef)
    {
        return HasStatusEffect(PlayerStatusEffect.INVULN);
    }
    public bool HasStatusEffect(PlayerStatusEffect ef)
    {
        return statusEffects.ContainsKey(ef) && statusEffects[ef] > 0;
    }
    public bool HasStunEffect()
    {
        return HasStatusEffect(PlayerStatusEffect.END_LAG) || HasStatusEffect(PlayerStatusEffect.KNOCKBACK);
    }
    public bool HasGroundFriction()
    {
        return !HasStatusEffect(PlayerStatusEffect.KNOCKBACK);
    }
    public void ApplyStatusEffect(PlayerStatusEffect ef, float duration)
    {
        StartCoroutine(ApplyStatusEffectFor(ef, duration));
    }

    IEnumerator ApplyStatusEffectFor(PlayerStatusEffect ef, float duration)
    {
        if (!statusEffects.ContainsKey(ef))
        {
            statusEffects[ef] = 0;
        }
        statusEffects[ef]++;
        yield return new WaitForSeconds(duration);
        statusEffects[ef]--;
    }
}
