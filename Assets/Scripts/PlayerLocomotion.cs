using System;
using UnityEngine;

namespace DarkSoulsLike
{
    public class PlayerLocomotion : MonoBehaviour
    {
        private Transform cameraTransform;
        private InputHandler inputHandler;
        private Vector3 moveDirection;
        private Transform myTransform;
        private AnimatorHandler animatorHandler;

        // 这里new关键字表示显式隐藏基类的同名成员。
        public new Rigidbody rigidbody;
        public GameObject normalCamera;

        [Header("Stats")]
        [SerializeField]
        private float movementSpeed = 5f;
        [SerializeField]
        private float sprintSpeed = 7f;
        [SerializeField]
        private float rotationSpeed = 10f;

        public bool canRoll = true;

        private void Start()
        {
            rigidbody = GetComponent<Rigidbody>();
            inputHandler = GetComponent<InputHandler>();
            animatorHandler = GetComponentInChildren<AnimatorHandler>();
            cameraTransform = Camera.main.transform;
            myTransform = transform;
            animatorHandler.Initialize();
        }

        #region Movement
        private Vector3 normalVector = Vector3.up;
        private Vector3 targetPosition;

        /// <summary>
        /// 人物转向
        /// 1.将输入信号转为方向向量 
        /// 2.转为旋转角度 Quaternion.LookRotation
        /// 3.让人物逐渐跟随到旋转角度 Quaternion.Slerp
        /// </summary>
        /// <param name="delta"></param>
        private void HandleRotation(float delta)
        {
            Vector3 targetDir;
            // float moveOverride = inputHandler.moveAmount;

            targetDir = cameraTransform.forward * inputHandler.vertical;
            targetDir += cameraTransform.right * inputHandler.horizontal;

            // normalized会返回标准化后的向量，但原值不会改变。而Normalize()会将原值标准化。
            targetDir.Normalize();
            targetDir.y = 0;

            if (targetDir == Vector3.zero)
                targetDir = myTransform.forward;

            float rs = rotationSpeed;
            Quaternion tr = Quaternion.LookRotation(targetDir);
            Quaternion targetRotation = Quaternion.Slerp(myTransform.rotation, tr, rs * delta);
            myTransform.rotation = targetRotation;
        }

        public void HandleMovement(float delta)
        {
            // 翻滚时不能移动
            if (inputHandler.rollFlag)
                return;

            // 根据相机朝向和输入移动人物
            moveDirection = cameraTransform.forward * inputHandler.vertical;
            moveDirection += cameraTransform.right * inputHandler.horizontal;
            moveDirection.Normalize();

            float speed = movementSpeed;
            if (inputHandler.sprintFlag)
            {
                speed = sprintSpeed;
                // isSprinting = true;
            }
            else if (Mathf.Abs(inputHandler.moveAmount) < 0.55f)
            {
                moveDirection /= 2;
            }
            moveDirection *= speed;

            Vector3 projectedVelocity = Vector3.ProjectOnPlane(moveDirection, normalVector);

            rigidbody.velocity = projectedVelocity;

            animatorHandler.UpdateAnimatorValues(inputHandler.moveAmount, 0, inputHandler.sprintFlag);

            if (animatorHandler.canRotate)
            {
                HandleRotation(delta);
            }
        }

        public void HandleRollingAndSprinting(float delta)
        {
            if (animatorHandler.anim.GetBool("isInteracting"))
                return;

            if (inputHandler.rollFlag && canRoll)
            {
                // 翻滚时，rollFlag = true, canRoll = false
                // 结束时, rollFlag = false, canRoll = true
                // 这样在翻滚中，再次输入翻滚不会被记录。因此，连续翻滚必须在一次翻滚结束后再次输入。
                canRoll = false;
                moveDirection = cameraTransform.forward * inputHandler.vertical;
                moveDirection += cameraTransform.right * inputHandler.horizontal;

                if (inputHandler.moveAmount > 0)
                {
                    animatorHandler.PlayTargetAnimation("Roll", true);
                    moveDirection.y = 0;
                    Quaternion rollRotation = Quaternion.LookRotation(moveDirection);
                    myTransform.rotation = rollRotation;
                }
                else
                {
                    animatorHandler.PlayTargetAnimation("Backflip", true);
                }
            }
        }

        // 在翻滚相关动画的最后一帧的动画事件中被调用
        public void ResetRollFlags()
        {
            canRoll = true;
            inputHandler.rollFlag = false;
        }
        #endregion

    }
}
