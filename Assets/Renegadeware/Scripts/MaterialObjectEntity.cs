using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;
using UnityEngine.UI.Extensions;

namespace Renegadeware.K2PS2 {
    public class MaterialObjectEntity : MonoBehaviour, M8.IPoolSpawn, M8.IPoolDespawn, IBeginDragHandler, IDragHandler, IEndDragHandler {
        public const string parmData = "d";
        public const string parmDragWidget = "dw";
        public const string parmState = "s";
        public const string parmIsNonPool = "np";

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
        public M8.SpriteColorGroup ghostSpriteGroup;
        public SpriteShapeRenderer ghostSpriteShape;

        [Header("Data")]
        public float density = 0f; //set to 0 to not apply

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
                if(!mPoolDataCtrl) {
                    mPoolDataCtrl = GetComponent<M8.PoolDataController>();
                    if(!mPoolDataCtrl)
                        mPoolDataCtrl = gameObject.AddComponent<M8.PoolDataController>();
                }

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

        public bool isDraggable { get { return mDragWidget != null; } }

        private State mState;

        private M8.PoolDataController mPoolDataCtrl;

        private MaterialObjectDragWidget mDragWidget;

        private Coroutine mRout;

        private ContactFilter2D mOverlapFilter = new ContactFilter2D();
        private Collider2D[] mOverlapColls = new Collider2D[8];

        private Color mGhostSpriteShapeDefaultColor;

        private bool mIsDragging;
        private Vector2 mLastPos;

        private bool mIsNonPool;

        private bool mIsGamePlay;

        /// <summary>
        /// Only call this in ghost mode
        /// </summary>
        public void UpdateGhostPosition(Vector2 pos) {
            position = pos;

            //change ghost display if placeable or not
            if(ghostSpriteGroup) {
                if(isPlaceable)
                    ghostSpriteGroup.ApplyColor(GameData.instance.objectGhostValidColor);
                else
                    ghostSpriteGroup.ApplyColor(GameData.instance.objectGhostInvalidColor);
            }

            if(ghostSpriteShape) {
                if(isPlaceable)
                    ghostSpriteShape.color = GameData.instance.objectGhostValidColor;
                else
                    ghostSpriteShape.color = GameData.instance.objectGhostInvalidColor;
            }
        }

        public void Release() {
            if(mIsNonPool)
                gameObject.SetActive(false);
            else
                poolDataCtrl.Release();
        }

        void OnApplicationFocus(bool focus) {
            if(!focus)
                EndDrag();
        }

        void OnEnable() {
            var gameDat = GameData.instance;

            gameDat.signalGamePlay.callback += OnGamePlay;
            gameDat.signalGameStop.callback += OnGameStop;
        }

        void OnDisable() {
            var gameDat = GameData.instance;

            gameDat.signalGamePlay.callback -= OnGamePlay;
            gameDat.signalGameStop.callback -= OnGameStop;

            EndDrag();
        }

        void Awake() {
            body = GetComponent<Rigidbody2D>();
            coll = GetComponent<Collider2D>();

            if(density > 0f) {
                body.useAutoMass = true;
                coll.density = density;
            }

            if(ghostSpriteGroup)
                ghostSpriteGroup.Init();

            if(ghostSpriteShape)
                mGhostSpriteShapeDefaultColor = ghostSpriteShape.color;
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {
            if(!isDraggable || mIsGamePlay || state != State.Normal)
                return;

            mDragWidget.Setup(data);
            mDragWidget.transform.position = eventData.position;

            state = State.Ghost;
            mIsDragging = true;

            mLastPos = position;

            GameData.instance.signalDragBegin.Invoke();
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
            if(!mIsDragging)
                return;

            //check if we are dropping on delete area
            var deleteTag = GameData.instance.deleteTag;

            var isDelete = false;

            if(eventData.pointerDrag && eventData.pointerDrag.CompareTag(deleteTag))
                isDelete = true;
            else if(eventData.pointerCurrentRaycast.gameObject && eventData.pointerCurrentRaycast.gameObject.CompareTag(deleteTag))
                isDelete = true;

            if(isDelete)
                Release();

            EndDrag();
        }

        void M8.IPoolSpawn.OnSpawned(M8.GenericParams parms) {
            //setup data
            var toState = State.None;
            data = null;
            mDragWidget = null;
            mIsNonPool = false;

            mIsGamePlay = false;

            if(parms != null) {
                if(parms.ContainsKey(parmData))
                    data = parms.GetValue<MaterialObjectData>(parmData);

                if(parms.ContainsKey(parmDragWidget))
                    mDragWidget = parms.GetValue<MaterialObjectDragWidget>(parmDragWidget);

                if(parms.ContainsKey(parmState))
                    toState = parms.GetValue<State>(parmState);

                if(parms.ContainsKey(parmIsNonPool))
                    mIsNonPool = parms.GetValue<bool>(parmIsNonPool);
            }

            mOverlapFilter.SetLayerMask(GameData.instance.placementLayerMask);
            mOverlapFilter.useTriggers = true;

            mState = toState;
            ApplyCurrentState();
        }

        void M8.IPoolDespawn.OnDespawned() {
            state = State.None;

            data = null;
            mDragWidget = null;

            EndDrag();
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
            mIsGamePlay = true;
        }

        void OnGameStop() {
            mIsGamePlay = false;
        }

        private void ApplyCurrentState() {
            if(mRout != null) {
                StopCoroutine(mRout);
                mRout = null;
            }

            var enableDisplay = true;
            var toPhysicsMode = PhysicsMode.None;

            if(ghostSpriteGroup)
                ghostSpriteGroup.Revert();

            if(ghostSpriteShape)
                ghostSpriteShape.color = mGhostSpriteShapeDefaultColor;

            switch(mState) {
                case State.None:
                    enableDisplay = false;
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

            GameData.instance.signalDragEnd.Invoke();
        }
    }
}