using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware.K2PS2 {
    [ExecuteInEditMode]
    public class WaterController : MonoBehaviour {
        public const int triggerCacheCapacity = 8;

        public struct SurfaceCache : IComparable, IComparable<SurfaceCache> {
            public Collider2D coll;

            public Vector2 lastPos;
            public float lastRot;

            public SurfaceCache(Collider2D aColl) {
                coll = aColl;

                //allow for change in the next water update
                var trans = coll.transform;

                lastPos = trans.position;
                lastRot = trans.eulerAngles.z;
            }

            public bool CheckAndUpdateTelemetry() {
                var trans = coll.transform;

                Vector2 curPos = trans.position;
                float curRot = trans.eulerAngles.z;

                if(curPos != lastPos || curRot != lastRot) {
                    lastPos = curPos;
                    lastRot = curRot;

                    return true;
                }

                return false;
            }

            public void AddSurfaceLevel(WaterController water) {
                var collBounds = coll.bounds;

                float waterY = water.transform.position.y;
                float surfaceLevel = water.mBuoyancy.surfaceLevel;

                float minY = collBounds.min.y;

                minY -= waterY;

                if(minY < surfaceLevel) {
                    float widthRatio = collBounds.size.x / water.mColl.size.x;
                    float yDiff = surfaceLevel - minY;

                    surfaceLevel += widthRatio * yDiff;

                    water.mBuoyancy.surfaceLevel = surfaceLevel;
                }
            }

            int IComparable<SurfaceCache>.CompareTo(SurfaceCache other) {
                if(lastPos.y < other.lastPos.y)
                    return -1;
                else if(lastPos.y > other.lastPos.y)
                    return 1;

                return 0;
            }

            int IComparable.CompareTo(object obj) {
                if(obj == null)
                    return -1;
                if(!(obj is SurfaceCache))
                    return -1;

                SurfaceCache other = (SurfaceCache)obj;

                if(lastPos.y < other.lastPos.y)
                    return -1;
                else if(lastPos.y > other.lastPos.y)
                    return 1;

                return 0;
            }
        }

        public struct KillableCache {
            public Collider2D coll;
            public PlayerControl ent;
            public float yOfs;

            public KillableCache(Collider2D aColl) {
                coll = aColl;
                yOfs = coll.bounds.size.y * 0.3f;
                ent = coll.GetComponent<PlayerControl>();
            }

            public bool IsKillable(WaterController water) {
                //just y position
                //just assume water is not rotated
                float collY = coll.transform.position.y - yOfs;
                float waterY = (water.transform.position.y + water.mColl.offset.y) - water.mColl.size.y * 0.5f + water.mBuoyancy.surfaceLevel;

                return collY <= waterY;
            }

            public void Kill() {
                ent.Kill();
            }
        }

        public Transform fillRoot;
        public Transform topRoot;

        private BuoyancyEffector2D mBuoyancy;
        private BoxCollider2D mColl;

        private float mSurfaceBaseLevel;

        private M8.CacheList<SurfaceCache> mSurfaceCache;

        private PlayerControl mPlayerCtrl;
        private float mPlayerYOfs;

        void OnTriggerEnter2D(Collider2D collision) {
            if(IsKillable(collision)) {
                mPlayerCtrl = collision.GetComponent<PlayerControl>();
                if(mPlayerCtrl) {
                    mPlayerYOfs = collision.bounds.size.y * 0.3f;
                }
            }
            else {
                if(mSurfaceCache.IsFull) mSurfaceCache.Expand();

                var newSurfaceCache = new SurfaceCache(collision);
                mSurfaceCache.Add(newSurfaceCache);

                ComputeSurface(true);
            }
        }

        void OnTriggerExit2D(Collider2D collision) {
            //check surfaces
            for(int i = 0; i < mSurfaceCache.Count; i++) {
                var dat = mSurfaceCache[i];
                if(dat.coll == collision) {
                    mSurfaceCache.RemoveAt(i);

                    //recompute surface
                    ComputeSurface(false);
                    return;
                }
            }

            //check killables
            if(mPlayerCtrl && mPlayerCtrl.player.moveCtrl.coll == collision) {
                mPlayerCtrl = null;
            }
        }

#if UNITY_EDITOR
        void Update() {
            if(!Application.isPlaying) {
                if(!mBuoyancy) mBuoyancy = GetComponent<BuoyancyEffector2D>();
                if(!mColl) mColl = GetComponent<BoxCollider2D>();

                if(!mColl || !mBuoyancy)
                    return;

                var collBounds = mColl.bounds;

                if(topRoot) {
                    Vector2 topPos = new Vector2(
                        collBounds.center.x,
                        transform.position.y + mBuoyancy.surfaceLevel);

                    topRoot.position = topPos;

                    Vector3 topScale = topRoot.localScale;
                    topScale.x = collBounds.size.x;

                    topRoot.localScale = topScale;
                }

                if(fillRoot) {
                    Vector2 fillPos = new Vector2(
                        collBounds.center.x,
                        transform.position.y);

                    fillRoot.position = fillPos;

                    Vector3 fillScale = fillRoot.localScale;
                    fillScale.x = collBounds.size.x;
                    fillScale.y = mBuoyancy.surfaceLevel;

                    fillRoot.localScale = fillScale;
                }

                return;
            }
        }
#endif

        void FixedUpdate() {
            //check and update surfaces
            bool isNeedUpdate = false;
            bool isComputeSurfaceSort = false;

            for(int i = mSurfaceCache.Count - 1; i >= 0; i--) {
                var surface = mSurfaceCache[i];

                //for some reason it's disabled? assume it got released
                if(surface.coll == null || !surface.coll.gameObject.activeSelf) {
                    mSurfaceCache.RemoveLast();
                    isNeedUpdate = true;
                }
                else if(surface.CheckAndUpdateTelemetry()) {
                    isNeedUpdate = true;
                    isComputeSurfaceSort = true;
                }
            }

            if(isNeedUpdate)
                ComputeSurface(isComputeSurfaceSort);

            //check if killables have reached the surface height
            if(CanKillPlayer()) {
                mPlayerCtrl.Kill();
                mPlayerCtrl = null;
            }
        }

        void Awake() {
#if UNITY_EDITOR
            if(!Application.isPlaying)
                return;
#endif

            mBuoyancy = GetComponent<BuoyancyEffector2D>();
            mColl = GetComponent<BoxCollider2D>();

            mSurfaceBaseLevel = mBuoyancy.surfaceLevel;

            mSurfaceCache = new M8.CacheList<SurfaceCache>(triggerCacheCapacity);
        }

        private bool IsKillable(Collider2D coll) {
            var tagKillFilter = GameData.instance.playerTag;

            if(string.IsNullOrEmpty(tagKillFilter))
                return false;

            return coll.gameObject.CompareTag(tagKillFilter);
        }

        private bool CanKillPlayer() {
            if(!mPlayerCtrl)
                return false;

            //just y position
            //just assume water is not rotated
            float collY = mPlayerCtrl.transform.position.y - mPlayerYOfs;
            float waterY = (transform.position.y + mColl.offset.y) - mColl.size.y * 0.5f + mBuoyancy.surfaceLevel;

            return collY <= waterY;
        }

        private void ComputeSurface(bool sort) {
            if(sort)
                mSurfaceCache.Sort();

            //recompute water surface level
            mBuoyancy.surfaceLevel = mSurfaceBaseLevel;

            for(int i = 0; i < mSurfaceCache.Count; i++)
                mSurfaceCache[i].AddSurfaceLevel(this);

            if(mBuoyancy.surfaceLevel > mColl.size.y)
                mBuoyancy.surfaceLevel = mColl.size.y;

            //apply top and fill telemetry
            if(topRoot) {
                Vector2 topPos = topRoot.localPosition;
                topPos.y = mBuoyancy.surfaceLevel;
                topRoot.transform.localPosition = topPos;
            }

            if(fillRoot) {
                Vector3 fillScale = fillRoot.localScale;
                fillScale.y = mBuoyancy.surfaceLevel;
                fillRoot.localScale = fillScale;
            }
        }
    }
}