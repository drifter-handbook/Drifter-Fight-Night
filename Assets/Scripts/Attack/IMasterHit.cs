using UnityEngine;

public interface IMasterHit
{
    void callTheAerial();
    void hitTheAerial(GameObject target);
    void cancelTheAerial();
    void callTheLight();
    void hitTheLight(GameObject target);
    void cancelTheLight();
    void callTheGrab();
    void hitTheGrab(GameObject target);
    void cancelTheGrab();
    void callTheRecovery();
    void hitTheRecovery(GameObject target);
    void cancelTheRecovery();
    //Side W
    void callTheSideW();
    void hitTheSideW(GameObject target);
    void cancelTheSideW();
    //Down W
    void callTheDownW();
    void hitTheDownW(GameObject target);
    void cancelTheDownW();
    //Neutral W
    void callTheNeutralW();
    void hitTheNeutralW(GameObject target);
    void cancelTheNeutralW();
    //Roll
    void callTheRoll();
    void hitTheRoll(GameObject target);
    void cancelTheRoll();
}
