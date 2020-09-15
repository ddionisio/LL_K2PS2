using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware.K2PS2 {
    public class PlayerControl : MonoBehaviour {
        public PlayerEntity player { get; private set; }

        [Header("Signals")]
        public M8.Signal signalListenSpawn;
        public M8.Signal signalListenDeath;
        public M8.Signal signalListenPlay;
        public M8.Signal signalListenVictory;

        private Coroutine mRout;

        void OnEnable() {
            if(signalListenSpawn) signalListenSpawn.callback += OnSpawn;
            if(signalListenDeath) signalListenDeath.callback += OnDeath;
            if(signalListenPlay) signalListenPlay.callback += OnPlay;
            if(signalListenVictory) signalListenVictory.callback += OnVictory;
        }

        void OnDisable() {
            if(signalListenSpawn) signalListenSpawn.callback -= OnSpawn;
            if(signalListenDeath) signalListenDeath.callback -= OnDeath;
            if(signalListenPlay) signalListenPlay.callback -= OnPlay;
            if(signalListenVictory) signalListenVictory.callback -= OnVictory;

            ClearRoutine();
        }

        void Awake() {
            player = GetComponent<PlayerEntity>();
        }

        void OnSpawn() {
            ClearRoutine();
            mRout = StartCoroutine(DoSpawn());
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

        IEnumerator DoDeath() {
            yield return null;

            player.state = PlayerEntity.State.None;

            //play despawn
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