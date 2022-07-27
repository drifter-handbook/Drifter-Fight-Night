// collection of data on a single attack
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public enum HitSpark
{
    NONE, POKE, BASH, PIERCE, GRAB, GUARD_STRONG, GUARD_WEAK, SPIKE, MAGICWEAK, CRIT, MAGICSTRONG, OOMPHSPARK, LUCILLE, REFLECT, STAR, STAR_FAST, OOMPHDARK, HEAL, RING, STAR1, STAR2, ORRO_SWEET, DEFAULT_IMPACT, DEFAULT_FLYOUT_PRIMARY, DEFAULT_FLYOUT_SECONDARY, DEFAULT_FLYOUT_TERTIARY
}

public enum HitType
{
    NORMAL, GUARD_CRUSH, GRAB,TRANSCENDANT,BURST
}


[CreateAssetMenu(fileName = "SingleAttackData", menuName = "VirtuaDrifter/SingleAttackData", order = 70)]



public class SingleAttackData : ScriptableObject
{
    #if UNITY_EDITOR
    [Help("All times are in frames. One game frame is .08333 seconds, or 12 frames/second.", UnityEditor.MessageType.Info)]
    #endif
    public float AttackDamage = 10.0f;
    public float Knockback = 10.0f;
    public float KnockbackScale = .5f;
    public float pushBlock = 0f;
    //A value of -1 uses NO baseline hitstun
    #if UNITY_EDITOR
    [Help("Use +/-x to determine advantage on hit or on shield.", UnityEditor.MessageType.Info)]
    #endif
    public bool dynamicStun = false;
    public int firstActiveFrame = 0;
    public int finalFrame = 0;
    public int HitStun = 0;
    public int ShieldStun = 0;

    #if UNITY_EDITOR
    [Help("A negative value will cause the move to base hitpause on the hitstun dealt. A positive value indicates the number of effective frames of hitstun to use in hitstop calculations.", UnityEditor.MessageType.Info)]
    #endif
    public int HitStop= -1;
    //  #if UNITY_EDITOR
    // [Help("Indicates whether or not the duration of the flat hit-stop will scale as a target takes more damage", UnityEditor.MessageType.Info)]
    // #endif
    // public bool ScaleHitStop = true;
    //public float EndLag = 0.1f;
    #if UNITY_EDITOR
    [Help("How does this move interact with shields? Normal is blocked by shields, Grab ignores shields, guard crush applies extra hitstun to shields, Trancendant will always hit not matter what", UnityEditor.MessageType.Info)]
    #endif
    public HitType hitType = HitType.NORMAL;
   
    public bool mirrorKnockback = false;
    public float AngleOfImpact = 45f;
    
    
    public PlayerStatusEffect StatusEffect = PlayerStatusEffect.HIT;
    public float StatusDuration = .1f;
    public AttackFXSystem HitFX = null;

    public bool canHitGrounded = true;
    public bool canHitAerial = true;

    //public bool knockDown = false;
    public bool canHitKnockedDown = false;

    #if UNITY_EDITOR
    [Help("The percentage range in which knockback scaling applies. -1 is unbounded. Scaling is not prorated and will begin from 0 at the floor.", UnityEditor.MessageType.Info)]
    #endif
    public float scalingLowerBound = -1;
    public float scalingUpperBound = -1;


    #if UNITY_EDITOR
        [CustomEditor(typeof(SingleAttackData))]
        public class SingleAttackDataEditor : Editor {
            public override void OnInspectorGUI() {
                
                GUIStyle title = new GUIStyle();
                title.fontStyle = FontStyle.Bold;
                title.normal.textColor = Color.white;


                SingleAttackData data = (SingleAttackData)target;

                EditorGUILayout.LabelField("Hit Data", title);
                EditorGUILayout.Space();
                
                    data.AttackDamage = EditorGUILayout.FloatField("Damage", data.AttackDamage);
                    EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Knockback", GUILayout.MaxWidth(70));
                        data.Knockback = EditorGUILayout.FloatField(data.Knockback, GUILayout.MaxWidth(50));
                        EditorGUILayout.LabelField("Scale", GUILayout.MaxWidth(40));
                        data.KnockbackScale = EditorGUILayout.FloatField(data.KnockbackScale, GUILayout.MaxWidth(50));
                        EditorGUILayout.LabelField("Push Block", GUILayout.MaxWidth(70));
                        data.pushBlock = EditorGUILayout.FloatField(data.pushBlock, GUILayout.MaxWidth(50));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                        data.scalingLowerBound = EditorGUILayout.FloatField("Scaling Floor", data.scalingLowerBound);
                        data.scalingUpperBound = EditorGUILayout.FloatField("Scaling Ceiling", data.scalingUpperBound);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                        data.AngleOfImpact = EditorGUILayout.FloatField("Knockback Angle", data.AngleOfImpact);
                        data.mirrorKnockback = EditorGUILayout.Toggle("Mirrored", data.mirrorKnockback);
                    EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Frame Data", title);
                EditorGUILayout.Space();

                    data.dynamicStun = EditorGUILayout.Toggle("Dynamic Hitstun", data.dynamicStun);
                    EditorGUILayout.BeginHorizontal();
                        if (data.dynamicStun) {
                            EditorGUILayout.LabelField("Active frame", GUILayout.MaxWidth(75));
                            data.firstActiveFrame =  EditorGUILayout.IntField(data.firstActiveFrame, GUILayout.MaxWidth(25));
                            EditorGUILayout.LabelField("End Frame", GUILayout.MaxWidth(65));
                            data.finalFrame =  EditorGUILayout.IntField(data.finalFrame, GUILayout.MaxWidth(25));
                        }
                        EditorGUILayout.LabelField("Hitstun", GUILayout.MaxWidth(45));
                        data.HitStun =  EditorGUILayout.IntField(data.HitStun, GUILayout.MaxWidth(25));
                        EditorGUILayout.LabelField("Blockstun", GUILayout.MaxWidth(60));
                        data.ShieldStun = EditorGUILayout.IntField(data.ShieldStun, GUILayout.MaxWidth(25));
                    EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Hit Properties", title);
                EditorGUILayout.Space();

                    data.hitType = (HitType)EditorGUILayout.EnumPopup("Hit Type", data.hitType);
                    data.HitStop = EditorGUILayout.IntField("Hitstop", data.HitStop);
                    EditorGUILayout.BeginHorizontal();
                        data.StatusEffect = (PlayerStatusEffect)EditorGUILayout.EnumPopup("Status Effect", data.StatusEffect);
                        data.StatusDuration = EditorGUILayout.FloatField("Duration", data.StatusDuration);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Hits Grounded", GUILayout.MaxWidth(90));
                        data.canHitGrounded = EditorGUILayout.Toggle(data.canHitGrounded);
                        EditorGUILayout.LabelField("Hits Aerial", GUILayout.MaxWidth(70));
                        data.canHitAerial = EditorGUILayout.Toggle(data.canHitAerial);
                        EditorGUILayout.LabelField("Hits OTG", GUILayout.MaxWidth(70));
                        data.canHitKnockedDown = EditorGUILayout.Toggle(data.canHitKnockedDown);
                    EditorGUILayout.EndHorizontal();
                    data.HitFX = (AttackFXSystem)EditorGUILayout.ObjectField("Status Effect", data.HitFX, typeof(AttackFXSystem), false);
            }
        }
    #endif
}
