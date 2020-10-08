using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware.K2PS2 {
    public class ConductiveController : MonoBehaviour {
        [Header("Data")]
        [SerializeField]
        bool _powered = false;

        public bool isHarmful = true;

        [Header("Display")]
        public GameObject[] poweredActiveGOs;

        public bool isPowered { get { return _powered || mPowerConnectCounter > 0; } }

        private int mPowerConnectCounter;

        private const int conductiveCapacity = 8;

        private M8.CacheList<ConductiveController> mConductives = new M8.CacheList<ConductiveController>(conductiveCapacity);

        private Coroutine mRout;

        void OnDisable() {
            ClearRout();
            ClearConductives();
        }

        void OnTriggerEnter2D(Collider2D coll) {
            if(coll.gameObject == gameObject)
                return;

            var gameDat = GameData.instance;

            //check if it's a player, kill if harmful and powered up
            if(coll.CompareTag(gameDat.playerTag)) {
                if(isHarmful && isPowered)
                    gameDat.signalPlayerDeath.Invoke();

                return;
            }

            if(_powered) //no refresh required
                return;

            if(!GameUtils.CheckTags(coll, gameDat.conductiveTags))
                return;

            var conductive = coll.GetComponent<ConductiveController>();
            if(conductive) {
                mConductives.Add(conductive);

                StartRefresh();
            }
        }

        void OnTriggerExit2D(Collider2D coll) {
            if(_powered) //no refresh required
                return;

            if(coll.gameObject == gameObject)
                return;

            if(!GameUtils.CheckTags(coll, GameData.instance.conductiveTags))
                return;

            //remove from list
            var go = coll.gameObject;

            for(int i = 0; i < mConductives.Count; i++) {
                var conductive = mConductives[i];
                if(conductive && conductive.gameObject == go) {
                    mConductives.RemoveAt(i);
                    break;
                }
            }
        }

        void OnCollisionEnter2D(Collision2D collision) {
            var gameDat = GameData.instance;

            var contactPts = collision.contacts;
            for(int i = 0; i < contactPts.Length; i++) {
                var contactPt = contactPts[i];
                var coll = contactPt.collider;

                if(coll.gameObject == gameObject)
                    continue;

                //check if it's a player, kill if harmful and powered up
                if(coll.CompareTag(gameDat.playerTag)) {
                    if(isHarmful && isPowered)
                        gameDat.signalPlayerDeath.Invoke();

                    continue;
                }

                if(!GameUtils.CheckTags(coll, gameDat.conductiveTags)) {
                    var conductive = coll.GetComponent<ConductiveController>();
                    if(!conductive)
                        continue;

                    if(!mConductives.Exists(conductive))
                        mConductives.Add(conductive);
                }
            }

            StartRefresh();
        }

        void OnCollisionExit2D(Collision2D collision) {
            if(_powered) //no refresh required
                return;

            var gameDat = GameData.instance;

            var contactPts = collision.contacts;
            for(int i = 0; i < contactPts.Length; i++) {
                var contactPt = contactPts[i];
                var coll = contactPt.collider;
                var go = coll.gameObject;

                if(go == gameObject)
                    continue;

                if(!GameUtils.CheckTags(coll, gameDat.conductiveTags)) {
                    //remove from list
                    for(int j = 0; j < mConductives.Count; j++) {
                        var conductive = mConductives[j];
                        if(conductive && conductive.gameObject == go) {
                            mConductives.RemoveAt(j);
                            break;
                        }
                    }
                }
            }
        }

        IEnumerator DoRefresh() {
            var wait = new WaitForSeconds(GameData.instance.conductiveRefreshDelay);

            while(mConductives.Count > 0) {
                var lastIsPowered = isPowered;

                //check if any is powered up
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

                if(isPowered != lastIsPowered)
                    RefreshDisplay();

                yield return wait;
            }

            mPowerConnectCounter = 0;
            RefreshDisplay();

            mRout = null;
        }

        private void StartRefresh() {
            if(mRout != null)
                return;

            if(mConductives.Count == 0)
                return;

            mRout = StartCoroutine(DoRefresh());
        }

        private void RefreshDisplay() {
            var _isPowered = isPowered;

            for(int i = 0; i < poweredActiveGOs.Length; i++) {
                var go = poweredActiveGOs[i];
                if(go)
                    go.SetActive(_isPowered);
            }
        }

        private void ClearConductives() {
            mConductives.Clear();
            mPowerConnectCounter = 0;

            RefreshDisplay();
        }

        private void ClearRout() {
            if(mRout != null) {
                StopCoroutine(mRout);
                mRout = null;
            }
        }
    }
}