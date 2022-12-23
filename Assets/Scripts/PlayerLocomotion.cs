using System;
using UnityEngine;

namespace DarkSoulsLike
{
    public class PlayerLocomotion : MonoBehaviour
    {
        private Transform cameraTransform;
        private InputHandler inputHandler;
        private Transform myTransform;
        private AnimatorHandler animatorHandler;
        private PlayerManager playerManager;

        public Vector3 moveDirection;
        // 这里new关键字表示显式隐藏基类的同名成员。
        public new Rigidbody rigidbody;
        public GameObject normalCamera;

        [Header("Ground & Air Detection Stats")]
        [SerializeField]
        private float groundDetectionRayStartPoint = 0.5f; // coll的底部坐标
        [SerializeField]
        private float minimumDetectionNeededToFall = 1.2f; // 大于此值才为掉落，否则为平地或下滑
        [SerializeField]
        private float groundDetectionRayDistance = 0.1f; // 探测移动方向前方此距离是否掉落
        private LayerMask ignoreForGroundCheck;
        public float inAirTimer;

        [Header("Stats")]
        [SerializeField]
        private float movementSpeed = 5f;
        [SerializeField]
        private float sprintSpeed = 7f;
        [SerializeField]
        private float rotationSpeed = 10f;
        [SerializeField]
        private float fallingSpeed = 80f;

        public bool canRoll = true;

        private void Start()
        {
            rigidbody = GetComponent<Rigidbody>();
            inputHandler = GetComponent<InputHandler>();
            playerManager = GetComponent<PlayerManager>();
            animatorHandler = GetComponentInChildren<AnimatorHandler>();
            cameraTransform = Camera.main.transform;
            myTransform = transform;

            animatorHandler.Initialize();

            playerManager.isGrounded = true;
            ignoreForGroundCheck = ~(1 << 8 | 1 << 10);
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
            // 理论上开始下坠后playerManager.isInteracting为true，rigidbody.velocity为下落时设定的值
            // 实际上，上楼梯时，从左右掉落，则rigidbody会和楼梯的coll重叠，导致rigidbody不断将其速度改为0。
            // 因此，上楼梯时掉落，会在边缘卡住。
            Log.Debug("Movement: " + rigidbody.velocity);
            // Log.Debug("isInteracting: " + playerManager.isInteracting);
            if (playerManager.isInteracting)
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
            // Log.Debug("Projected Velocity: " + projectedVelocity);
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

        public void HandleFalling(float delta, Vector3 moveDirection)
        {
            playerManager.isGrounded = false;

            // 射线起点在coll底部
            Vector3 origin = myTransform.position;
            origin.y += groundDetectionRayStartPoint;

            if (Physics.Raycast(origin, myTransform.forward, out _, 0.4f))
            { // 检测前方障碍物。
                moveDirection = Vector3.zero;
            }

            if (playerManager.isInAir)
            { // 已经在空中了
                rigidbody.AddForce(-Vector3.up * fallingSpeed);
                rigidbody.AddForce(moveDirection * fallingSpeed / 5f);
            }


            // 射线向移动方向偏移
            origin += moveDirection.normalized * groundDetectionRayDistance;

            targetPosition = myTransform.position;

            Debug.DrawRay(origin, -Vector3.up * minimumDetectionNeededToFall, Color.red, 0.1f, false);

            if (Physics.Raycast(origin, -Vector3.up, out RaycastHit hit, minimumDetectionNeededToFall, ignoreForGroundCheck))
            { // 离地面很近(着陆，正常移动)
                normalVector = hit.normal;
                Vector3 tp = hit.point;
                DebugTool.DebugPoint.transform.position = hit.point;
                // Log.Debug(hit.transform.gameObject.name);// 和自己(DebugPoint)碰撞导致人物一直向上升, 2333

                playerManager.isGrounded = true;
                targetPosition.y = tp.y;

                if (playerManager.isInAir)
                { // 着陆
                    if (inAirTimer > 0.5f)
                    {
                        Log.Debug("你滞空 " + inAirTimer + " 秒了");
                        animatorHandler.PlayTargetAnimation("Land", true);
                    }
                    else
                    {
                        animatorHandler.PlayTargetAnimation("Locomotion", false);
                    }

                    playerManager.isInAir = false;
                    inAirTimer = 0;
                }
            }
            else
            { // 在空中
                if (playerManager.isGrounded)
                {
                    playerManager.isGrounded = false;
                }

                if (playerManager.isInAir == false)
                {
                    if (playerManager.isInteracting == false)
                    {
                        animatorHandler.PlayTargetAnimation("Falling", true);
                    }

                    Vector3 vel = rigidbody.velocity;
                    vel.Normalize();
                    rigidbody.velocity = vel * (movementSpeed / 2);
                    // Log.Debug("Rig v: " + rigidbody.velocity);
                    playerManager.isInAir = true;
                }
            }

            if (playerManager.isGrounded)
            {
                if (playerManager.isInteracting || inputHandler.moveAmount > 0)
                {
                    myTransform.position = Vector3.Lerp(myTransform.position, targetPosition, delta * 5f);
                }
                else
                {
                    myTransform.position = targetPosition;
                }
            }
        }
        #endregion

    }
}
