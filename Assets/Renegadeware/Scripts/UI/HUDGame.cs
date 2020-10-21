using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware.K2PS2 {
    public class HUDGame : MonoBehaviour {
        [Header("Edit")]
        public GameObject editRootGO; //also includes Play button

        public M8.Animator.Animate editAnimator;
        [M8.Animator.TakeSelector(animatorField = "editAnimator")]
        public string editTakeEnter;
        [M8.Animator.TakeSelector(animatorField = "editAnimator")]
        public string editTakeExit;

        [Header("Palette")]
        public MaterialObjectPaletteWidget[] paletteWidgets;

        [Header("Drag")]
        public GameObject dragRootGO;

        [Header("Trash")]
        public GameObject trashRootGO;

        public M8.Animator.Animate trashAnimator;
        [M8.Animator.TakeSelector(animatorField = "trashAnimator")]
        public string trashTakeEnter;
        [M8.Animator.TakeSelector(animatorField = "trashAnimator")]
        public string trashTakeExit;

        [Header("Stop")]
        public GameObject stopRootGO;

        public M8.Animator.Animate stopAnimator;
        [M8.Animator.TakeSelector(animatorField = "stopAnimator")]
        public string stopTakeEnter;
        [M8.Animator.TakeSelector(animatorField = "stopAnimator")]
        public string stopTakeExit;

        public bool isPaletteActive { get; private set; }

        private int mCurPaletteInd;

        private Coroutine mRout;

        public void RefreshCurrentPalette() {
            paletteWidgets[mCurPaletteInd].Refresh(false);
        }

        public void ActivatePalette(int paletteIndex) {
            if(mCurPaletteInd != paletteIndex) {
                mCurPaletteInd = paletteIndex;

                //TODO: animation

                paletteWidgets[mCurPaletteInd].Refresh(false);
                paletteWidgets[mCurPaletteInd].transform.SetAsLastSibling();
            }
        }

        public void Init(LevelData data) {
            HideAll();

            //initialize palette widgets
            int paletteCount = Mathf.Min(paletteWidgets.Length, data.tags.Length);
            for(int i = 0; i < paletteCount; i++) {
                paletteWidgets[i].gameObject.SetActive(true);
                paletteWidgets[i].Setup(data.tags[i], data.items);
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
        }

        public void Show() {
            ClearRout();

            HideAll();

            isPaletteActive = true;
            RefreshCurrentPalette();

            mRout = StartCoroutine(DoShow());
        }

        public void Hide() {
            ClearRout();

            isPaletteActive = false;

            mRout = StartCoroutine(DoHide());
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

        IEnumerator DoShow() {
            //wait for other animations to end
            while(IsAnyAnimationPlaying())
                yield return null;

            if(editRootGO) editRootGO.SetActive(true);

            if(editAnimator && !string.IsNullOrEmpty(editTakeEnter))
                editAnimator.Play(editTakeEnter);

            mRout = null;
        }

        IEnumerator DoHide() {
            //wait for other animations to end
            while(IsAnyAnimationPlaying())
                yield return null;

            if(editRootGO.activeSelf && editAnimator && !string.IsNullOrEmpty(editTakeExit))
                editAnimator.Play(editTakeExit);

            if(trashRootGO.activeSelf && trashAnimator && !string.IsNullOrEmpty(trashTakeExit))
                trashAnimator.Play(trashTakeExit);

            if(stopRootGO.activeSelf && stopAnimator && !string.IsNullOrEmpty(stopTakeExit))
                stopAnimator.Play(stopTakeExit);

            while(IsAnyAnimationPlaying())
                yield return null;

            mRout = null;

            HideAll();
        }

        IEnumerator DoGamePlay() {
            isPaletteActive = false;

            //wait for other animations to end
            while(IsAnyAnimationPlaying())
                yield return null;

            //edit exit animation
            if(editAnimator && !string.IsNullOrEmpty(editTakeExit))
                yield return editAnimator.PlayWait(editTakeExit);

            editRootGO.SetActive(false);

            //show stop
            stopRootGO.SetActive(true);

            //stop enter animation
            if(stopAnimator && !string.IsNullOrEmpty(stopTakeEnter))
                stopAnimator.Play(stopTakeEnter);

            mRout = null;
        }

        IEnumerator DoGameStop() {
            isPaletteActive = true;
            RefreshCurrentPalette();

            //wait for other animations to end
            while(IsAnyAnimationPlaying())
                yield return null;

            //stop exit animation
            if(stopAnimator && !string.IsNullOrEmpty(stopTakeExit))
                yield return stopAnimator.PlayWait(stopTakeExit);

            //hide stop
            stopRootGO.SetActive(false);

            //show edit display
            editRootGO.SetActive(true);

            if(editAnimator && !string.IsNullOrEmpty(editTakeEnter))
                editAnimator.Play(editTakeEnter);

            mRout = null;
        }

        IEnumerator DoDragBegin() {
            //show drag
            if(dragRootGO) dragRootGO.SetActive(true);

            //wait for other animations to end
            while(IsAnyAnimationPlaying())
                yield return null;

            //hide edit
            if(editAnimator && !string.IsNullOrEmpty(editTakeExit))
                yield return editAnimator.PlayWait(editTakeExit);

            //show trash
            trashRootGO.SetActive(true);

            //trash enter animation
            if(trashAnimator && !string.IsNullOrEmpty(trashTakeEnter))
                trashAnimator.Play(trashTakeEnter);

            mRout = null;
        }

        IEnumerator DoDragEnd() {
            //hide drag
            if(dragRootGO) dragRootGO.SetActive(false);

            //wait for other animations to end
            while(IsAnyAnimationPlaying())
                yield return null;

            //hide trash
            if(trashAnimator && !string.IsNullOrEmpty(trashTakeExit))
                yield return trashAnimator.PlayWait(trashTakeExit);

            if(trashRootGO) trashRootGO.SetActive(false);

            //show edit display
            if(editAnimator && !string.IsNullOrEmpty(editTakeEnter))
                editAnimator.Play(editTakeEnter);

            mRout = null;
        }

        private bool IsAnyAnimationPlaying() {
            return (editAnimator && editAnimator.isPlaying) 
                || (trashAnimator && trashAnimator.isPlaying)
                || (stopAnimator && stopAnimator.isPlaying);
        }

        private void HideAll() {
            editRootGO.SetActive(false);
            dragRootGO.SetActive(false);
            trashRootGO.SetActive(false);
            stopRootGO.SetActive(false);

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