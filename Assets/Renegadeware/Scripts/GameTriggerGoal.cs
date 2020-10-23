using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware.K2PS2 {
    public class GameTriggerGoal : MonoBehaviour {
        [Header("Animation")]
        public M8.Animator.Animate animator;
        [M8.Animator.TakeSelector(animatorField = "animator")]
        public string takeActive;
        [M8.Animator.TakeSelector(animatorField = "animator")]
        public string takeCollect;

        [Header("SFX")]
        [M8.SoundPlaylist]
        public string sfxCollect;

        private bool mIsTriggered;

        void OnTriggerEnter2D(Collider2D collision) {
            if(mIsTriggered)
                return;

            var gameDat = GameData.instance;

            //check if it's a player
            if(collision.CompareTag(gameDat.playerTag)) {
                mIsTriggered = true;

                M8.SoundPlaylist.instance.Play(sfxCollect, false);

                gameDat.signalGoal.Invoke();

                StartCoroutine(DoGoalTrigger());
            }
        }

        void Start() {
            animator.Play(takeActive);
        }

        IEnumerator DoGoalTrigger() {
            yield return animator.PlayWait(takeCollect);

            gameObject.SetActive(false);
        }
    }
}