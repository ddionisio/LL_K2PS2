using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware.K2PS2 {
    public class HUDClassify : MonoBehaviour {
        [Header("Palette")]
        public GameObject paletteRootGO;
        public MaterialObjectPaletteWidget[] paletteWidgets;

        public M8.Animator.Animate paletteAnimator;
        [M8.Animator.TakeSelector(animatorField = "paletteAnimator")]
        public string paletteTakeEnter;
        [M8.Animator.TakeSelector(animatorField = "paletteAnimator")]
        public string paletteTakeExit;

        [Header("Drag")]
        public GameObject dragRootGO;
        public MaterialObjectDragWidget dragWidget;

        [Header("Classify")]
        public GameObject classifyRootGO;

        public M8.Animator.Animate classifyAnimator;
        [M8.Animator.TakeSelector(animatorField = "classifyAnimator")]
        public string classifyTakeEnter;
        [M8.Animator.TakeSelector(animatorField = "classifyAnimator")]
        public string classifyTakeExit;

        public bool isBusy { get { return mRout != null; } }

        public int errorCount { get; private set; }

        private Coroutine mRout;

        public void Init(LevelData data) {
            var gameDat = GameData.instance;

            var paletteCount = Mathf.Min(paletteWidgets.Length, data.tags.Length);

            for(int i = 0; i < paletteCount; i++) {
                paletteWidgets[i].gameObject.SetActive(true);
                paletteWidgets[i].Setup(data.tags[i], null);
            }

            for(int i = paletteCount; i < paletteWidgets.Length; i++)
                paletteWidgets[i].gameObject.SetActive(false);

            gameDat.signalDragBegin.callback += OnDragBegin;
            gameDat.signalDragEnd.callback += OnDragEnd;
            gameDat.signalClassify.callback += OnClassify;
        }

        public void Deinit() {
            var gameDat = GameData.instance;

            gameDat.signalDragBegin.callback -= OnDragBegin;
            gameDat.signalDragEnd.callback -= OnDragEnd;
            gameDat.signalClassify.callback -= OnClassify;

            ClearRout();

            HideAll(true);
        }

        public void ShowPalette() {
            ClearRout();

            if(paletteRootGO)
                paletteRootGO.SetActive(true);

            mRout = StartCoroutine(DoAnimation(paletteAnimator, paletteTakeEnter, null));
        }

        public void HidePalette() {
            ClearRout();

            mRout = StartCoroutine(DoAnimation(paletteAnimator, paletteTakeExit, paletteRootGO));
        }

        public void ShowClassify() {
            ClearRout();

            if(classifyRootGO)
                classifyRootGO.SetActive(true);

            mRout = StartCoroutine(DoAnimation(classifyAnimator, classifyTakeEnter, null));
        }

        public void HideClassify() {
            ClearRout();

            mRout = StartCoroutine(DoAnimation(classifyAnimator, classifyTakeExit, classifyRootGO));
        }

        public void HideAll(bool instant) {
            if(instant) {
                if(paletteRootGO) paletteRootGO.SetActive(false);
                if(classifyRootGO) classifyRootGO.SetActive(false);
                if(dragRootGO) dragRootGO.SetActive(false);
            }
            else {
                ClearRout();
                mRout = StartCoroutine(DoHideAll());
            }
        }

        void Awake() {
            HideAll(true);
        }

        void OnDragBegin() {
            if(dragRootGO) dragRootGO.SetActive(true);
        }

        void OnDragEnd() {
            if(dragRootGO) dragRootGO.SetActive(false);
        }

        void OnClassify() {
            //check for errors
            errorCount = 0;

            for(int i = 0; i < paletteWidgets.Length; i++) {
                var palette = paletteWidgets[i];

                var paletteErrorCount = 0;

                for(int j = 0; j < palette.itemActives.Count; j++) {
                    var itm = palette.itemActives[j];

                    if(itm.data.CompareTag(palette.tagData)) {
                        itm.SetError(false);
                    }
                    else {
                        itm.SetError(true);
                        paletteErrorCount++;
                    }
                }

                if(paletteErrorCount > 0)
                    palette.Error();

                errorCount += paletteErrorCount;
            }

            //show error message
        }

        IEnumerator DoAnimation(M8.Animator.Animate animator, string take, GameObject disableAtEndGO) {
            if(animator && !string.IsNullOrEmpty(take))
                yield return animator.PlayWait(take);

            if(disableAtEndGO)
                disableAtEndGO.SetActive(false);

            mRout = null;
        }

        IEnumerator DoHideAll() {
            if(paletteAnimator && !string.IsNullOrEmpty(paletteTakeExit))
                paletteAnimator.Play(paletteTakeExit);

            if(classifyAnimator && !string.IsNullOrEmpty(classifyTakeExit))
                classifyAnimator.Play(classifyTakeExit);

            while((paletteAnimator && paletteAnimator.isPlaying) || (classifyAnimator && classifyAnimator.isPlaying))
                yield return null;

            mRout = null;

            HideAll(true);
        }

        private void ClearRout() {
            if(mRout != null) {
                StopCoroutine(mRout);
                mRout = null;
            }
        }
    }
}