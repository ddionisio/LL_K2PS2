using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware.K2PS2 {
    public class PlayerControl : MonoBehaviour {
        [Header("Display")]
        public SpriteRenderer spriteRenderer;

        [Header("Animation")]
        public M8.Animator.Animate animator;
        [M8.Animator.TakeSelector(animatorField = "animator")]
        public string takeSpawn;
        [M8.Animator.TakeSelector(animatorField = "animator")]
        public string takeIdle;
        [M8.Animator.TakeSelector(animatorField = "animator")]
        public string takeMove;
        [M8.Animator.TakeSelector(animatorField = "animator")]
        public string takeAirUp;
        [M8.Animator.TakeSelector(animatorField = "animator")]
        public string takeAirDown;
        [M8.Animator.TakeSelector(animatorField = "animator")]
        public string takeDespawn;
        [M8.Animator.TakeSelector(animatorField = "animator")]
        public string takeDeath;
        [M8.Animator.TakeSelector(animatorField = "animator")]
        public string takeVictory;

        public PlayerEntity player { get; private set; }

        private Coroutine mRout;

        private int mTakeSpawnInd;
        private int mTakeIdleInd;
        private int mTakeMoveInd;
        private int mTakeAirUpInd;
        private int mTakeAirDownInd;
        private int mTakeDespawnInd;
        private int mTakeDeathInd;
        private int mTakeVictoryInd;

        private PlayerEntity.MoveState mCurMoveState;
        private bool mCurGrounded;
        private float mCurVelY;

        public void Kill() {
            ClearRoutine();

            player.state = PlayerEntity.State.None;

            //play death animation
            animator.Play(mTakeDeathInd);
        }

        void OnEnable() {
            var gameDat = GameData.instance;
            gameDat.signalPlayerSpawn.callback += OnSpawn;
            gameDat.signalPlayerDeath.callback += Kill;
            gameDat.signalGamePlay.callback += OnPlay;
            gameDat.signalGameStop.callback += OnRespawn;
            gameDat.signalGoal.callback += OnGoal;
            gameDat.signalPlayerMoveTo.callback += OnMoveTo;
        }

        void OnDisable() {
            var gameDat = GameData.instance;
            gameDat.signalPlayerSpawn.callback -= OnSpawn;
            gameDat.signalPlayerDeath.callback -= Kill;
            gameDat.signalGamePlay.callback -= OnPlay;
            gameDat.signalGameStop.callback -= OnRespawn;
            gameDat.signalGoal.callback -= OnGoal;
            gameDat.signalPlayerMoveTo.callback -= OnMoveTo;

            ClearRoutine();
        }

        void Awake() {
            player = GetComponent<PlayerEntity>();

            mTakeSpawnInd = animator.GetTakeIndex(takeSpawn);
            mTakeIdleInd = animator.GetTakeIndex(takeIdle);
            mTakeMoveInd = animator.GetTakeIndex(takeMove);
            mTakeAirUpInd = animator.GetTakeIndex(takeAirUp);
            mTakeAirDownInd = animator.GetTakeIndex(takeAirDown);
            mTakeDespawnInd = animator.GetTakeIndex(takeDespawn);
            mTakeDeathInd = animator.GetTakeIndex(takeDeath);
            mTakeVictoryInd = animator.GetTakeIndex(takeVictory);

            //default hidden
            spriteRenderer.gameObject.SetActive(false);
        }

        void OnSpawn() {
            ClearRoutine();
            mRout = StartCoroutine(DoSpawn());
        }

        void OnRespawn() {
            ClearRoutine();
            mRout = StartCoroutine(DoRespawn());
        }

        void OnPlay() {
            ClearRoutine();

            //despawning, spawning, death? Force position to start
            if(animator.currentPlayingTakeIndex == mTakeDespawnInd || animator.currentPlayingTakeIndex == mTakeSpawnInd || animator.currentPlayingTakeIndex == mTakeDeathInd)
                transform.position = player.startPosition;

            player.state = PlayerEntity.State.Move;

            mRout = StartCoroutine(DoNormal());
        }

        void OnGoal() {
            ClearRoutine();
            mRout = StartCoroutine(DoGoal());
        }

        void OnMoveTo(Vector2 pos) {
            //set new start position
            player.startPosition = pos;

            ClearRoutine();
            mRout = StartCoroutine(DoMoveTo(pos));
        }

        IEnumerator DoNormal() {
            MoveStartAnimation();

            while(player.state == PlayerEntity.State.Move || player.state == PlayerEntity.State.Standby) {
                MoveUpdateAnimation();
                yield return null;
            }

            mRout = null;
        }

        IEnumerator DoSpawn() {
            player.state = PlayerEntity.State.None;
            transform.position = player.startPosition;

            //play spawn
            yield return animator.PlayWait(mTakeSpawnInd);

            //stand-by
            player.state = PlayerEntity.State.Standby;

            //prevent awkward animation
            animator.Play(mTakeIdleInd);
            while(!player.moveCtrl.isGrounded)
                yield return null;

            mRout = StartCoroutine(DoNormal());
        }

        IEnumerator DoRespawn() {
            player.state = PlayerEntity.State.None;

            //play despawn
            yield return animator.PlayWait(mTakeDespawnInd);

            //respawn
            mRout = StartCoroutine(DoSpawn());
        }

        IEnumerator DoGoal() {
            //wait for player to be grounded
            player.state = PlayerEntity.State.Standby;
            while(!player.moveCtrl.isGrounded) {
                MoveUpdateAnimation();
                yield return null;
            }

            //play victory
            animator.Play(mTakeVictoryInd);

            mRout = null;
        }

        IEnumerator DoMoveTo(Vector2 pos) {
            player.state = PlayerEntity.State.Standby;

            var body = player.moveCtrl.body;
            var toX = pos.x;

            if(body.position.x < toX) {
                player.moveState = PlayerEntity.MoveState.Right;

                MoveStartAnimation();

                while(body.position.x < toX) {
                    MoveUpdateAnimation();
                    yield return null;
                }
            }
            else if(body.position.x > toX) {
                player.moveState = PlayerEntity.MoveState.Left;

                MoveStartAnimation();

                while(body.position.x > toX) {
                    MoveUpdateAnimation();
                    yield return null;
                }
            }

            player.moveState = PlayerEntity.MoveState.Stop;

            mRout = StartCoroutine(DoNormal());
        }

        private void MoveStartAnimation() {
            mCurMoveState = player.moveState;
            mCurGrounded = player.moveCtrl.isGrounded;
            mCurVelY = player.moveCtrl.body.velocity.y;

            MoveApplyAnimation();
        }

        private void MoveUpdateAnimation() {
            var curMoveState = player.moveState;
            var curGrounded = player.moveCtrl.isGrounded;
            var curVelY = player.moveCtrl.body.velocity.y;

            if(mCurMoveState != curMoveState || mCurGrounded != curGrounded || mCurVelY != curVelY) {
                mCurMoveState = curMoveState;
                mCurGrounded = curGrounded;
                mCurVelY = curVelY;

                MoveApplyAnimation();
            }
        }

        private void MoveApplyAnimation() {
            var moveCtrl = player.moveCtrl;

            if(player.moveState == PlayerEntity.MoveState.Left)
                spriteRenderer.flipX = true;
            else if(player.moveState == PlayerEntity.MoveState.Right)
                spriteRenderer.flipX = false;

            if(moveCtrl.isGrounded) {
                if(mCurMoveState == PlayerEntity.MoveState.Stop)
                    animator.Play(mTakeIdleInd);
                else
                    animator.Play(mTakeMoveInd);
            }
            else {
                if(mCurVelY > 0f)
                    animator.Play(mTakeAirUpInd);
                else
                    animator.Play(mTakeAirDownInd);
            }
        }

        private void ClearRoutine() {
            if(mRout != null) {
                StopCoroutine(mRout);
                mRout = null;
            }
        }
    }
}