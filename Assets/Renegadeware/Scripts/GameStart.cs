using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Renegadeware.K2PS2 {
    public class GameStart : MonoBehaviour {
        [Header("Screen")]
        public GameObject loadingGO;
        public GameObject readyGO;

        [Header("Title")]
        [M8.Localize]
        public string titleRef;
        public TMP_Text titleText;

        [Header("Play")]
        public Button playButton;

        void Awake() {
            if(loadingGO) loadingGO.SetActive(false);
            if(readyGO) readyGO.SetActive(false);
                        
            //Setup Play
            if(playButton) playButton.onClick.AddListener(OnPlayClick);
        }

        IEnumerator Start() {
            while(M8.SceneManager.instance.isLoading)
                yield return null;

            //Loading
            if(loadingGO) loadingGO.SetActive(true);

            while(!LoLExt.LoLManager.instance.isReady)
                yield return null;

            if(loadingGO) loadingGO.SetActive(false);

            //Title/Ready

            //Setup Title
            if(titleText) titleText.text = M8.Localize.Get(titleRef);

            if(readyGO) readyGO.SetActive(true);
        }

        void OnPlayClick() {

        }
    }
}