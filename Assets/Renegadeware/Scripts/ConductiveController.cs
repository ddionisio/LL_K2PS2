using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware.K2PS2 {
    public class ConductiveController : MonoBehaviour {
        [Header("Data")]
        [SerializeField]
        bool _powered = false;
        [SerializeField]
        bool _defaultActive = false;

        public bool isHarmful = true;

        [Header("Display")]
        public GameObject[] poweredActiveGOs;

        public bool isPowered { get { return mActive && (_powered || mPowerConnectCounter > 0); } }

        public Collider2D coll { get; private set; }

        public bool active { 
            get { return mActive; }
            set {
                if(mActive != value) {
                    mActive = value;
                    if(mActive)
                        RefreshPowerActive();
                    else {
                        //update conductives in contact
                        for(int i = 0; i < mConductives.Count; i++) {
                            mConductives[i].UpdatePowerConnect();
                            mConductives[i].RefreshPowerActive();
                        }

                        ClearConductives();
                    }
                }
            }
        }

        private int mPowerConnectCounter;

        private const int conductiveCapacity = 8;

        private M8.CacheList<ConductiveController> mConductives = new M8.CacheList<ConductiveController>(conductiveCapacity);

        private Coroutine mRout;

        private bool mActive;

        void OnEnable() {
            mActive = _defaultActive;
            RefreshPowerActive();
        }

        void OnDisable() {
            ClearConductives();
        }

        void Awake() {
            coll = GetComponentInChildren<Collider2D>();
        }

        void OnTriggerStay2D(Collider2D collision) {
            if(!mActive)
                return;

            var gameDat = GameData.instance;

            //check if it's a player, kill if harmful and powered up
            if(collision.CompareTag(gameDat.playerTag)) {
                if(isHarmful && isPowered)
                    gameDat.signalPlayerDeath.Invoke();
                return;
            }

            if(_powered) //no refresh required
                return;

            if(!GameUtils.CheckTags(collision, gameDat.conductiveTags))
                return;

            //check if already added
            for(int i = 0; i < mConductives.Count; i++) {
                var _conductive = mConductives[i];
                if(_conductive.coll == collision)
                    return;
            }

            var conductive = collision.GetComponentInParent<ConductiveController>();
            if(!conductive)
                return;

            mConductives.Add(conductive);

            StartRefresh();
        }

        void OnTriggerExit2D(Collider2D collision) {
            if(!mActive)
                return;

            if(_powered) //no refresh required
                return;

            var gameDat = GameData.instance;

            if(!GameUtils.CheckTags(collision, gameDat.conductiveTags))
                return;

            for(int i = 0; i < mConductives.Count; i++) {
                if(mConductives[i].coll == collision) {
                    mConductives.RemoveAt(i);
                    break;
                }
            }
        }

        void OnCollisionStay2D(Collision2D collision) {
            if(!mActive)
                return;

            var gameDat = GameData.instance;

            var contactPts = collision.contacts;
            for(int i = 0; i < contactPts.Length; i++) {
                var contactPt = contactPts[i];
                var _coll = contactPt.collider;

                //check if it's a player, kill if harmful and powered up
                if(_coll.CompareTag(gameDat.playerTag)) {
                    if(isHarmful && isPowered)
                        gameDat.signalPlayerDeath.Invoke();

                    continue;
                }

                if(_powered) //no refresh required
                    return;

                if(!GameUtils.CheckTags(_coll, gameDat.conductiveTags))
                    continue;

                //check if already added
                bool isAdded = false;
                for(int j = 0; j < mConductives.Count; j++) {
                    if(mConductives[j].coll == _coll) {
                        isAdded = true;
                        break;
                    }
                }

                if(!isAdded) {
                    var conductive = _coll.GetComponentInParent<ConductiveController>();
                    if(conductive)
                        mConductives.Add(conductive);
                }
            }

            StartRefresh();
        }

        void OnCollisionExit2D(Collision2D collision) {
            if(!mActive)
                return;

            if(_powered) //no refresh required
                return;

            var gameDat = GameData.instance;

            var contactPts = collision.contacts;
            for(int i = 0; i < contactPts.Length; i++) {
                var contactPt = contactPts[i];
                var _coll = contactPt.collider;

                if(!GameUtils.CheckTags(_coll, gameDat.conductiveTags))
                    continue;

                for(int j = 0; j < mConductives.Count; j++) {
                    if(mConductives[j].coll == _coll) {
                        mConductives.RemoveAt(j);
                        break;
                    }
                }
            }
        }

        IEnumerator DoRefresh() {
            var wait = new WaitForSeconds(GameData.instance.conductiveRefreshDelay);

            while(mConductives.Count > 0) {
                var lastIsPowered = isPowered;

                //check if any is powered up
                UpdatePowerConnect();

                if(isPowered != lastIsPowered)
                    RefreshPowerActive();

                yield return wait;
            }

            mPowerConnectCounter = 0;
            RefreshPowerActive();

            mRout = null;
        }

        private void UpdatePowerConnect() {
            mPowerConnectCounter = 0;

            for(int i = mConductives.Count - 1; i >= 0; i--) {
                var conductive = mConductives[i];
                if(conductive) {
                    if(conductive.isPowered)
                        mPowerConnectCounter++;
                }
                else //fail-safe
                    mConductives.RemoveLast();
            }
        }

        private void StartRefresh() {
            if(mRout != null)
                return;

            if(mConductives.Count == 0)
                return;

            mRout = StartCoroutine(DoRefresh());
        }

        private void RefreshPowerActive() {
            var _isPowered = isPowered;

            for(int i = 0; i < poweredActiveGOs.Length; i++) {
                var go = poweredActiveGOs[i];
                if(go)
                    go.SetActive(_isPowered);
            }
        }

        private void ClearConductives() {
            if(mRout != null) {
                StopCoroutine(mRout);
                mRout = null;
            }

            mConductives.Clear();
            mPowerConnectCounter = 0;

            RefreshPowerActive();
        }
    }
}