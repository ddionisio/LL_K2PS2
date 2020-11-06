using LoLExt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware.K2PS2 {
    public class ModalVictory : M8.ModalController, M8.IModalPush, M8.IModalPop {
        public const string parmRetryCount = "r";
        public const string parmHintCount = "h";

        [Header("Display")]
        public M8.TextMeshPro.TextMeshProCounter levelScoreCounter;
        public M8.TextMeshPro.TextMeshProCounter retryPenaltyCounter;
        public M8.TextMeshPro.TextMeshProCounter hintPenaltyCounter;
        public M8.TextMeshPro.TextMeshProCounter scoreCounter;
        public M8.TextMeshPro.TextMeshProCounter totalScoreCounter;

        [Header("SFX")]
        [M8.SoundPlaylist]
        public string sfxVictory;

        public void End() {
            Close();

            GameData.instance.Progress();
        }

        void M8.IModalPop.Pop() {

        }

        void M8.IModalPush.Push(M8.GenericParams parms) {

            M8.SoundPlaylist.instance.Play(sfxVictory, false);

            int retryCount = 0;
            int hintCount = 0;

            if(parms != null) {
                if(parms.ContainsKey(parmRetryCount))
                    retryCount = parms.GetValue<int>(parmRetryCount);

                if(parms.ContainsKey(parmHintCount))
                    hintCount = parms.GetValue<int>(parmHintCount);
            }

            var gameDat = GameData.instance;

            //compute score/penalty
            int levelScore = gameDat.scoreLevelComplete;
            int retryPenalty = -Mathf.Min(gameDat.scoreEditCountPenalty * retryCount, gameDat.scoreEditCountPenaltyLimit);
            int hintPenalty = hintCount > 0 ? -gameDat.scoreHintPenalty : 0;

            int score = Mathf.Max(0, levelScore + retryPenalty + hintPenalty);

            int curTotalScore = LoLManager.instance.curScore;
            int newTotalScore = curTotalScore + score;

            //apply new total score
            LoLManager.instance.curScore = newTotalScore;

            //set display
            levelScoreCounter.SetCountImmediate(0);
            levelScoreCounter.count = levelScore;

            retryPenaltyCounter.SetCountImmediate(0);
            retryPenaltyCounter.count = retryPenalty;

            hintPenaltyCounter.SetCountImmediate(0);
            hintPenaltyCounter.count = hintPenalty;

            scoreCounter.SetCountImmediate(0);
            scoreCounter.count = score;

            totalScoreCounter.SetCountImmediate(curTotalScore);
            totalScoreCounter.count = newTotalScore;
        }
    }
}