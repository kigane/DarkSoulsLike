using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DarkSoulsLike
{
    public class InputHandler : MonoBehaviour
    {
        public float horizontal;
        public float vertical;
        // 移动幅度，用于决定人物的行动模式是走还是跑。
        public float moveAmount;
        public float mouseX;
        public float mouseY;

        public bool b_input;
        public bool rollFlag;
        public bool sprintFlag;
        public float rollInputTimer;
        public bool isInteracting;

        private PlayerControls inputActions;
        private CameraHandler cameraHandler;

        private Vector2 movementInput;
        private Vector2 cameraInput;

        private void Awake()
        {
            cameraHandler = CameraHandler.singleton;
        }

        private void FixedUpdate()
        {
            float delta = Time.fixedDeltaTime;

            if (cameraHandler != null)
            {
                cameraHandler.FollowTarget(delta);
                cameraHandler.HandleCameraRotation(delta, mouseX, mouseY);
            }
        }

        private void OnEnable()
        {
            if (inputActions == null)
            {
                inputActions = new PlayerControls();
                inputActions.PlayerMovement.Move.performed += inputAction => movementInput = inputAction.ReadValue<Vector2>();
                inputActions.PlayerMovement.Camera.performed += inputAction => cameraInput = inputAction.ReadValue<Vector2>();
            }

            inputActions.Enable();
            inputActions.PlayerActions.Enable();
            inputActions.PlayerActions.Roll.performed += ctx => rollFlag = true;
            inputActions.PlayerActions.Sprint.performed += ctx => sprintFlag = true;
            inputActions.PlayerActions.Sprint.canceled += ctx => sprintFlag = false;
        }

        private void OnDisable()
        {
            inputActions.Disable();
        }

        public void TickInput(float delta)
        {
            MoveInput(delta);
            // HandleRollInput(delta);
        }

        private void MoveInput(float delta)
        {
            horizontal = movementInput.x;
            vertical = movementInput.y;
            moveAmount = Mathf.Clamp01(Mathf.Abs(horizontal) + Mathf.Abs(vertical));
            mouseX = cameraInput.x;
            mouseY = cameraInput.y;
        }

        // private void HandleRollInput(float delta)
        // {
        //     b_input = inputActions.PlayerActions.Roll.phase == UnityEngine.InputSystem.InputActionPhase.Performed;

        //     // Log.Debug("B: " + b_input);
        //     // press只有waiting和performed两种状态
        //     // tap只有waiting和stared两种状态
        //     Log.Debug(inputActions.PlayerActions.Roll.phase);

        //     if (b_input) // 点按翻滚，长按冲刺
        //     {
        //         rollInputTimer += delta;
        //         sprintFlag = true;
        //     }
        //     else
        //     {
        //         if (rollInputTimer > 0 && rollInputTimer < 0.5f)
        //         {
        //             sprintFlag = false;
        //             rollFlag = true;
        //         }

        //         rollInputTimer = 0;
        //     }
        // }
    }
}
