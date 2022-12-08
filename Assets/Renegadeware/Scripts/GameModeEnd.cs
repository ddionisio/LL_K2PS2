using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LoLExt;

namespace Renegadeware.K2PS2 {
    public class GameModeEnd : GameModeController<GameModeEnd> {
        [Header("End")]
        public M8.Animator.Animate endAnimator;
        [M8.Animator.TakeSelector(animatorField = "endAnimator")]
        public string endTakePlay;

        [Header("Complete")]
        public GameObject completeGO;
        public M8.Animator.Animate completeAnimator;
        [M8.Animator.TakeSelector(animatorField = "completeAnimator")]
        public string completeTakePlay;

        public M8.TextMeshPro.TextMeshProCounter scoreCounter;

        [Header("Music")]
        [M8.MusicPlaylist]
        public string music;

        protected override void OnInstanceInit() {
            base.OnInstanceInit();

            endAnimator.ResetTake(endTakePlay);

            completeGO.SetActive(false);

            scoreCounter.SetCountImmediate(0);
            scoreCounter.count = LoLManager.instance.curScore;
        }

        protected override IEnumerator Start() {
            yield return base.Start();

            M8.MusicPlaylist.instance.Play(music, true, false);

            yield return endAnimator.PlayWait(endTakePlay);

            completeGO.SetActive(true);

            yield return completeAnimator.PlayWait(completeTakePlay);

            LoLManager.instance.Complete();
        }
    }
}