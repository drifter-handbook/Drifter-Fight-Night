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
    [SerializeField] private int minimumMainFlyouts;
    [SerializeField] private int maximumMainFlyouts;
    
    #if UNITY_EDITOR
    [Header("Particles which will draw with some variance around the target, scaling with damage dealt")]
    #endif
    [SerializeField] private HitSpark[] secondaryFlyouts;
    [SerializeField] private int minimumSecondaryFlyouts;
    [SerializeField] private int maximumSecondaryFlyouts;

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


    public void TriggerFXSystem(float damage, float hitstun, Vector3 pos, float angle, Vector2 scale) {

        for (int i = 0; i < Min(maximumMainFlyouts, minimumMainFlyouts + damage / 10); i++)
            GraphicalEffectManager.Instance.CreateHitSparks(mainFlyouts[Random.Range(0, mainFlyouts.Length - 1)], pos, angle, scale);

        for (int i = 0; i < Min(maximumSecondaryFlyouts, minimumSecondaryFlyouts + damage / 10); i++)
            GraphicalEffectManager.Instance.CreateHitSparks(secondaryFlyouts[Random.Range(0, secondaryFlyouts.Length - 1)], pos, angle, scale);

        foreach (HitSpark impact in impacts)
            GraphicalEffectManager.Instance.CreateHitSparks(impact, pos, 0, scale);

        for (int i = 0; i < miscParticles.Length; i++) {
            float tempAngle = angle;
            if (i >= miscIsAngled.Length || !miscIsAngled[i])
                tempAngle = 0;
            GraphicalEffectManager.Instance.CreateHitSparks(miscParticles[i], pos, tempAngle, scale);
        }

        //if (damage > 0f) 
        //    StartCoroutine(delayHitsparks(pos, angle, damage, hitstun *.25f));
    }

/*
    private IEnumerator delayHitsparks(Vector3 position, float angle,float damage, float duration)
    {
        Vector3 hitSparkPos = position;
        float angleT;
        float stepSize = duration / ((damage + 2 )/3);

        if (damage >= 2.5f) 
            GraphicalEffectManager.Instance.CreateHitSparks(HitSpark.RING, position,angle, new Vector2(10f, 10f));

        for (int i = 0; i < (damage + 2 )/3 ; i++)
        {
            angleT = angle + Random.Range(-45, 45);
            hitSparkPos += Quaternion.Euler(0, 0, angleT) * new Vector3(-Random.Range(1, 4), 0, 0);
            GraphicalEffectManager.Instance.CreateHitSparks(sparks[Random.Range(0, sparks.Length - 1)], position, angleT, new Vector2(10f, 10f));

            angleT += 180;

            hitSparkPos += Quaternion.Euler(0, 0, angleT) * new Vector3(-Random.Range(1, 4), 0, 0);
            GraphicalEffectManager.Instance.CreateHitSparks(sparks[Random.Range(0, sparks.Length - 1)], hitSparkPos, angleT, new Vector2(10f, 10f));

            yield return new WaitForSeconds(stepSize);
        }
        yield break;
    }
*/
}
