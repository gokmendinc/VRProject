using UnityEngine;
using static UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics.HapticsUtility;

public class GravityBehaviour : MonoBehaviour
{
    public CharacterController characterController;
    float gravity = -9.81f;
    Vector3 velocity;
    void Update()
    {

        // 🔴 ADD IT HERE
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -5f; // stick to ground
        }

        // apply gravity
        velocity.y += gravity * Time.deltaTime;

        // 🔴 OPTIONAL BUT VERY IMPORTANT (helps your exact issue)
        characterController.Move(Vector3.down * 0.05f);
    }
}
