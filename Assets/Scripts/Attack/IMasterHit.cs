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
}
