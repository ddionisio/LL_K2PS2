using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware.K2PS2 {
    public class PlayerControl : MonoBehaviour {
        public PlayerEntity player { get; private set; }

        private Coroutine mRout;

        void OnEnable() {
            var gameDat = GameData.instance;
            gameDat.signalPlayerSpawn.callback += OnSpawn;
            gameDat.signalPlayerDeath.callback += OnDeath;
            gameDat.signalGamePlay.callback += OnPlay;
            gameDat.signalGameStop.callback += OnRespawn;
            gameDat.signalVictory.callback += OnVictory;
        }

        void OnDisable() {
            var gameDat = GameData.instance;
            gameDat.signalPlayerSpawn.callback -= OnSpawn;
            gameDat.signalPlayerDeath.callback -= OnDeath;
            gameDat.signalGamePlay.callback -= OnPlay;
            gameDat.signalGameStop.callback -= OnRespawn;
            gameDat.signalVictory.callback -= OnVictory;

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

        void OnDeath() {
            ClearRoutine();
            mRout = StartCoroutine(DoDeath());
        }

        void OnPlay() {
            ClearRoutine();
            player.state = PlayerEntity.State.Move;
        }

        void OnVictory() {
            ClearRoutine();
            mRout = StartCoroutine(DoVictory());
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
        }

        IEnumerator DoVictory() {
            yield return null;

            player.moveState = PlayerEntity.MoveState.Stop;

            //play victory

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