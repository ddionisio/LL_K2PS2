using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware.K2PS2 {
    public class PlayerControl : MonoBehaviour {
        public PlayerEntity player { get; private set; }

        private Coroutine mRout;

        public void Kill() {
            ClearRoutine();
            mRout = StartCoroutine(DoDeath());
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
            player.state = PlayerEntity.State.Move;
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

        IEnumerator DoSpawn() {
            //initialize spawn display

            yield return null;

            player.state = PlayerEntity.State.None;
            transform.position = player.startPosition;

            //play spawn
            yield return null;

            //stand-by
            player.state = PlayerEntity.State.Standby;

            mRout = null;
        }

        IEnumerator DoRespawn() {
            player.state = PlayerEntity.State.None;

            //play despawn
            yield return null;

            //respawn
            mRout = StartCoroutine(DoSpawn());
        }

        IEnumerator DoDeath() {
            yield return null;

            player.state = PlayerEntity.State.None;

            //play death

            mRout = null;
        }

        IEnumerator DoGoal() {
            yield return null;

            player.state = PlayerEntity.State.Standby;

            //play victory

            mRout = null;
        }

        IEnumerator DoMoveTo(Vector2 pos) {
            player.state = PlayerEntity.State.Standby;

            var body = player.moveCtrl.body;
            var toX = pos.x;

            if(body.position.x < toX) {
                player.moveState = PlayerEntity.MoveState.Right;

                while(body.position.x < toX)
                    yield return null;
            }
            else if(body.position.x > toX) {
                player.moveState = PlayerEntity.MoveState.Left;

                while(body.position.x > toX)
                    yield return null;
            }

            player.moveState = PlayerEntity.MoveState.Stop;

            mRout = null;
        }

        private void ClearRoutine() {
            if(mRout != null) {
                StopCoroutine(mRout);
                mRout = null;
            }
        }
    }
}