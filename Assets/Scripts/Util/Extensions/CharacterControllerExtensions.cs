using UnityEngine;

public static partial class CharacterControllerExtensions
{
    public static void Teleport(this CharacterController controller, GameObject player, Transform targetTransform)
    {
        controller.enabled = false;
        player.transform.position = targetTransform.position;
        player.transform.rotation = targetTransform.rotation;
        controller.enabled = true;
    }
}