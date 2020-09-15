using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware.K2PS2 {
    public class HUDGame : MonoBehaviour {
        [Header("Display")]
        public GameObject displayRoot;

        [Header("Drag")]
        public GameObject dragRoot;

        private GamePlayData mData;

        public void Init(GamePlayData data) {
            mData = data;

            //initialize widgets
        }

        public void Deinit() {
            //clear out widgets

            mData = null;
        }

        void Awake() {
            //initial setup
            if(displayRoot) displayRoot.SetActive(false);

            if(dragRoot) dragRoot.SetActive(false);
        }
    }
}