using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware.K2PS2 {
    public class GameTriggerGoal : MonoBehaviour {
        //animation

        private bool mIsTriggered;

        void OnTriggerEnter2D(Collider2D collision) {
            if(mIsTriggered)
                return;

            var gameDat = GameData.instance;

            //check if it's a player
            if(collision.CompareTag(gameDat.playerTag)) {
                mIsTriggered = true;

                gameDat.signalGoal.Invoke();

                StartCoroutine(DoGoalTrigger());
            }
        }

        IEnumerator DoGoalTrigger() {
            //play animation
            yield return null;

            gameObject.SetActive(false);
        }
    }
}