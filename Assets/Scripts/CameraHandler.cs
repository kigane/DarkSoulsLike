using UnityEngine;

namespace DarkSoulsLike
{
    public class CameraHandler : MonoBehaviour
    {
        public Transform targetTransform;
        public Transform cameraTransform; // 相机，通过localPosition控制相机远近--z轴
        public Transform cameraPivotTransform; // 控制相机高度--y轴
        private Transform myTransform;
        private Vector3 cameraTransformPosition;
        private LayerMask ignoreLayers;
        private Vector3 cameraFollowVelocity = Vector3.zero;

        public static CameraHandler singleton;

        public float lookSpeed = 0.1f;
        public float followSpeed = 0.1f;
        public float pivotSpeed = 0.03f;

        private float targetPosition;
        private float defaultPosition;
        private float lookAngle;
        private float pivotAngle;
        private float minimumPivot = -35;
        private float maximumPivot = 35;

        public float cameraSphereRadius = 0.2f;
        public float cameraCollisionOffset = 0.2f;
        public float minimumCollisionOffset = 0.2f;

        private void Awake()
        {
            singleton = this;
            myTransform = transform;
            defaultPosition = cameraTransform.localPosition.z;
            ignoreLayers = ~(1 << 8 | 1 << 9 | 1 << 10);
        }

        public void FollowTarget(float delta)
        {
            Vector3 targetPosition = Vector3.SmoothDamp(myTransform.position, targetTransform.position, ref cameraFollowVelocity, delta / followSpeed);
            myTransform.position = targetPosition;

            HandleCameraCollision(delta);
        }

        public void HandleCameraRotation(float delta, float mouseX, float mouseY)
        {
            // unity使用左手系, 相机看向z轴正方向
            // 欧拉角的旋转角x的值表示绕x轴旋转的度数。
            // 所以rotation.x表示上下转动，对应输入的上下信号
            // 所以rotation.y表示左右转动，对应输入的左右信号
            lookAngle += mouseX * lookSpeed / delta;
            pivotAngle -= mouseY * pivotSpeed / delta;
            pivotAngle = Mathf.Clamp(pivotAngle, minimumPivot, maximumPivot);

            Vector3 rotation = Vector3.zero;
            rotation.y = lookAngle;
            Quaternion targetRotation = Quaternion.Euler(rotation);
            myTransform.rotation = targetRotation; // 先转CameraHolder

            rotation = Vector3.zero;
            rotation.x = pivotAngle;

            targetRotation = Quaternion.Euler(rotation);
            cameraPivotTransform.localRotation = targetRotation; // 再转CameraPivot
        }

        private void HandleCameraCollision(float delta)
        {
            targetPosition = defaultPosition; // 如果没有碰撞则重置

            // 方向为从CameraPivot指向摄像机(平的过去)
            // CameraPivot挂在CameraHandler上，和Player位于相同位置。通过调整CameraPivot的y轴位置，可以调节检测碰撞的高度。
            Vector3 direction = cameraTransform.position - cameraPivotTransform.position;
            direction.Normalize();

            if (Physics.SphereCast // 圆柱状的射线。可以确定有一定大小的物体能移动而不发生碰撞的的距离。
                (cameraPivotTransform.position, cameraSphereRadius, direction,
                 out RaycastHit hit, Mathf.Abs(targetPosition), ignoreLayers))
            {
                float dis = Vector3.Distance(cameraPivotTransform.position, hit.point);
                targetPosition = -(dis - cameraCollisionOffset);
            }

            if (Mathf.Abs(targetPosition) < minimumCollisionOffset)
            {
                targetPosition = -minimumCollisionOffset;
            }

            cameraTransformPosition.z = Mathf.Lerp(cameraTransform.localPosition.z, targetPosition, delta / 0.2f);
            cameraTransform.localPosition = cameraTransformPosition;
        }
    }
}
