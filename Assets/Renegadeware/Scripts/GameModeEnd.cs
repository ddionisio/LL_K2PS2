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

        [Header("Music")]
        [M8.MusicPlaylist]
        public string music;

        protected override void OnInstanceInit() {
            base.OnInstanceInit();

            endAnimator.ResetTake(endTakePlay);

            completeGO.SetActive(false);
        }

        protected override IEnumerator Start() {
            yield return base.Start();

            M8.MusicPlaylist.instance.Play(music, true, false);

            yield return endAnimator.PlayWait(endTakePlay);

            completeGO.SetActive(true);
        }
    }
}