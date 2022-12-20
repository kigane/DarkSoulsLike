using UnityEngine;

namespace DarkSoulsLike
{
    public class AnimatorHandler : MonoBehaviour
    {
        public Animator anim;
        private PlayerManager playerManager;
        private PlayerLocomotion playerLocomotion;
        private int vertical;
        private int horizontal;
        public bool canRotate;

        public void Initialize()
        {
            anim = GetComponent<Animator>();
            playerManager = GetComponentInParent<PlayerManager>();
            playerLocomotion = GetComponentInParent<PlayerLocomotion>();
            // 动画参数
            vertical = Animator.StringToHash("Vertical");
            horizontal = Animator.StringToHash("Horizontal");

            // 在翻滚结束后将rollFlag,canRoll重新设定。
            AddAnimationEvent("Roll", "ResetRollFlags", 1f);
            AddAnimationEvent("Backflip", "ResetRollFlags", 0.8f);
            AddAnimationEvent("Backflip", "PlaySpeedUp", 0.4f);
            AddAnimationEvent("Backflip", "ResetSpeed", 0f);
        }

        public void UpdateAnimatorValues(float verticalMovement, float horizontalMovement, bool isSprinting)
        {
            // 轻推走，重推跑
            #region Vertical
            float v;
            if (verticalMovement > 0 && verticalMovement < 0.55f)
                v = 0.5f;
            else if (verticalMovement > 0.55f)
                v = 1;
            else if (verticalMovement < 0 && verticalMovement > -0.55f)
                v = -0.5f;
            else if (verticalMovement < 0.55f)
                v = -1;
            else
                v = 0;
            #endregion

            #region Horizontal
            float h;
            if (horizontalMovement > 0 && horizontalMovement < 0.55f)
                h = 0.5f;
            else if (horizontalMovement > 0.55f)
                h = 1;
            else if (horizontalMovement < 0 && horizontalMovement > -0.55f)
                h = -0.5f;
            else if (horizontalMovement < 0.55f)
                h = -1;
            else
                h = 0;
            #endregion

            if (isSprinting)
            {
                v = 2;
                h = horizontalMovement;
            }

            anim.SetFloat(vertical, v, 0.1f, Time.deltaTime);
            anim.SetFloat(horizontal, h, 0.1f, Time.deltaTime); // h根本没用到
        }

        public void PlayTargetAnimation(string targetAnim, bool isInteracting)
        {
            anim.applyRootMotion = isInteracting;
            anim.SetBool("isInteracting", isInteracting);
            anim.CrossFade(targetAnim, 0.2f); // 播放相应动画
        }

        public void CanRotate()
        {
            canRotate = true;
        }

        public void StopRotate()
        {
            canRotate = false;
        }

        // 处理动画root motion移动的回调函数
        // 在状态机和动画计算执行后，在OnAnimatorIK执行前执行。
        private void OnAnimatorMove()
        {
            if (playerManager.isInteracting == false)
                return;

            float delta = Time.deltaTime;
            // drag用于使物体减速。例如，打开降落伞。
            playerLocomotion.rigidbody.drag = 0;
            // 上一帧avatar的移动。使用applyRootMotion时才计算。
            Vector3 deltaPosition = anim.deltaPosition;
            deltaPosition.y = 0;
            Vector3 velocity = deltaPosition / delta;
            // 让GO跟上模型
            playerLocomotion.rigidbody.velocity = velocity;
        }

        /// <summary>
        /// 为指定动画片段名添加动画帧事件
        /// </summary>
        /// <param name="name">动画片段名</param>
        /// <param name="fun">事件函数(必须在animator所在对象上的脚本中写)</param>
        /// <param name="time">事件位置，百分比</param>
        public void AddAnimationEvent(string name, string fun, float time)
        {
            // 找到动画控制器中的所有动画片段
            AnimationClip[] temp = anim.runtimeAnimatorController.animationClips;
            // 遍历，找到指定的那个
            for (int i = 0; i < temp.Length; i++)
            {
                if (temp[i].name == name)
                {
                    Log.Debug(temp[i].length); // 动画长度 秒
                    Log.Debug(temp[i].frameRate); // 动画帧率
                    Log.Debug(temp[i].frameRate * temp[i].length); // 动画帧数
                    AnimationEvent animationEvent = new()
                    {
                        functionName = fun, // 函数名
                        time = temp[i].length * time // 事件在第几秒触发
                    };
                    temp[i].AddEvent(animationEvent); // 为指定的动画控制器添加事件
                    break;
                }
            }
            //重新绑定
            // ani.Rebind();
        }

        private void ResetRollFlags()
        {
            playerLocomotion.ResetRollFlags();
        }

        private void PlaySpeedUp()
        {
            anim.SetFloat("BackflipSpeed", 2f);
        }

        private void ResetSpeed()
        {
            anim.SetFloat("BackflipSpeed", 1.6f);
        }
    }
}
