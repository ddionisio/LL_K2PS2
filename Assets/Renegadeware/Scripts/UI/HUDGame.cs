using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware.K2PS2 {
    public class HUDGame : MonoBehaviour {
        [Header("Display")]
        public GameObject displayRootGO;
        public GameObject paletteRootGO;
        public MaterialObjectPaletteWidget[] paletteWidgets;

        [Header("Drag")]
        public GameObject dragRootGO;
        public MaterialObjectDragWidget dragWidget;

        [Header("Trash")]
        public GameObject trashGO;

        [Header("Signals")]
        public M8.Signal signalListenDragBegin;
        public M8.Signal signalListenDragEnd;

        private GamePlayData mData;
        private int mCurPaletteInd;

        private Coroutine mRout;

        public void ActivatePalette(int paletteIndex) {
            if(mCurPaletteInd != paletteIndex) {
                mCurPaletteInd = paletteIndex;

                //TODO: animation

                paletteWidgets[mCurPaletteInd].transform.SetAsFirstSibling();
            }
        }

        public void Init(GamePlayData data) {
            mData = data;

            HideAll();

            //initialize palette widgets
            int paletteCount = Mathf.Min(paletteWidgets.Length, mData.tags.Length);
            for(int i = 0; i < paletteCount; i++) {
                paletteWidgets[i].gameObject.SetActive(true);
                paletteWidgets[i].Setup(mData, mData.tags[i], dragWidget);
                paletteWidgets[i].transform.SetSiblingIndex(i);
            }

            //hide other palette widgets
            for(int i = paletteCount; i < paletteWidgets.Length; i++)
                paletteWidgets[i].gameObject.SetActive(false);

            mCurPaletteInd = 0;

            //setup signals
            if(signalListenDragBegin) signalListenDragBegin.callback += OnDragBegin;
            if(signalListenDragEnd) signalListenDragEnd.callback += OnDragEnd;
        }

        public void Deinit() {
            ClearRout();

            //clear signals
            if(signalListenDragBegin) signalListenDragBegin.callback -= OnDragBegin;
            if(signalListenDragEnd) signalListenDragEnd.callback -= OnDragEnd;

            //clear out widgets
            for(int i = 0; i < paletteWidgets.Length; i++)
                paletteWidgets[i].gameObject.SetActive(false);

            HideAll();

            mData = null;
        }

        public void Show() {
            if(displayRootGO) displayRootGO.SetActive(true);
            if(paletteRootGO) paletteRootGO.SetActive(true);

            //TODO: play animation
        }

        void Awake() {
            //initial setup
            HideAll();
        }

        void OnDragBegin() {
            ClearRout();

            mRout = StartCoroutine(DoDragBegin());
        }

        void OnDragEnd() {
            ClearRout();

            mRout = StartCoroutine(DoDragEnd());
        }

        IEnumerator DoDragBegin() {
            //wait for other animations to end

            yield return null;

            //hide palettes
            if(paletteRootGO) paletteRootGO.SetActive(false);

            //show trash
            if(trashGO) trashGO.SetActive(true);

            mRout = null;
        }

        IEnumerator DoDragEnd() {
            //wait for other animations to end

            yield return null;

            //hide trash
            if(trashGO) trashGO.SetActive(false);

            //show palettes
            paletteWidgets[mCurPaletteInd].Refresh();

            if(paletteRootGO) paletteRootGO.SetActive(true);

            mRout = null;
        }

        private void HideAll() {
            if(displayRootGO) displayRootGO.SetActive(false);
            if(dragRootGO) dragRootGO.SetActive(false);
            if(trashGO) trashGO.SetActive(false);
        }

        private void ClearRout() {
            if(mRout != null) {
                StopCoroutine(mRout);
                mRout = null;
            }
        }
    }
}