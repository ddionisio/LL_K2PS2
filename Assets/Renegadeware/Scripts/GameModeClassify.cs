using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LoLExt;

namespace Renegadeware.K2PS2 {
    public class GameModeClassify : GameModeController<GameModeClassify> {
        [Header("Data")]
        public LevelData data;

        protected override void OnInstanceInit() {
            base.OnInstanceInit();
        }

        protected override void OnInstanceDeinit() {
        }

        protected override IEnumerator Start() {
            yield return base.Start();
        }
    }
}