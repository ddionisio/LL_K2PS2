using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware.K2PS2 {
    public class HUDGame : MonoBehaviour {
        [Header("Display")]
        public GameObject displayRootGO;

        [Header("Palette")]
        public GameObject paletteRootGO; //also includes Play button
        public MaterialObjectPaletteWidget[] paletteWidgets;

        [Header("Drag")]
        public GameObject dragRootGO;
        public MaterialObjectDragWidget dragWidget;

        [Header("Trash")]
        public GameObject trashGO;

        [Header("Stop")]
        public GameObject stopGO;

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
            var gameDat = GameData.instance;
            gameDat.signalDragBegin.callback += OnDragBegin;
            gameDat.signalDragEnd.callback += OnDragEnd;
            gameDat.signalGamePlay.callback += OnGamePlay;
            gameDat.signalGameStop.callback += OnGameStop;
        }

        public void Deinit() {
            ClearRout();

            //clear signals
            var gameDat = GameData.instance;
            gameDat.signalDragBegin.callback -= OnDragBegin;
            gameDat.signalDragEnd.callback -= OnDragEnd;
            gameDat.signalGamePlay.callback -= OnGamePlay;
            gameDat.signalGameStop.callback -= OnGameStop;

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

        void OnGamePlay() {
            ClearRout();

            mRout = StartCoroutine(DoGamePlay());
        }

        void OnGameStop() {
            ClearRout();

            mRout = StartCoroutine(DoGameStop());
        }

        void OnDragBegin() {
            ClearRout();

            mRout = StartCoroutine(DoDragBegin());
        }

        void OnDragEnd() {
            ClearRout();

            mRout = StartCoroutine(DoDragEnd());
        }

        IEnumerator DoGamePlay() {
            yield return null;

            //hide palette animation

            //hide palettes
            if(paletteRootGO) paletteRootGO.SetActive(false);
                        
            //show stop
            if(stopGO) stopGO.SetActive(true);

            mRout = null;
        }

        IEnumerator DoGameStop() {
            yield return null;

            //hide stop animation

            //hide stop
            if(stopGO) stopGO.SetActive(false);

            //show palettes
            if(paletteRootGO) paletteRootGO.SetActive(true);

            //show palettes animation

            mRout = null;
        }

        IEnumerator DoDragBegin() {
            //show drag
            if(dragRootGO) dragRootGO.SetActive(true);

            //wait for other animations to end

            yield return null;

            //hide palettes
            if(paletteRootGO) paletteRootGO.SetActive(false);

            //show trash
            if(trashGO) trashGO.SetActive(true);

            mRout = null;
        }

        IEnumerator DoDragEnd() {
            //hide drag
            if(dragRootGO) dragRootGO.SetActive(false);

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
            if(stopGO) stopGO.SetActive(false);
        }

        private void ClearRout() {
            if(mRout != null) {
                StopCoroutine(mRout);
                mRout = null;
            }
        }
    }
}