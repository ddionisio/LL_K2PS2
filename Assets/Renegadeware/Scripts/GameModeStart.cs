﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using LoLExt;

namespace Renegadeware.K2PS2 {
    public class GameModeStart : GameModeController<GameModeStart> {
        [Header("Screen")]
        public GameObject loadingGO;
        public GameObject readyGO;

        [Header("Title")]
        [M8.Localize]
        public string titleRef;
        public TMP_Text titleText;

        [Header("Play")]
        public Button playButton;

        [Header("Ready")]
        public M8.Animator.Animate readyAnimator;
        [M8.Animator.TakeSelector(animatorField = "readyAnimator")]
        public string readyTakeEnter;
        [M8.Animator.TakeSelector(animatorField = "readyAnimator")]
        public string readyTakeExit;

        [Header("Intro")]
        public M8.Animator.Animate introAnimator;
        [M8.Animator.TakeSelector(animatorField = "introAnimator")]
        public string introTakePlay;

        [Header("Music")]
        [M8.MusicPlaylist]
        public string music;


        protected override void OnInstanceInit() {
            base.OnInstanceInit();

            if(loadingGO) loadingGO.SetActive(true);
            if(readyGO) readyGO.SetActive(false);
                        
            //Setup Play
            if(playButton) playButton.onClick.AddListener(OnPlayClick);
        }

        protected override IEnumerator Start() {
            //Loading
            yield return base.Start();

            while(!LoLManager.instance.isReady)
                yield return null;

            if(loadingGO) loadingGO.SetActive(false);

            //Title/Ready

            M8.MusicPlaylist.instance.Play(music, true, true);

            //Setup Title
            if(titleText) titleText.text = M8.Localize.Get(titleRef);

            if(readyGO) readyGO.SetActive(true);

            yield return readyAnimator.PlayWait(readyTakeEnter);
        }

        void OnPlayClick() {
            StartCoroutine(DoPlay());
        }

        IEnumerator DoPlay() {
            yield return readyAnimator.PlayWait(readyTakeExit);

            if(readyGO) readyGO.SetActive(false);

            if(LoLManager.instance.curProgress > 0)
                GameData.instance.Begin();
            else
                StartCoroutine(DoIntro());
        }

        IEnumerator DoIntro() {
            yield return introAnimator.PlayWait(introTakePlay);

            GameData.instance.Begin();
        }
    }
}