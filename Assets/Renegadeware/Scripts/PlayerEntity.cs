using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware.K2PS2 {
    public class PlayerEntity : MonoBehaviour {
        public const float radiusCheckOfs = 0.2f;

        public enum State {
            None,
            Move,
            Standby
        }

        public enum MoveState {
            Stop,
            Left,
            Right
        }

        [Header("Data")]
        public MoveState moveStart = MoveState.Right;
        public float jumpImpulse = 4.75f;
        public float jumpCancelImpulse = 1.5f;
        public bool allowMoveOpposite = true;

        [Header("AI Data")]
        public LayerMask solidCheckMask;
        [M8.TagSelector]
        public string[] solidIgnoreTags;

        [M8.TagSelector]
        public string[] harmCheckTags;

        public float groundCheckForwardDist = 0.2f; //add radius
        public float groundCheckLastJumpDelay = 2f;
        public float groundCheckDownDist = 3.1f; //add radius

        private RaycastHit2D[] mHitCache = new RaycastHit2D[4];

        public State state {
            get { return mState; }
            set {
                if(mState != value) {
                    mState = value;

                    ApplyState();

                    stateChangedCallback?.Invoke();
                }
            }
        }

        public PlayerMove moveCtrl { get; private set; }

        public MoveState moveState {
            get { return mMoveState; }
            set {
                if(mMoveState != value) {
                    mMoveState = value;

                    ApplyMoveState();

                    moveChangedCallback?.Invoke();
                }
            }
        }

        public bool isJumping { get { return mJumpRout != null; } }

        /// <summary>
        /// time since we finished jumping
        /// </summary>
        public float lastJumpTime { get; private set; }

        public Vector2 startPosition { get; set; }

        //callbacks
        public event System.Action stateChangedCallback;
        public event System.Action moveChangedCallback;
        public event System.Action jumpStartCallback;
        public event System.Action jumpEndCallback;

        private State mState = State.None;

        private MoveState mMoveState = MoveState.Stop;

        private Collider2D mGroundLastWallChecked;

        private Coroutine mJumpRout;
        private Coroutine mStateRout;

        public void Jump() {
            if(mJumpRout == null)
                mJumpRout = StartCoroutine(DoJump());
        }

        public void JumpCancel() {
            if(mJumpRout != null) {
                lastJumpTime = Time.fixedTime;

                var lvel = moveCtrl.localVelocity;
                if(lvel.y > 0f)
                    moveCtrl.body.AddForce(Vector2.down * jumpCancelImpulse, ForceMode2D.Impulse);

                StopCoroutine(mJumpRout);
                mJumpRout = null;
            }
        }

        void Awake() {
            //setup move controller
            moveCtrl = GetComponent<PlayerMove>();

            moveCtrl.collisionEnterCallback += OnMoveCollisionEnter;
            moveCtrl.collisionExitCallback += OnMoveCollisionExit;
            moveCtrl.triggerEnterCallback += OnMoveTriggerEnter;
            moveCtrl.triggerExitCallback += OnMoveTriggerExit;

            //initial state
            mState = State.None;
            ApplyState();
        }

        void OnDestroy() {
            if(moveCtrl) {
                moveCtrl.collisionEnterCallback -= OnMoveCollisionEnter;
                moveCtrl.collisionExitCallback -= OnMoveCollisionExit;
                moveCtrl.triggerEnterCallback -= OnMoveTriggerEnter;
                moveCtrl.triggerExitCallback -= OnMoveTriggerExit;
            }
        }

        void FixedUpdate() {
            //AI during Normal
            if(mState == State.Move) {
                //not moving forward?
                if(moveCtrl.moveHorizontal == 0f) {
                    //landed, check if we need to resume moving according to move state
                    if(moveCtrl.isGrounded)
                        ApplyMoveState();
                }
                else if(moveCtrl.isSlopSlide) {
                    //TODO
                }
                //ground
                else if(moveCtrl.isGrounded)
                    GroundUpdate();
            }
        }

        void OnMoveCollisionEnter(PlayerMove ctrl, Collision2D coll) {
            //check if it's a side collision
            //if(!ctrl.isSlopSlide && (ctrl.collisionFlags & CollisionFlags.Sides) != CollisionFlags.None)
            //ctrl.moveHorizontal *= -1.0f;
        }

        void OnMoveCollisionExit(PlayerMove ctrl, Collision2D coll) {
        }

        void OnMoveTriggerEnter(PlayerMove ctrl, Collider2D coll) {
        }

        void OnMoveTriggerExit(PlayerMove ctrl, Collider2D coll) {
        }

        IEnumerator DoJump() {
            var wait = new WaitForFixedUpdate();

            Vector2 lastPos = transform.position;

            moveCtrl.body.AddForce(Vector2.up * jumpImpulse, ForceMode2D.Impulse);

            //moveCtrl.moveVertical = jumpMagnitude;

            jumpStartCallback?.Invoke();

            yield return new WaitForSeconds(0.1f);

            while(true) {
                yield return wait;

                //hit a ceiling?
                if((moveCtrl.collisionFlags & CollisionFlags.Above) != CollisionFlags.None)
                    break;

                //check if velocity is downward
                if(moveCtrl.isGrounded)
                    break;
            }

            lastJumpTime = Time.fixedTime;

            mJumpRout = null;

            jumpEndCallback?.Invoke();
        }

        private void ApplyState() {
            JumpCancel();

            if(mStateRout != null) {
                StopCoroutine(mStateRout);
                mStateRout = null;
            }

            var physicsActive = false;
            var toMoveState = MoveState.Stop;

            switch(mState) {
                case State.None:
                    break;

                case State.Move:
                    toMoveState = moveStart;
                    physicsActive = true;
                    break;

                case State.Standby:
                    physicsActive = true;
                    break;
            }

            moveState = toMoveState;

            if(physicsActive) {
                moveCtrl.coll.enabled = true;
                moveCtrl.body.simulated = true;
            }
            else {
                DisablePhysics();
            }
        }

        private void ApplyMoveState() {
            switch(mMoveState) {
                case MoveState.Stop:
                    moveCtrl.moveHorizontal = 0f;

                    mGroundLastWallChecked = null;
                    break;
                case MoveState.Left:
                    moveCtrl.moveHorizontal = -1f;
                    break;
                case MoveState.Right:
                    moveCtrl.moveHorizontal = 1f;
                    break;
            }
        }

        private void DisablePhysics() {
            moveCtrl.ResetCollision();

            moveCtrl.coll.enabled = false;

            moveCtrl.body.velocity = Vector2.zero;
            moveCtrl.body.angularVelocity = 0f;
            moveCtrl.body.simulated = false;

            mGroundLastWallChecked = null;

            lastJumpTime = 0f;
        }

        private bool CheckSolidCast(Vector2 dir, out RaycastHit2D hit, float dist) {
            var hitCount = moveCtrl.CheckAllCasts(Vector2.zero, radiusCheckOfs, dir, mHitCache, dist, solidCheckMask);
            for(int i = 0; i < hitCount; i++) {
                var _hit = mHitCache[i];

                //ignore player
                if(_hit.collider == moveCtrl.coll)
                    continue;

                //ignore certain tags
                if(GameUtils.CheckTags(_hit.collider, solidIgnoreTags))
                    continue;

                hit = _hit;
                return true;
            }

            hit = new RaycastHit2D();
            return false;
        }

        private RaycastHit2D Raycast(Vector2 pos, Vector2 dir, float dist) {
            var hitCount = Physics2D.RaycastNonAlloc(pos, dir, mHitCache, dist, solidCheckMask);
            for(int i = 0; i < hitCount; i++) {
                var _hit = mHitCache[i];

                //ignore player
                if(_hit.collider == moveCtrl.coll)
                    continue;

                //return harm tags
                if(GameUtils.CheckTags(_hit.collider, harmCheckTags))
                    return _hit;

                //ignore certain tags
                if(GameUtils.CheckTags(_hit.collider, solidIgnoreTags))
                    continue;

                return _hit;
            }

            return new RaycastHit2D();
        }

        private void GroundUpdate() {
            if(moveState == MoveState.Stop)
                return;

            if(isJumping)
                return;

            bool moveOpposite = false;

            //check forward to see if there's an obstacle
            Vector2 up = moveCtrl.dirHolder.up;
            Vector2 dir = new Vector2(Mathf.Sign(moveCtrl.moveHorizontal), 0f);
            float forwardDist = moveCtrl.radius + groundCheckForwardDist;

            RaycastHit2D hit;
            if(CheckSolidCast(dir, out hit, forwardDist)) {
                //check if it's harm's way
                if(GameUtils.CheckTags(hit.collider, harmCheckTags)) {
                    //move the opposite direction
                    moveOpposite = true;
                }
                else {
                    //check if it's a wall
                    var collFlag = moveCtrl.GetCollisionFlag(up, hit.normal);
                    if(collFlag == CollisionFlags.Sides) {
                        //only jump if it's a new collision
                        if(!allowMoveOpposite || mGroundLastWallChecked != hit.collider) {
                            mGroundLastWallChecked = hit.collider;

                            //jump!
                            Jump();
                        }
                        else
                            moveOpposite = true;
                    }
                    else if(collFlag == CollisionFlags.Above) //possibly a roof slanted towards the ground
                        moveOpposite = true;
                }

                if(allowMoveOpposite && moveOpposite) {
                    switch(moveState) {
                        case MoveState.Left:
                            moveState = MoveState.Right;
                            break;
                        case MoveState.Right:
                            moveState = MoveState.Left;
                            break;
                    }
                }
            }
            else {
                mGroundLastWallChecked = null;

                //check below
                Vector2 pos = transform.position;
                Vector2 down = -up;
                pos += dir * forwardDist;

                var hitDown = Raycast(pos, down, moveCtrl.radius + groundCheckDownDist);
                if(hitDown.collider) {
                    //check if it's harm's way
                    if(GameUtils.CheckTags(hitDown.collider, harmCheckTags)) {
                        //try jumping, that's a good trick
                        Jump();
                    }
                }
                else {
                    //nothing hit, jump!
                    Jump();
                }
            }
        }
    }
}