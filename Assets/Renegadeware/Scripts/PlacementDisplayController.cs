using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware.K2PS2 {
    [ExecuteInEditMode]
    public class PlacementDisplayController : MonoBehaviour {
        [Header("Config")]
        public DG.Tweening.Ease fadeInEase = DG.Tweening.Ease.OutSine;
        public DG.Tweening.Ease fadeOutEase = DG.Tweening.Ease.InSine;
        public float fadeDelay = 0.3f;

        [Header("Display")]
        public SpriteRenderer gridSprite;

        private Color mDefaultColor;

        private DG.Tweening.EaseFunction mFadeInEaseFunc;
        private DG.Tweening.EaseFunction mFadeOutEaseFunc;

        void OnDestroy() {
            if(Application.isPlaying) {
                var gameDat = GameData.instance;

                gameDat.signalDragBegin.callback -= OnDragBegin;
                gameDat.signalDragEnd.callback -= OnDragEnd;
            }
        }

        void Awake() {
            if(Application.isPlaying) {
                gridSprite.gameObject.SetActive(false);

                mDefaultColor = gridSprite.color;

                //default hidden
                gridSprite.color = new Color(mDefaultColor.r, mDefaultColor.g, mDefaultColor.b, 0f);

                mFadeInEaseFunc = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(fadeInEase);
                mFadeOutEaseFunc = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(fadeOutEase);

                var gameDat = GameData.instance;

                gameDat.signalDragBegin.callback += OnDragBegin;
                gameDat.signalDragEnd.callback += OnDragEnd;
            }
        }

#if UNITY_EDITOR
        void Update() {
            if(!Application.isPlaying && gridSprite) {
                var boxColl = GetComponent<BoxCollider2D>();
                gridSprite.transform.localPosition = boxColl.offset;
                gridSprite.size = boxColl.size;
            }
        }
#endif

        void OnDragBegin() {
            StopAllCoroutines();
            StartCoroutine(DoFadeIn());
        }

        void OnDragEnd() {
            StopAllCoroutines();
            StartCoroutine(DoFadeOut());
        }

        IEnumerator DoFadeIn() {
            gridSprite.gameObject.SetActive(true);

            var startAlpha = gridSprite.color.a;
            var endAlpha = mDefaultColor.a;

            var lastTime = Time.realtimeSinceStartup;

            var curTime = 0f;
            while(curTime < fadeDelay) {
                yield return null;

                curTime = Time.realtimeSinceStartup - lastTime;

                var t = mFadeInEaseFunc(curTime, fadeDelay, 0f, 0f);

                gridSprite.color = new Color(mDefaultColor.r, mDefaultColor.g, mDefaultColor.b, Mathf.Lerp(startAlpha, endAlpha, t));
            }
        }

        IEnumerator DoFadeOut() {
            var startAlpha = mDefaultColor.a;
            var endAlpha = 0f;

            var lastTime = Time.realtimeSinceStartup;

            var curTime = 0f;
            while(curTime < fadeDelay) {
                yield return null;

                curTime = Time.realtimeSinceStartup - lastTime;

                var t = mFadeOutEaseFunc(curTime, fadeDelay, 0f, 0f);

                gridSprite.color = new Color(mDefaultColor.r, mDefaultColor.g, mDefaultColor.b, Mathf.Lerp(startAlpha, endAlpha, t));
            }

            gridSprite.gameObject.SetActive(false);
        }
    }
}