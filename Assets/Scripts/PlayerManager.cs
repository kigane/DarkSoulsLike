using UnityEngine;

namespace DarkSoulsLike
{
    public class PlayerManager : MonoBehaviour
    {
        InputHandler inputHandler;
        Animator anim;

        void Start()
        {
            inputHandler = GetComponent<InputHandler>();
            anim = GetComponentInChildren<Animator>();
        }

        void Update()
        {
            inputHandler.isInteracting = anim.GetBool("isInteracting");
            // Log.Debug("PlayerManager-isInteracting: " + inputHandler.isInteracting);
            // inputHandler.rollFlag = false;
            // inputHandler.sprintFlag = false;
        }
    }
}
