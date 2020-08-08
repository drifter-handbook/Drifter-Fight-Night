using System.Collections.Generic;
using UnityEngine;

public class ClampObjectToScreenSpace : MonoBehaviour
{
    public static Vector3 FindPosition(Transform tfm)
    {
        Vector2 minPos = Camera.main.ScreenToWorldPoint(new Vector3(0f, 0f));
        Vector2 maxPos = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height));
        return new Vector3(
            Mathf.Clamp(tfm.position.x, minPos.x, maxPos.x),
            Mathf.Clamp(tfm.position.y, minPos.y, maxPos.y),
            tfm.position.z
        );
    }

    // FIXME: I don't know enough about math to change this
    public static Vector3 FindNearestOctagonalAngle(Transform tfm)
    {
        Vector2 center = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2));
        List<Vector2> bounds = new List<Vector2>()
            {
                new Vector3(Screen.width, Screen.height / 2),
                new Vector3(Screen.width, Screen.height),
                new Vector3(Screen.width / 2, Screen.height),
                new Vector3(0f, Screen.height),
                new Vector3(0f, Screen.height / 2),
                new Vector3(0f, 0f),
                new Vector3(Screen.width / 2, 0f),
                new Vector3(Screen.width, 0f)
            };
        float angle = Vector2.SignedAngle(Vector2.right, (Vector2)tfm.position - center);
        float bestAngle = 0f;
        float min = float.MaxValue;
        for (int i = 0; i < bounds.Count; i++)
        {
            Vector2 pos = Camera.main.ScreenToWorldPoint(bounds[i]);
            float posAngle = Vector2.SignedAngle(Vector2.right, pos - center);
            float deltaAngle = Mathf.Abs(Mathf.DeltaAngle(angle, posAngle));
            if (deltaAngle < min)
            {
                bestAngle = posAngle;
                min = deltaAngle;
            }
        }
        return new Vector3(0f, 0f, bestAngle);
    }

}