using System;
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

   	[SerializeField] public FlyoutDefinition[] mainFlyouts = {new FlyoutDefinition(HitSpark.DEFAULT_FLYOUT_PRIMARY,Color.white)};

   	[SerializeField] private int offsetPrimary=5;
    [SerializeField] private int minimumMainFlyouts=1;
    [SerializeField] private int maximumMainFlyouts=1;
    
    #if UNITY_EDITOR
    [Header("Particles which will draw with some variance around the target, scaling with damage dealt")]
    #endif

    [SerializeField] public FlyoutDefinition[] secondaryFlyouts = {new FlyoutDefinition(HitSpark.DEFAULT_FLYOUT_SECONDARY,Color.white)};

    [SerializeField] private int offsetSecondary = -6;
    [SerializeField] private int minimumSecondaryFlyouts = 2;
    [SerializeField] private int maximumSecondaryFlyouts = 2;
    

    #if UNITY_EDITOR
    [Header("Particles which will draw with some variance around the target, scaling with damage dealt")]
    #endif

    [SerializeField] public FlyoutDefinition[] tertiaryFlyouts = {new FlyoutDefinition(HitSpark.DEFAULT_FLYOUT_TERTIARY,Color.white)};

    [SerializeField] private int offsetTertiary =3;
    [SerializeField] private int minimumTertiaryFlyouts = 3;
    [SerializeField] private int maximumTertiaryFlyouts = 7;


    #if UNITY_EDITOR
    [Header("Particles which will draw directly on top of the struck target")]
    #endif
    [SerializeField] public FlyoutDefinition[] impacts = {new FlyoutDefinition(HitSpark.DEFAULT_IMPACT,Color.white)};

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
            float tempAngle = UnityEngine.Random.Range(-8, 8);
            int tempIndex = UnityEngine.Random.Range(0, mainFlyouts.Length);
            Vector3 tempTempOffset = Quaternion.Euler(0, 0, tempAngle) * tempOffsetP;
            GraphicalEffectManager.Instance.CreateHitSparks(mainFlyouts[tempIndex].flyoutEffect, pos + tempTempOffset, angle + tempAngle, scale,mainFlyouts[tempIndex].flyoutColor);
        }
            
        for (int i = 0; i < Min(maximumSecondaryFlyouts, minimumSecondaryFlyouts + damage / 10); i++)
        {
            float tempAngle = UnityEngine.Random.Range(-25, 25);
            int tempIndex = UnityEngine.Random.Range(0, secondaryFlyouts.Length);
            Vector3 tempTempOffset = Quaternion.Euler(0, 0, tempAngle) * tempOffsetS;
            GraphicalEffectManager.Instance.CreateHitSparks(secondaryFlyouts[tempIndex].flyoutEffect, pos + tempTempOffset, angle + 180f + tempAngle, scale,secondaryFlyouts[tempIndex].flyoutColor);
        }

        for (int i = 0; i < Min(maximumTertiaryFlyouts, minimumTertiaryFlyouts + damage / 5); i++)
        {
            float tempAngle = UnityEngine.Random.Range(-180, 180);
            int tempIndex = UnityEngine.Random.Range(0, tertiaryFlyouts.Length);
            Vector3 tempTempOffset = Quaternion.Euler(0, 0, tempAngle) * (tempOffsetT * UnityEngine.Random.Range(-2f, 2f));
            GraphicalEffectManager.Instance.CreateHitSparks(tertiaryFlyouts[tempIndex].flyoutEffect, pos + tempTempOffset, angle + tempAngle, scale,tertiaryFlyouts[tempIndex].flyoutColor);
        }
                
        foreach (FlyoutDefinition impact in impacts)
            GraphicalEffectManager.Instance.CreateHitSparks(impact.flyoutEffect, pos, 0, scale,impact.flyoutColor);

        for (int i = 0; i < miscParticles.Length; i++) {
            float tempAngle = angle;
            if (i >= miscIsAngled.Length || !miscIsAngled[i])
                tempAngle = 0;
            GraphicalEffectManager.Instance.CreateHitSparks(miscParticles[i], pos, tempAngle, scale);
        }

        if (hitSounds.Length > 0 && !overrideSFX)
            AudioSystemManager.Instance.CreateSyncedSFX(hitSounds[UnityEngine.Random.Range(0, hitSounds.Length)]);
    }

    public HitSpark GetSpark() {
        if (sparks.Length > 0)
            return sparks[UnityEngine.Random.Range(0, sparks.Length - 1)];
        else
            return HitSpark.NONE;
    }  

}

[Serializable]
public struct FlyoutDefinition
{
    public HitSpark flyoutEffect;
    public Color flyoutColor;

	public FlyoutDefinition(HitSpark spark, Color color)
	{
		flyoutEffect = spark;
    	flyoutColor = color;
	}
}
