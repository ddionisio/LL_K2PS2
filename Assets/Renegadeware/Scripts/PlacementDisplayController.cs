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
        public GameObject gridSpriteRootGO;
        public M8.SpriteColorAlphaGroup gridAlpha;        

        private DG.Tweening.EaseFunction mFadeInEaseFunc;
        private DG.Tweening.EaseFunction mFadeOutEaseFunc;

        void OnEnable() {
            if(Application.isPlaying) {
                //default hidden
                gridAlpha.alpha = 0f;
                gridSpriteRootGO.SetActive(false);

                var gameDat = GameData.instance;

                gameDat.signalDragBegin.callback += OnDragBegin;
                gameDat.signalDragEnd.callback += OnDragEnd;
            }
        }

        void OnDisable() {
            if(Application.isPlaying) {
                var gameDat = GameData.instance;

                gameDat.signalDragBegin.callback -= OnDragBegin;
                gameDat.signalDragEnd.callback -= OnDragEnd;
            }
        }

        void Awake() {
            if(Application.isPlaying) {
                mFadeInEaseFunc = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(fadeInEase);
                mFadeOutEaseFunc = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(fadeOutEase);
            }
        }

#if UNITY_EDITOR
        void Update() {
            if(!Application.isPlaying && gridSpriteRootGO) {
                var boxColl = GetComponent<BoxCollider2D>();
                var gridSprites = gridSpriteRootGO.GetComponentsInChildren<SpriteRenderer>();
                for(int i = 0; i < gridSprites.Length; i++) {
                    var gridSprite = gridSprites[i];
                    gridSprite.transform.localPosition = boxColl.offset;
                    gridSprite.size = boxColl.size;
                }
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
            gridSpriteRootGO.SetActive(true);

            var startAlpha = gridAlpha.alpha;
            var endAlpha = 1f;

            var lastTime = Time.realtimeSinceStartup;

            var curTime = 0f;
            while(curTime < fadeDelay) {
                yield return null;

                curTime = Time.realtimeSinceStartup - lastTime;

                var t = mFadeInEaseFunc(curTime, fadeDelay, 0f, 0f);

                gridAlpha.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            }
        }

        IEnumerator DoFadeOut() {
            var startAlpha = gridAlpha.alpha;
            var endAlpha = 0f;

            var lastTime = Time.realtimeSinceStartup;

            var curTime = 0f;
            while(curTime < fadeDelay) {
                yield return null;

                curTime = Time.realtimeSinceStartup - lastTime;

                var t = mFadeOutEaseFunc(curTime, fadeDelay, 0f, 0f);

                gridAlpha.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            }

            gridSpriteRootGO.SetActive(false);
        }
    }
}