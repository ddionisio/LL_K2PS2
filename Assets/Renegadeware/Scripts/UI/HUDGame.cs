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

        [Header("Play")]
        public GameObject playGO;

        [Header("Stop")]
        public GameObject stopGO;

        public bool isPaletteActive { get; private set; }

        private LevelData mData;
        private int mCurPaletteInd;

        private Coroutine mRout;

        public void RefreshCurrentPalette() {
            paletteWidgets[mCurPaletteInd].Refresh();
        }

        public void ActivatePalette(int paletteIndex) {
            if(mCurPaletteInd != paletteIndex) {
                mCurPaletteInd = paletteIndex;

                //TODO: animation

                paletteWidgets[mCurPaletteInd].transform.SetAsLastSibling();
            }
        }

        public void Init(LevelData data) {
            mData = data;

            HideAll();

            //initialize palette widgets
            int paletteCount = Mathf.Min(paletteWidgets.Length, mData.tags.Length);
            for(int i = 0; i < paletteCount; i++) {
                paletteWidgets[i].gameObject.SetActive(true);
                paletteWidgets[i].Setup(mData, mData.tags[i], dragWidget);
                paletteWidgets[i].transform.SetAsFirstSibling();
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
            gameDat.signalObjectReleased.callback += OnObjectReleased;
        }

        public void Deinit() {
            ClearRout();

            //clear signals
            var gameDat = GameData.instance;
            gameDat.signalDragBegin.callback -= OnDragBegin;
            gameDat.signalDragEnd.callback -= OnDragEnd;
            gameDat.signalGamePlay.callback -= OnGamePlay;
            gameDat.signalGameStop.callback -= OnGameStop;
            gameDat.signalObjectReleased.callback -= OnObjectReleased;

            //clear out widgets
            for(int i = 0; i < paletteWidgets.Length; i++)
                paletteWidgets[i].gameObject.SetActive(false);

            HideAll();

            mData = null;
        }

        public void Show() {
            HideAll();

            isPaletteActive = true;
            RefreshCurrentPalette();            

            if(displayRootGO) displayRootGO.SetActive(true);
            if(paletteRootGO) paletteRootGO.SetActive(true);
            if(playGO) playGO.SetActive(true);

            //TODO: play animation
        }

        public void Hide() {
            //TODO: play animation

            HideAll();
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
            isPaletteActive = false;

            ClearRout();

            mRout = StartCoroutine(DoDragBegin());
        }

        void OnDragEnd() {
            isPaletteActive = true;
            RefreshCurrentPalette();

            ClearRout();

            mRout = StartCoroutine(DoDragEnd());
        }

        void OnObjectReleased() {
            if(isPaletteActive)
                RefreshCurrentPalette();
        }

        IEnumerator DoGamePlay() {
            isPaletteActive = false;

            //wait for other animations to end

            yield return null;

            //palette exit animation

            //hide edit display
            if(paletteRootGO) paletteRootGO.SetActive(false);
            if(playGO) playGO.SetActive(false);

            //show stop
            if(stopGO) stopGO.SetActive(true);

            //stop enter animation

            mRout = null;
        }

        IEnumerator DoGameStop() {
            isPaletteActive = true;
            RefreshCurrentPalette();

            //wait for other animations to end

            yield return null;

            //stop exit animation

            //hide stop
            if(stopGO) stopGO.SetActive(false);

            //show edit display
            if(paletteRootGO) paletteRootGO.SetActive(true);
            if(playGO) playGO.SetActive(true);

            //palette enter animation

            mRout = null;
        }

        IEnumerator DoDragBegin() {
            //show drag
            if(dragRootGO) dragRootGO.SetActive(true);

            //wait for other animations to end

            yield return null;

            //hide edit display

            //palette exit animation

            //if(paletteRootGO) paletteRootGO.SetActive(false);
            if(playGO) playGO.SetActive(false);

            //show trash
            if(trashGO) trashGO.SetActive(true);

            //trash enter animation

            mRout = null;
        }

        IEnumerator DoDragEnd() {
            //hide drag
            if(dragRootGO) dragRootGO.SetActive(false);

            //wait for other animations to end

            yield return null;

            //hide trash

            //trash exit animation

            if(trashGO) trashGO.SetActive(false);

            //show edit display

            if(paletteRootGO) paletteRootGO.SetActive(true);
            if(playGO) playGO.SetActive(true);

            //palette enter animation

            mRout = null;
        }

        private void HideAll() {
            if(displayRootGO) displayRootGO.SetActive(false);
            if(paletteRootGO) paletteRootGO.SetActive(false);
            if(playGO) playGO.SetActive(false);
            if(dragRootGO) dragRootGO.SetActive(false);
            if(trashGO) trashGO.SetActive(false);
            if(stopGO) stopGO.SetActive(false);

            isPaletteActive = false;
        }

        private void ClearRout() {
            if(mRout != null) {
                StopCoroutine(mRout);
                mRout = null;
            }
        }
    }
}