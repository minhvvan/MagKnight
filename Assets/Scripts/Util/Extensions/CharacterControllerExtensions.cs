using UnityEngine;

public static class CharacterControllerExtensions
{
    public static void TeleportByTransform(this CharacterController controller, GameObject player, Transform targetTransform)
    {
        controller.enabled = false;
        player.transform.position = targetTransform.position;
        player.transform.rotation = targetTransform.rotation;
        controller.enabled = true;
    }
    
    public static void Teleport(this CharacterController controller, GameObject player, Vector3 targetPosition, Quaternion targetRotation)
    {
        controller.enabled = false;
        player.transform.position = targetPosition;
        player.transform.rotation = targetRotation;
        controller.enabled = true;
    }
}