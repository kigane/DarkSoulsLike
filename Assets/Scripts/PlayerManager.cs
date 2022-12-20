using UnityEngine;

namespace DarkSoulsLike
{
    // 负责更新
    // 负责各种flag   
    public class PlayerManager : MonoBehaviour
    {
        private InputHandler inputHandler;
        private Animator anim;
        private CameraHandler cameraHandler;
        private PlayerLocomotion playerLocomotion;

        [Header("Player Flags")]
        public bool isInteracting;

        void Start()
        {
            cameraHandler = CameraHandler.singleton;
            inputHandler = GetComponent<InputHandler>();
            anim = GetComponentInChildren<Animator>();
            playerLocomotion = GetComponent<PlayerLocomotion>();
        }

        void Update()
        {
            float delta = Time.deltaTime;

            inputHandler.TickInput(delta);
            playerLocomotion.HandleMovement(delta);
            playerLocomotion.HandleRollingAndSprinting(delta);
        }

        private void FixedUpdate()
        {
            float delta = Time.fixedDeltaTime;

            if (cameraHandler != null)
            {
                cameraHandler.FollowTarget(delta);
                cameraHandler.HandleCameraRotation(delta, inputHandler.mouseX, inputHandler.mouseY);
            }
        }

        // LateUpdate可以存放一些flag的处理代码
        private void LateUpdate()
        {
            isInteracting = anim.GetBool("isInteracting");
            // Log.Debug("PlayerManager-isInteracting: " + inputHandler.isInteracting);
        }
    }
}
