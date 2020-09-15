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

        private State mState;

        private M8.PoolDataController mPoolDataCtrl;

        void M8.IPoolSpawn.OnSpawned(M8.GenericParams parms) {
            //start state as ghost
            mState = State.Ghost;
            ApplyCurrentState();
        }

        void M8.IPoolDespawn.OnDespawned() {
            state = State.None;
        }

        private void ApplyCurrentState() {

        }
    }
}