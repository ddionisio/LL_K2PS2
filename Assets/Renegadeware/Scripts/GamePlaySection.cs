using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware.K2PS2 {
    public class GamePlaySection : MonoBehaviour {
        public GameObject goalGO;
        public Transform playerStart;
        public Transform cameraPoint;
        public GameObject[] hintGOs;

        public void SetHintVisible(bool visible) {
            for(int i = 0; i < hintGOs.Length; i++) {
                if(hintGOs[i])
                    hintGOs[i].SetActive(visible);
            }
        }
    }
}