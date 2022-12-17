using UnityEngine;

namespace DarkSoulsLike
{
    public class PlayerLocomotion : MonoBehaviour
    {
        Transform cameraTransform;
        InputHandler inputHandler;
        Vector3 moveDirection;

        [HideInInspector]
        public Transform myTransform;
        [HideInInspector]
        public AnimatorHandler animatorHandler;

        // 这里new关键字表示显式隐藏基类的同名成员。
        public new Rigidbody rigidbody;
        public GameObject normalCamera;

        [Header("Stats")]
        [SerializeField]
        float movementSpeed = 5f;
        [SerializeField]
        float rotationSpeed = 10f;

        private void Start()
        {
            rigidbody = GetComponent<Rigidbody>();
            inputHandler = GetComponent<InputHandler>();
            animatorHandler = GetComponentInChildren<AnimatorHandler>();
            cameraTransform = Camera.main.transform;
            myTransform = transform;
            animatorHandler.Initialize();
        }

        private void Update()
        {
            float delta = Time.deltaTime;

            inputHandler.TickInput(delta);

            // 根据相机朝向和输入移动人物
            moveDirection = cameraTransform.forward * inputHandler.vertical;
            moveDirection += cameraTransform.right * inputHandler.horizontal;
            moveDirection.Normalize();

            float speed = movementSpeed;
            moveDirection *= speed;

            Vector3 projectedVelocity = Vector3.ProjectOnPlane(moveDirection, normalVector);

            // Log.Debug("Projected Velocity: " + projectedVelocity);
            // Log.Debug("NormalVector: " + normalVector);

            rigidbody.velocity = projectedVelocity;

            animatorHandler.UpdateAnimatorValues(inputHandler.moveAmount, 0);

            if (animatorHandler.canRotate)
            {
                HandleRotation(delta);
            }
        }

        #region Movement
        Vector3 normalVector;
        Vector3 targetPosition;

        /// <summary>
        /// 人物转向
        /// </summary>
        /// <param name="delta"></param>
        private void HandleRotation(float delta)
        {
            Vector3 targetDir = Vector3.zero;
            float moveOverride = inputHandler.moveAmount;

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

            // hello

        }
        #endregion

    }
}
