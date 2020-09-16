using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware.K2PS2 {
    public class HUDGame : MonoBehaviour {
        [Header("Display")]
        public GameObject displayRootGO;

        [Header("Drag")]
        public GameObject dragRootGO;

        private GamePlayData mData;

        public void Init(GamePlayData data) {
            mData = data;

            //initialize widgets
            if(displayRootGO) displayRootGO.SetActive(true);
            if(dragRootGO) dragRootGO.SetActive(false);

            //setup signals
        }

        public void Deinit() {
            //clear signals

            //clear out widgets

            if(displayRootGO) displayRootGO.SetActive(false);
            if(dragRootGO) dragRootGO.SetActive(false);

            mData = null;
        }

        void Awake() {
            //initial setup
            if(displayRootGO) displayRootGO.SetActive(false);
            if(dragRootGO) dragRootGO.SetActive(false);
        }
    }
}