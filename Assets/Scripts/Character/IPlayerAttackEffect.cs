using System.Collections.Generic;

public interface IPlayerAttackEffect
{
    IEnumerator<object> Recovery();
    IEnumerator<object> Light();
    IEnumerator<object> Aerial();
    IEnumerator<object> Grab();
}