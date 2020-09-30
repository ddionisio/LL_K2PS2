using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LoLExt;

namespace Renegadeware.K2PS2 {
    public class GameModeClassify : GameModeController<GameModeClassify> {
        [Header("Data")]
        public LevelData data;
        public bool shuffleObjects;

        [Header("Object Data")]
        public Transform holderRoot;

        private HUDClassify mHUD;

        private MaterialObjectEntity[] mObjectEnts;

        protected override void OnInstanceInit() {
            base.OnInstanceInit();

            var gameDat = GameData.instance;

            //initialize objects
            mObjectEnts = new MaterialObjectEntity[data.items.Length];

            for(int i = 0; i < mObjectEnts.Length; i++) {
                var itm = data.items[i];

                var entGO = Instantiate(itm.materialObject.template);
                var ent = entGO.GetComponent<MaterialObjectEntity>();

                ent.transform.SetParent(holderRoot, false);

                mObjectEnts[i] = ent;
            }

            //initialize HUD
            var hudGO = GameObject.FindGameObjectWithTag(gameDat.HUDGameTag);
            if(hudGO)
                mHUD = hudGO.GetComponent<HUDClassify>();

            mHUD.Init(data);
        }

        protected override void OnInstanceDeinit() {
            if(mHUD)
                mHUD.Deinit();
        }

        protected override IEnumerator Start() {
            yield return base.Start();


        }
    }
}