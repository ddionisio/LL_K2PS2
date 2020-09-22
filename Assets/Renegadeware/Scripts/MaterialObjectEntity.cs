using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Renegadeware.K2PS2 {
    public class MaterialObjectEntity : MonoBehaviour, M8.IPoolSpawn, M8.IPoolDespawn, IBeginDragHandler, IDragHandler, IEndDragHandler {
        public const string parmData = "d";
        public const string parmDragWidget = "dw";

        public enum State {
            None,
            Ghost,
            Spawning,
            Normal,
            Despawning
        }

        public enum PhysicsMode {
            None,
            Ghost,
            Normal
        }

        [Header("Display")]
        public GameObject displayRootGO;

        [Header("Signals")]
        public M8.Signal signalInvokeDragBegin;
        public M8.Signal signalInvokeDragEnd;
        public M8.Signal signalListenGamePlay;
        public M8.Signal signalListenGameStop;

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

        public Vector2 position {
            get { return body.simulated ? body.position : (Vector2)transform.position; }
            set {
                if(body.simulated)
                    body.position = value;
                else
                    transform.position = value;
            }
        }

        public bool isPlaceable {
            get {
                if(!coll)
                    return false;

                var placementTag = GameData.instance.placementTag;
                var ignoreTags = GameData.instance.placementIgnoreTags;

                bool isPlacementOverlap = false;

                int overlapCount = coll.OverlapCollider(mOverlapFilter, mOverlapColls);
                for(int i = 0; i < overlapCount; i++) {
                    var overlapColl = mOverlapColls[i];

                    if(overlapColl == coll)
                        continue;
                    else if(overlapColl.CompareTag(placementTag))
                        isPlacementOverlap = true;
                    else if(GameUtils.CheckTags(overlapColl, ignoreTags))
                        continue;
                    else
                        return false;
                }

                return isPlacementOverlap;
            }
        }

        public MaterialObjectData data { get; private set; }

        public Rigidbody2D body { get; private set; }

        public Collider2D coll { get; private set; }

        private State mState;

        private M8.PoolDataController mPoolDataCtrl;

        private MaterialObjectDragWidget mDragWidget;

        private Coroutine mRout;

        private ContactFilter2D mOverlapFilter = new ContactFilter2D();
        private Collider2D[] mOverlapColls = new Collider2D[8];

        private bool mIsDragging;
        private bool mIsDraggable;
        private Vector2 mLastPos;

        /// <summary>
        /// Only call this in ghost mode
        /// </summary>
        public void UpdateGhostPosition(Vector2 pos) {
            position = pos;

            //change ghost display if placeable or not
            if(isPlaceable) {

            }
        }

        public void Release() {
            if(poolDataCtrl)
                poolDataCtrl.Despawn();
            else //fail-safe
                gameObject.SetActive(false);
        }

        void OnApplicationFocus(bool focus) {
            if(!focus)
                EndDrag();
        }

        void OnEnable() {
            if(signalListenGamePlay) signalListenGamePlay.callback += OnGamePlay;
            if(signalListenGameStop) signalListenGameStop.callback += OnGameStop;
        }

        void OnDisable() {
            if(signalListenGamePlay) signalListenGamePlay.callback -= OnGamePlay;
            if(signalListenGameStop) signalListenGameStop.callback -= OnGameStop;

            EndDrag();
        }

        void Awake() {
            body = GetComponent<Rigidbody2D>();
            coll = GetComponent<Collider2D>();
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {
            if(!mIsDraggable)
                return;

            mDragWidget.Setup(data);
            mDragWidget.transform.position = eventData.position;

            state = State.Ghost;
            mIsDragging = true;

            mLastPos = position;

            signalInvokeDragBegin?.Invoke();
        }

        void IDragHandler.OnDrag(PointerEventData eventData) {
            if(!mIsDragging)
                return;

            var pos = eventData.position;

            //update drag widget position
            mDragWidget.transform.position = pos;

            var cam = Camera.main;
            var entPos = cam.ScreenToWorldPoint(pos);
            UpdateGhostPosition(entPos);
        }
        
        void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
            EndDrag();
        }

        void M8.IPoolSpawn.OnSpawned(M8.GenericParams parms) {
            //setup data
            data = null;
            mDragWidget = null;

            if(parms != null) {
                if(parms.ContainsKey(parmData))
                    data = parms.GetValue<MaterialObjectData>(parmData);

                if(parms.ContainsKey(parmDragWidget))
                    mDragWidget = parms.GetValue<MaterialObjectDragWidget>(parmDragWidget);
            }

            mOverlapFilter.SetLayerMask(GameData.instance.placementLayerMask);
            mOverlapFilter.useTriggers = true;

            //start state as ghost
            mState = State.Ghost;
            ApplyCurrentState();

            mIsDraggable = true; //assume we are spawned in edit mode
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

        void OnGamePlay() {
            EndDrag();
            mIsDraggable = false;
        }

        void OnGameStop() {
            if(mState == State.Spawning || mState == State.Normal)
                mIsDraggable = true;
        }

        private void ApplyCurrentState() {
            if(mRout != null) {
                StopCoroutine(mRout);
                mRout = null;
            }

            var enableDisplay = true;
            var toPhysicsMode = PhysicsMode.None;

            switch(mState) {
                case State.None:
                    enableDisplay = false;
                    mIsDraggable = false;
                    break;
                case State.Ghost:
                    toPhysicsMode = PhysicsMode.Ghost;
                    break;
                case State.Spawning:
                    mRout = StartCoroutine(DoSpawn());
                    break;
                case State.Normal:
                    toPhysicsMode = PhysicsMode.Normal;
                    break;
                case State.Despawning:
                    mRout = StartCoroutine(DoDespawn());
                    mIsDraggable = false;
                    break;
            }

            if(displayRootGO) displayRootGO.SetActive(enableDisplay);

            SetPhysicsMode(toPhysicsMode);
        }

        private void SetPhysicsMode(PhysicsMode mode) {
            switch(mode) {
                case PhysicsMode.None:
                    if(coll) coll.enabled = false;

                    if(body) {
                        body.velocity = Vector2.zero;
                        body.angularVelocity = 0f;
                        body.simulated = false;
                    }
                    break;

                case PhysicsMode.Ghost:
                    if(coll) {
                        coll.isTrigger = true;
                        coll.enabled = true;
                    }

                    if(body) {
                        body.isKinematic = true;
                        body.simulated = true;
                    }
                    break;

                case PhysicsMode.Normal:
                    if(coll) {
                        coll.isTrigger = false;
                        coll.enabled = true;
                    }

                    if(body) {
                        body.isKinematic = false;
                        body.simulated = true;
                    }
                    break;
            }
        }

        private void EndDrag() {
            if(!mIsDragging)
                return;

            if(state == State.Ghost) {
                if(!isPlaceable)
                    position = mLastPos;

                state = State.Normal;
            }

            mIsDragging = false;

            signalInvokeDragEnd?.Invoke();
        }
    }
}