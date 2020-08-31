using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware.K2PS2 {
    public class GameStart : MonoBehaviour {
        [Header("Screen")]
        public GameObject loadingGO;
        public GameObject readyGO;

        [Header("Title")]
        [M8.Localize]
        public string titleRef;
        //public 

        IEnumerator Start() {
            while(M8.SceneManager.instance.isLoading)
                yield return null;

            //Loading
            while(!LoLExt.LoLManager.instance.isReady)
                yield return null;

            //Title/Ready
        }
    }
}