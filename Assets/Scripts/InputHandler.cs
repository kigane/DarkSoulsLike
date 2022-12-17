using UnityEngine;

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

        PlayerControls inputActions;

        Vector2 movementInput;
        Vector2 cameraInput;

        private void OnEnable()
        {
            if (inputActions == null)
            {
                inputActions = new PlayerControls();
                inputActions.Player.Move.performed += inputAction => movementInput = inputAction.ReadValue<Vector2>();
                inputActions.Player.Camera.performed += inputAction => cameraInput = inputAction.ReadValue<Vector2>();
            }

            inputActions.Enable();
        }

        private void OnDisable()
        {
            inputActions.Disable();
        }

        public void TickInput(float delta)
        {
            MoveInput(delta);
        }

        public void MoveInput(float delta)
        {
            horizontal = movementInput.x;
            vertical = movementInput.y;
            moveAmount = Mathf.Clamp01(Mathf.Abs(horizontal) + Mathf.Abs(vertical));
            mouseX = cameraInput.x;
            mouseY = cameraInput.y;
        }
    }
}
