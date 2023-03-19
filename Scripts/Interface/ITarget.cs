using UnityEngine;

public interface ITarget
{
    bool GetIsDead();

    Vector3 GetMeshCenter();

    Vector3 GetPositionToTarget();
}