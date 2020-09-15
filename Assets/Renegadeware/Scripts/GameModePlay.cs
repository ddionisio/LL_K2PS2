using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LoLExt;

namespace Renegadeware.K2PS2 {
    public class GameModePlay : GameModeController<GameModePlay> {
        [Header("Data")]
        public GamePlayData data;

        [Header("HUD")]
        [M8.TagSelector]
        public string HUDTag;

        [Header("Signals")]
        public M8.Signal signalListenVictory;

        private HUDGame mHUD;

        protected override void OnInstanceInit() {
            base.OnInstanceInit();

            //initialize Data
            data.InitItemPools();

            //initialize HUD
            var hudGO = GameObject.FindGameObjectWithTag(HUDTag);
            if(hudGO)
                mHUD = hudGO.GetComponent<HUDGame>();

            mHUD.Init(data);
        }

        protected override void OnInstanceDeinit() {
            if(mHUD)
                mHUD.Deinit();

            //clean up Data
            data.DeinitItemPools();
        }

        protected override IEnumerator Start() {
            yield return base.Start();

            //show HUD

            //wait for victory

            //victory
        }
    }
}