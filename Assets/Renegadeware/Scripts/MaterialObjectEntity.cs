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
        public M8.SpriteColorGroup ghostSpriteGroup;
        public SpriteShapeRenderer ghostSpriteShape;

        [Header("Animation")]
        public M8.Animator.Animate animator;
        [M8.Animator.TakeSelector(animatorField="animator")]
        public string takeDefault;
        [M8.Animator.TakeSelector(animatorField = "animator")]
        public string takeSpawn;
        [M8.Animator.TakeSelector(animatorField = "animator")]
        public string takeDespawn;

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
                if(M8.SceneManager.instance.isPaused)
                    transform.position = value;
                else if(body.simulated)
                    body.position = value;
                else
                    transform.position = value;
            }
        }

        public float rotation {
            get { return body.simulated ? body.rotation : transform.eulerAngles.z; }
            set {
                if(M8.SceneManager.instance.isPaused) {
                    var r = transform.eulerAngles; r.z = value;
                    transform.eulerAngles = r;
                }
                else if(body.simulated)
                    body.rotation = value;
                else {
                    var r = transform.eulerAngles; r.z = value;
                    transform.eulerAngles = r;
                }
            }
        }

        public bool isPlaceable {
            get {
                if(!coll)
                    return false;

                var placementTag = GameData.instance.placementTag;
                var ignoreTags = GameData.instance.placementIgnoreTags;

                Collider2D overlapColl = null;

                int overlapCount = coll.OverlapCollider(mOverlapFilter, mOverlapColls);
                for(int i = 0; i < overlapCount; i++) {
                    var _overlapColl = mOverlapColls[i];

                    if(_overlapColl == coll)
                        continue;
                    else if(_overlapColl.CompareTag(placementTag))
                        overlapColl = _overlapColl;
                    else if(GameUtils.CheckTags(_overlapColl, ignoreTags))
                        continue;
                    else
                        return false;
                }

                if(overlapColl) {
                    var bounds = coll.bounds;
                    var overlapBounds = overlapColl.bounds;

                    return overlapBounds.min.x < bounds.min.x && overlapBounds.min.y < bounds.min.y && overlapBounds.max.x > bounds.max.x && overlapBounds.max.y > bounds.max.y;
                }

                return false;
            }
        }

        public MaterialObjectData data { get; private set; }

        public Rigidbody2D body { get; private set; }

        public Collider2D coll { get; private set; }

        public ConductiveController conductive { get; private set; }

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
        private float mLastRot;

        private bool mIsNonPool;

        private bool mIsGamePlay;

        /// <summary>
        /// Only call this in ghost mode
        /// </summary>
        public bool UpdateGhostPosition(Vector2 pos) {
            var _isPlaceable = isPlaceable;

            position = pos;

            //change ghost display if placeable or not
            if(ghostSpriteGroup) {
                if(_isPlaceable)
                    ghostSpriteGroup.ApplyColor(GameData.instance.objectGhostValidColor);
                else
                    ghostSpriteGroup.ApplyColor(GameData.instance.objectGhostInvalidColor);
            }

            if(ghostSpriteShape) {
                if(_isPlaceable)
                    ghostSpriteShape.color = GameData.instance.objectGhostValidColor;
                else
                    ghostSpriteShape.color = GameData.instance.objectGhostInvalidColor;
            }

            return _isPlaceable;
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
            gameDat.signalObjectDespawn.callback += OnDespawn;
        }

        void OnDisable() {
            var gameDat = GameData.instance;

            gameDat.signalGamePlay.callback -= OnGamePlay;
            gameDat.signalGameStop.callback -= OnGameStop;
            gameDat.signalObjectDespawn.callback -= OnDespawn;

            EndDrag();
        }

        void Awake() {
            body = GetComponentInChildren<Rigidbody2D>();
            coll = GetComponentInChildren<Collider2D>();
            conductive = GetComponent<ConductiveController>();

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

            GameData.instance.signalDragBegin.Invoke();

            mLastPos = position;
            mLastRot = rotation;

            rotation = 0f;
        }

        void IDragHandler.OnDrag(PointerEventData eventData) {
            if(!mIsDragging)
                return;

            var pos = eventData.position;

            //update position
            var cam = Camera.main;
            var entPos = cam.ScreenToWorldPoint(pos);

            var valid = UpdateGhostPosition(entPos);

            //update drag widget position
            mDragWidget.transform.position = pos;
            mDragWidget.SetValid(valid);
        }
        
        void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
            if(!mIsDragging)
                return;

            var gameDat = GameData.instance;

            var cast = eventData.pointerCurrentRaycast;

            //check ui contact
            if(cast.isValid && (gameDat.uiLayerMask & (1 << cast.gameObject.layer)) != 0) {
                var go = cast.gameObject;
                
                //delete object?
                if(go.CompareTag(gameDat.deleteTag)) {
                    Release();
                    return;
                }

                MaterialObjectPaletteWidget palette = null;

                //material object widget?
                if(go.CompareTag(gameDat.materialObjectTag)) {
                    var matObjWidget = go.GetComponent<MaterialObjectWidget>();
                    if(matObjWidget)
                        palette = matObjWidget.palette;
                }
                //palette widget?
                else if(go.CompareTag(gameDat.placementTag)) {
                    palette = go.GetComponent<MaterialObjectPaletteWidget>();
                }

                if(palette && !palette.isFull) {
                    palette.AddItem(data);
                    Release();
                    return;
                }
            }

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

            if(animator && !string.IsNullOrEmpty(takeDefault))
                animator.Play(takeDefault);

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
            if(animator) {
                //wait for animation to finish
                while(animator.isPlaying)
                    yield return null;

                //Do animation
                if(!string.IsNullOrEmpty(takeSpawn))
                    yield return animator.PlayWait(takeSpawn);
            }

            mRout = null;

            state = State.Normal;
        }

        IEnumerator DoDespawn() {
            if(animator) {
                //wait for animation to finish
                while(animator.isPlaying)
                    yield return null;

                //Do animation
                if(!string.IsNullOrEmpty(takeDespawn))
                    yield return animator.PlayWait(takeDespawn);
            }

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

        void OnDespawn() {
            state = State.Despawning;
        }

        private void ApplyCurrentState() {
            if(mRout != null) {
                StopCoroutine(mRout);
                mRout = null;
            }
            var toPhysicsMode = PhysicsMode.None;

            if(ghostSpriteGroup)
                ghostSpriteGroup.Revert();

            if(ghostSpriteShape)
                ghostSpriteShape.color = mGhostSpriteShapeDefaultColor;

            switch(mState) {
                case State.None:
                    break;
                case State.Ghost:
                    toPhysicsMode = PhysicsMode.Ghost;

                    if(conductive)
                        conductive.active = false;
                    break;
                case State.Spawning:
                    mRout = StartCoroutine(DoSpawn());
                    break;
                case State.Normal:
                    toPhysicsMode = PhysicsMode.Normal;

                    if(conductive)
                        conductive.active = true;
                    break;
                case State.Despawning:
                    mRout = StartCoroutine(DoDespawn());
                    break;
            }

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
                        body.velocity = Vector2.zero;
                        body.angularVelocity = 0f;
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
                state = State.Normal;

                if(!isPlaceable) {
                    position = mLastPos;
                    rotation = mLastRot;
                }
            }

            mIsDragging = false;

            GameData.instance.signalDragEnd.Invoke();
        }
    }
}