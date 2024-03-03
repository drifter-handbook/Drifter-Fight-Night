using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TrainingUIManager : MonoBehaviour
{

    public PlayerStatus attacker;
    public PlayerStatus defender;

    public TrainingDummyHandler DummyHandler;

    int attackerFrame = 0;
    int defenderFrame = 0;

    bool monitoringFrameAdvantage = false;

    int combo = 0;

    float comboDamage = 0;

    [SerializeField] private TextMeshProUGUI ComboDisplayText;
    [SerializeField] private TextMeshProUGUI FrameDisplayText;
    [SerializeField] private TextMeshProUGUI DamageDisplayText;
        
    // public void WriteCombo(int combo) {
    //     string text = combo.ToString("00");
    //     ComboDisplayText.text = text;
    // }

    // public void WriteFrame(int frame) {
    //     string text = frame.ToString();
    //     if (frame >= 0)
    //         text = "+" + text;
    //     FrameDisplayText.text = text;
    // }

    // public void WriteDamage(float damage) {
    //     string text = damage.ToString();
    //     DamageDisplayText.text = text;
    // }

    void FixedUpdate()
    {
        if(monitoringFrameAdvantage)
        {
            if(attacker.HasStunEffect())
                attackerFrame++;
            if(defender.HasEnemyStunEffect())
                defenderFrame++;

            int frame = defenderFrame - attackerFrame;

            string text = frame.ToString();
            if (frame >= 0)
                text = "+" + text;

            FrameDisplayText.text = text;
            ComboDisplayText.text = combo.ToString("00");
            DamageDisplayText.text = comboDamage.ToString("0.0");

            if(!attacker.HasStunEffect() && !defender.HasStunEffect())
                monitoringFrameAdvantage = false;
        }
    }

    public void readFrameAdvantage(PlayerStatus p_attacker, PlayerStatus p_defender)
    {
        if(monitoringFrameAdvantage && p_defender == defender)
            combo++;
        else
        {
            combo = 1;
            monitoringFrameAdvantage = true;
        }

        attacker = p_attacker;
        defender = p_defender;
        attackerFrame = 0;
        defenderFrame = 0;
        
    }

    public void addDamage(float damage, int hitstun, PlayerStatus p_defender){
       if(monitoringFrameAdvantage)
            comboDamage += damage;
       else
            comboDamage = damage;
       
    }


    // //Combo Counter
    //     if(data.isStun && !data.isSelfInflicted)
    //     {

    //         if(ef == PlayerStatusEffect.DEAD && combocount > 0)
    //         {
                
    //             //UnityEngine.Debug.Log(drifter.drifterType + " got bodied in " + combocount + " hits!");
    //             combocount = 0;
    //             isInCombo = false;
    //             damageDisplay.Reset();
    //         }
    //         else if(ef == PlayerStatusEffect.DEAD)
    //         {
    //             combocount = 0;
    //             isInCombo = false;
    //             damageDisplay.Reset();
    //         }
    //         else
    //         {
    //             combocount++;
    //             //UnityEngine.Debug.Log(combocount + " Hit; " + (frameAdvantage > 0 ?"+":"" ) + frameAdvantage.ToString("0.0"));
    //             frameAdvantage = 0;
    //             isInCombo = true;
    //         }
    //     }

}
