using UnityEngine;

public class CameraManager : Singleton<CameraManager>
{
    protected override bool Persistent => false;

    [SerializeField] private Camera _camera;
    [SerializeField] private Transform gameCam;

    protected override void OnAwake()
    {
        SetPositionAndRotation(gameCam.position, gameCam.rotation);
    }

    public void SetPositionAndRotation(Vector3 position, Quaternion rotation)
    {
        _camera.transform.position = position;
        _camera.transform.rotation = rotation;
    }
}