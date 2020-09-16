using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware.K2PS2 {
    public class MaterialObjectEntity : MonoBehaviour, M8.IPoolSpawn, M8.IPoolDespawn {
        public enum State {
            None,
            Ghost,
            Spawning,
            Normal,
            Despawning
        }

        [Header("Display")]
        public GameObject displayRootGO;

        public State state {
            get { return mState; }
            set {
                if(mState != value) {
                    mState = value;
                    ApplyCurrentState();
                }
            }
        }

        public M8.PoolDataController poolDataCtrl {
            get {
                if(!mPoolDataCtrl)
                    mPoolDataCtrl = GetComponent<M8.PoolDataController>();
                return mPoolDataCtrl; 
            }
        }

        public Rigidbody2D body { get; private set; }

        public Collider2D coll { get; private set; }

        private State mState;

        private M8.PoolDataController mPoolDataCtrl;

        private Coroutine mRout;

        public void Release() {
            if(poolDataCtrl)
                poolDataCtrl.Despawn();
            else //fail-safe
                gameObject.SetActive(false);
        }

        void Awake() {
            body = GetComponent<Rigidbody2D>();
            coll = GetComponent<Collider2D>();
        }

        void M8.IPoolSpawn.OnSpawned(M8.GenericParams parms) {
            //start state as ghost
            mState = State.Ghost;
            ApplyCurrentState();
        }

        void M8.IPoolDespawn.OnDespawned() {
            state = State.None;
        }

        IEnumerator DoSpawn() {
            //Do animation
            yield return null;

            mRout = null;

            state = State.Normal;
        }

        IEnumerator DoDespawn() {
            //Do animation
            yield return null;

            mRout = null;

            Release();
        }

        private void ApplyCurrentState() {
            if(mRout != null) {
                StopCoroutine(mRout);
                mRout = null;
            }

            var enableDisplay = true;
            var enablePhysics = false;

            switch(mState) {
                case State.None:
                    enableDisplay = false;
                    break;
                case State.Ghost:
                    break;
                case State.Spawning:
                    mRout = StartCoroutine(DoSpawn());
                    break;
                case State.Normal:
                    enablePhysics = true;
                    break;
                case State.Despawning:
                    mRout = StartCoroutine(DoDespawn());
                    break;
            }

            if(displayRootGO) displayRootGO.SetActive(enableDisplay);

            SetPhysics(enablePhysics);
        }

        private void SetPhysics(bool enable) {
            if(enable) {
                if(coll) coll.enabled = true;
                if(body) body.simulated = true;
            }
            else {
                if(coll) coll.enabled = false;

                if(body) {
                    body.velocity = Vector2.zero;
                    body.angularVelocity = 0f;
                    body.simulated = false;
                }
            }
        }
    }
}