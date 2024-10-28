using Lockstep.Collision2D;
using Lockstep.Math;

namespace Lockstep.Logic {
    public partial class CMover : PlayerComponent {
        static LFloat _sqrStopDist = new LFloat(true, 40);
        public LFloat speed => entity.speed;
        public bool hasReachTarget = false;

        public bool needMove = true;

        public override void DoUpdate(LFloat deltaTime){
            //if (InputManager.hasHitFloor) {
            //    var dir = (InputManager.mousePos - GameManager.player.transform2D.pos);
            //    transform2D.deg = CTransform2D.ToDeg(dir);
            //}
            if (!entity.rigidbody.isOnFloor) {
                return;
            }
#if true
            var needAc = input.inputUV.sqrMagnitude > new LFloat(true, 10);
            if (needAc) {
                var dir = input.inputUV.normalized;
                transform.pos = transform.pos + dir * speed * deltaTime;
                var targetDeg = dir.ToDeg();
                //todo:一直摁着一个方向，有时都会出现y角度 晃动明显的情况，这里的实现有问题，先不要插值了
                // transform.deg = CTransform2D.TurnToward(targetDeg, transform.deg, 360 * deltaTime, out var hasReachDeg);
                transform.deg = targetDeg;
                // UnityEngine.Debug.Log("deg "+ transform.deg);
            }

            hasReachTarget = !needAc;
#else
            if (InputManager.hasHitFloor) {
                needMove = true;
            }

            if (!needMove) {
                return;
            }
            var targetPos = InputManager.mousePos;
            var movement = targetPos - transform.pos;
            var hasReachPos = movement.sqrMagnitude < _sqrStopDist;
            if (!hasReachPos) {
                movement = movement.normalized * speed * deltaTime;
                transform.pos = transform.pos + movement;
            }

            var deg = CTransform2D.TurnToward(targetPos, transform.pos,
                transform.deg, 150 * deltaTime,
                out var hasReachDeg);

            if (!hasReachDeg) {
                transform.deg = deg;
            }

            hasReachTarget = hasReachPos & hasReachDeg;
            if (hasReachTarget) {
                needMove = false;
            }
#endif
        }
    }
}