using Cinemachine;
using UnityEngine;

public static class CinemachineImpulseController
{
    private static CinemachineImpulseSource _impulseSource;     

    
    public static void Initialize()
    {
        _impulseSource = GameObject.FindObjectOfType<CinemachineImpulseSource>();
        if (_impulseSource == null)
        {
            Debug.LogError("CinemachineImpulseSource not found in the scene.");
        }
    }

    public static void GenerateImpulse()
    {
        if (_impulseSource == null)
        {
            Initialize();
            return;
        }
        
        _impulseSource.GenerateImpulse();
    }
    
}