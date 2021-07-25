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

    [SerializeField] private HitSpark[] mainFlyouts = {HitSpark.DEFAULT_FLYOUT_PRIMARY};
    [SerializeField] private int offsetPrimary=5;
    [SerializeField] private int minimumMainFlyouts=1;
    [SerializeField] private int maximumMainFlyouts=1;
    [SerializeField] private Color mainFlyoutColor = Color.white;
    
    #if UNITY_EDITOR
    [Header("Particles which will draw with some variance around the target, scaling with damage dealt")]
    #endif
    [SerializeField] private HitSpark[] secondaryFlyouts = {HitSpark.DEFAULT_FLYOUT_SECONDARY};
    [SerializeField] private int offsetSecondary = -6;
    [SerializeField] private int minimumSecondaryFlyouts = 2;
    [SerializeField] private int maximumSecondaryFlyouts = 2;
    [SerializeField] private Color secondaryFlyoutColor = Color.white;

    #if UNITY_EDITOR
    [Header("Particles which will draw with some variance around the target, scaling with damage dealt")]
    #endif
    [SerializeField] private HitSpark[] tertiaryFlyouts = {HitSpark.DEFAULT_FLYOUT_TERTIARY};
    [SerializeField] private int offsetTertiary =3;
    [SerializeField] private int minimumTertiaryFlyouts = 3;
    [SerializeField] private int maximumTertiaryFlyouts = 7;
    [SerializeField] private Color tertiaryFlyoutColor = Color.white;
    

    #if UNITY_EDITOR
    [Header("Particles which will draw directly on top of the struck target")]
    #endif
    [SerializeField] private HitSpark[] impacts = {HitSpark.DEFAULT_IMPACT};
    [SerializeField] private Color impactColor = Color.white;

    #if UNITY_EDITOR
    [Header("Particles which will draw in random locations around the hit, scaling with damage dealt")]
    #endif
    [SerializeField] private HitSpark[] sparks = {HitSpark.STAR1,HitSpark.STAR2};

    #if UNITY_EDITOR
    [Header("Other misc particles that may be needed. All will be rendered. used Angled array to decide if individual sparks are rendered angled")]
    #endif
    [SerializeField] private HitSpark[] miscParticles;
    [SerializeField] private bool[] miscIsAngled;

    #if UNITY_EDITOR
    [Header("List of sounds to play on hit, one will be chosen randomly on each hit")]
    #endif
    [SerializeField] private string[] hitSounds;


    public void TriggerFXSystem(float damage, float hitstun, Vector3 pos, float angle, Vector3 adjustedAngle, Vector2 scale, bool overrideSFX = false) {
        Vector3 tempOffsetP = adjustedAngle * offsetPrimary;
        Vector3 tempOffsetS = adjustedAngle * offsetSecondary;
        Vector3 tempOffsetT = adjustedAngle * offsetTertiary;

        for (int i = 0; i < Min(maximumMainFlyouts, minimumMainFlyouts + damage / 10); i++)
        {
            float tempAngle = Random.Range(-8, 8);
            Vector3 tempTempOffset = Quaternion.Euler(0, 0, tempAngle) * tempOffsetP;
            GraphicalEffectManager.Instance.CreateHitSparks(mainFlyouts[Random.Range(0, mainFlyouts.Length)], pos + tempTempOffset, angle + tempAngle, scale,mainFlyoutColor);
        }
            
        for (int i = 0; i < Min(maximumSecondaryFlyouts, minimumSecondaryFlyouts + damage / 10); i++)
        {
            float tempAngle = Random.Range(-25, 25);
            Vector3 tempTempOffset = Quaternion.Euler(0, 0, tempAngle) * tempOffsetS;
            GraphicalEffectManager.Instance.CreateHitSparks(secondaryFlyouts[Random.Range(0, secondaryFlyouts.Length)], pos + tempTempOffset, angle + 180f + tempAngle, scale,secondaryFlyoutColor);
        }

        for (int i = 0; i < Min(maximumTertiaryFlyouts, minimumTertiaryFlyouts + damage / 5); i++)
        {
            float tempAngle = Random.Range(-80, 80);
            Vector3 tempTempOffset = Quaternion.Euler(0, 0, tempAngle) * tempOffsetT;
            GraphicalEffectManager.Instance.CreateHitSparks(tertiaryFlyouts[Random.Range(0, tertiaryFlyouts.Length)], pos + tempTempOffset, angle + tempAngle, scale,tertiaryFlyoutColor);
        }
                
        foreach (HitSpark impact in impacts)
            GraphicalEffectManager.Instance.CreateHitSparks(impact, pos, 0, scale,impactColor);

        for (int i = 0; i < miscParticles.Length; i++) {
            float tempAngle = angle;
            if (i >= miscIsAngled.Length || !miscIsAngled[i])
                tempAngle = 0;
            GraphicalEffectManager.Instance.CreateHitSparks(miscParticles[i], pos, tempAngle, scale);
        }

        if (hitSounds.Length > 0 && !overrideSFX)
            AudioSystemManager.Instance.CreateSyncedSFX(hitSounds[Random.Range(0, hitSounds.Length)]);
    }

    public HitSpark GetSpark() {
        if (sparks.Length > 0)
            return sparks[Random.Range(0, sparks.Length - 1)];
        else
            return HitSpark.NONE;
    }

}
