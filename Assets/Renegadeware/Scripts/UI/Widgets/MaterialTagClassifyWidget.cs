using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Renegadeware.K2PS2 {
    public class MaterialTagClassifyWidget : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
        [Header("Display")]
        public Transform displayRoot;

        public MaterialTagWidget tagWidget {
            get {
                if(!mTagWidget)
                    mTagWidget = GetComponent<MaterialTagWidget>();
                return mTagWidget;
            }
        }

        public Graphic graphic {
            get {
                if(!mGraphic)
                    mGraphic = GetComponent<Graphic>();
                return mGraphic;
            }
        }

        public bool isAttached { get { return mAttachTo; } }

        private MaterialTagWidget mTagWidget;
        private Graphic mGraphic;

        private Transform mDragAreaRoot;
        private Transform mPlacementRoot;

        private Transform mAttachTo;
        private Vector2 mAttachLocalPos;

        private Camera mCam;

        private bool mIsDragging;

        public void Setup(MaterialTagData data, Transform dragAreaRoot, Transform placementRoot) {
            tagWidget.Setup(data);

            mDragAreaRoot = dragAreaRoot;
            mPlacementRoot = placementRoot;
        }

        public void Detach() {
            if(!isAttached)
                return;

            //put display back
            RevertDisplayRoot();

            mAttachTo = null;

            graphic.raycastTarget = true;

            mCam = null;
        }

        void OnApplicationFocus(bool focus) {
            if(!focus)
                EndDrag();
        }

        void OnDisable() {
            Detach();
            EndDrag();
        }

        void Update() {
            if(isAttached) {
                var toWorldPos = mAttachTo.localToWorldMatrix.MultiplyPoint(mAttachLocalPos);
                Vector2 screenPos = mCam.WorldToScreenPoint(toWorldPos);

                displayRoot.position = screenPos;
            }
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {
            if(isAttached)
                return;

            mIsDragging = true;

            displayRoot.SetParent(mDragAreaRoot, true);

            GameData.instance.signalDragBegin.Invoke();
        }

        void IDragHandler.OnDrag(PointerEventData eventData) {
            if(!mIsDragging)
                return;

            displayRoot.position = eventData.position;
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
            if(!mIsDragging)
                return;

            //check if we are on object, if so, attach
            var ptrCast = eventData.pointerCurrentRaycast;

            if(ptrCast.isValid && ptrCast.gameObject.CompareTag(GameData.instance.materialObjectTag)) {
                AttachTo(ptrCast.gameObject.transform, ptrCast.worldPosition);
            }

            EndDrag();
        }

        private void AttachTo(Transform attachTo, Vector2 worldPos) {
            Detach(); //fail-safe

            mAttachTo = attachTo;
            mAttachLocalPos = attachTo.worldToLocalMatrix.MultiplyPoint(worldPos);

            graphic.raycastTarget = false; //disable interaction

            displayRoot.SetParent(mPlacementRoot, true);

            mCam = Camera.main;
        }

        private void EndDrag() {
            if(!mIsDragging)
                return;

            if(!isAttached)
                RevertDisplayRoot();

            mIsDragging = false;

            GameData.instance.signalDragEnd.Invoke();
        }

        private void RevertDisplayRoot() {
            displayRoot.SetParent(transform, false);
            displayRoot.localPosition = Vector3.zero;
            displayRoot.localRotation = Quaternion.identity;
        }
    }
}