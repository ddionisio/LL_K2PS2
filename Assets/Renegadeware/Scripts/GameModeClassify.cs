using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LoLExt;

namespace Renegadeware.K2PS2 {
    public class GameModeClassify : GameModeController<GameModeClassify> {
        [Header("Data")]
        public LevelData data;
        public bool shuffleObjects;

        [Header("Object Entity Data")]
        public Transform holderRoot;
        public Transform objectEntSpawnPt;
        public MaterialObjectWidget objectWidget;

        private HUDClassify mHUD;

        private MaterialObjectEntity[] mObjectEnts;

        private bool mIsClassifyPressed;

        private M8.GenericParams mObjParms = new M8.GenericParams();

        protected override void OnInstanceInit() {
            base.OnInstanceInit();

            var gameDat = GameData.instance;

            //initialize objects
            holderRoot.gameObject.SetActive(false);
            objectWidget.gameObject.SetActive(false);

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

            gameDat.signalClassify.callback += OnClassify;
        }

        protected override void OnInstanceDeinit() {
            var gameDat = GameData.instance;

            gameDat.signalClassify.callback -= OnClassify;

            if(mHUD)
                mHUD.Deinit();

            base.OnInstanceDeinit();
        }

        protected override IEnumerator Start() {
            yield return base.Start();

            //dialog, animation

            mHUD.Show();

            for(int i = 0; i < mObjectEnts.Length; i++) {
                var itm = data.items[i];
                var matObj = itm.materialObject;
                var objEnt = mObjectEnts[i];

                //show object widget and animate to be thrown towards spawn point
                objectWidget.Setup(matObj, null);
                objectWidget.gameObject.SetActive(true);

                yield return new WaitForSeconds(1f);

                objectWidget.gameObject.SetActive(false);

                //spawn object
                mObjParms[MaterialObjectEntity.parmData] = matObj;
                mObjParms[MaterialObjectEntity.parmState] = MaterialObjectEntity.State.Spawning;
                mObjParms[MaterialObjectEntity.parmIsNonPool] = true;

                objEnt.poolDataCtrl.Spawn(mObjParms);

                objEnt.transform.SetParent(null, false);
                objEnt.position = objectEntSpawnPt.position;
                //

                bool proceed = false;
                while(!proceed) {
                    mHUD.ShowTags();

                    //wait for classify pressed
                    mIsClassifyPressed = false;
                    while(!mIsClassifyPressed)
                        yield return null;

                    mHUD.HideTags();

                    //check if tag matched
                    var attachedTag = mHUD.attachedTag.data;
                    if(matObj.CompareTag(attachedTag)) {
                        //correct animation, fx

                        //dialog, etc.

                        proceed = true;
                    }
                    else {
                        //incorrect animation, fx

                        //dialog etc.

                        //clear attached tag
                        mHUD.DetachTag();
                    }

                    yield return null;
                }

                //despawn object
                objEnt.state = MaterialObjectEntity.State.Despawning;

                while(objEnt.state == MaterialObjectEntity.State.Despawning)
                    yield return null;
                //
            }

            mHUD.Hide();

            //show summary
        }

        void OnClassify() {
            mIsClassifyPressed = true;
        }
    }
}