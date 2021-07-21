using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static System.Math;

[CreateAssetMenu(fileName = "AttackFXSystem", menuName = "VirtuaDrifter/AttackFXSystem", order = 90)]
public class AttackFXSystem : ScriptableObject
{
    #if UNITY_EDITOR
    [Header("Particles which will draw along the launch angle, scaling with damage dealt")]
    #endif

    [SerializeField] private HitSpark[] mainFlyouts;
    [SerializeField] private int offsetPrimary;
    [SerializeField] private int minimumMainFlyouts;
    [SerializeField] private int maximumMainFlyouts;
    
    #if UNITY_EDITOR
    [Header("Particles which will draw with some variance around the target, scaling with damage dealt")]
    #endif
    [SerializeField] private HitSpark[] secondaryFlyouts;
    [SerializeField] private int offsetSecondary;
    [SerializeField] private int minimumSecondaryFlyouts;
    [SerializeField] private int maximumSecondaryFlyouts;

    #if UNITY_EDITOR
    [Header("Particles which will draw with some variance around the target, scaling with damage dealt")]
    #endif
    [SerializeField] private HitSpark[] tertiaryFlyouts;
    [SerializeField] private int offsetTertiary;
    [SerializeField] private int minimumTertiaryFlyouts;
    [SerializeField] private int maximumTertiaryFlyouts;

    #if UNITY_EDITOR
    [Header("Particles which will draw directly on top of the struck target")]
    #endif
    [SerializeField] private HitSpark[] impacts;

    #if UNITY_EDITOR
    [Header("Particles which will draw in random locations around the hit, scaling with damage dealt")]
    #endif
    [SerializeField] private HitSpark[] sparks;

    #if UNITY_EDITOR
    [Header("Other misc particles that may be needed. All will be rendered. used Angled array to decide if individual sparks are rendered angled")]
    #endif
    [SerializeField] private HitSpark[] miscParticles;
    [SerializeField] private bool[] miscIsAngled;

    #if UNITY_EDITOR
    [Header("List of sounds to play on hit, one will be chosen randomly on each hit")]
    #endif
    [SerializeField] private AudioClip[] HitSounds;


    public void TriggerFXSystem(float damage, float hitstun, Vector3 pos, float angle, Vector3 adjustedAngle, Vector2 scale) {
        Vector3 tempOffsetP = adjustedAngle * offsetPrimary;
        Vector3 tempOffsetS = adjustedAngle * offsetSecondary;
        Vector3 tempOffsetT = adjustedAngle * offsetTertiary;

        for (int i = 0; i < Min(maximumMainFlyouts, minimumMainFlyouts + damage / 10); i++)
        {
            float tempAngle = Random.Range(-8, 8);
            Vector3 tempTempOffset = Quaternion.Euler(0, 0, tempAngle) * tempOffsetP;
            GraphicalEffectManager.Instance.CreateHitSparks(mainFlyouts[Random.Range(0, mainFlyouts.Length - 1)], pos + tempTempOffset, angle + tempAngle, scale);
        }
            
        for (int i = 0; i < Min(maximumSecondaryFlyouts, minimumSecondaryFlyouts + damage / 10); i++)
        {
            float tempAngle = Random.Range(-25, 25);
            Vector3 tempTempOffset = Quaternion.Euler(0, 0, tempAngle) * tempOffsetS;
            GraphicalEffectManager.Instance.CreateHitSparks(secondaryFlyouts[Random.Range(0, secondaryFlyouts.Length - 1)], pos + tempTempOffset, angle + 180f + tempAngle, scale);
        }

        for (int i = 0; i < Min(maximumTertiaryFlyouts, minimumTertiaryFlyouts + damage / 5); i++)
        {
            float tempAngle = Random.Range(-80, 80);
            Vector3 tempTempOffset = Quaternion.Euler(0, 0, tempAngle) * tempOffsetT;
            GraphicalEffectManager.Instance.CreateHitSparks(tertiaryFlyouts[Random.Range(0, tertiaryFlyouts.Length - 1)], pos + tempTempOffset, angle + tempAngle, scale);
        }
                
        foreach (HitSpark impact in impacts)
            GraphicalEffectManager.Instance.CreateHitSparks(impact, pos, 0, scale);

        for (int i = 0; i < miscParticles.Length; i++) {
            float tempAngle = angle;
            if (i >= miscIsAngled.Length || !miscIsAngled[i])
                tempAngle = 0;
            GraphicalEffectManager.Instance.CreateHitSparks(miscParticles[i], pos, tempAngle, scale);
        }
    }

    public HitSpark GetSpark() {
        if (sparks.Length > 0)
            return sparks[Random.Range(0, sparks.Length - 1)];
        else
            return HitSpark.NONE;
    }

}
