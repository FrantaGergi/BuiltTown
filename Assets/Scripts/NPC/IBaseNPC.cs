using UnityEngine;

public interface IBaseNPC
{
    void MoveTo(Vector3 position);
    bool IsAtDestination(float tolerance = -1f);
    void Stop();
    void LookAtTarget(Vector3 target);
}
