﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LoLExt;

namespace Renegadeware.K2PS2 {
    public class GameModeClassify : GameModeController<GameModeClassify> {
        [System.Serializable]
        public class SpawnAnimation {
            public string tagName { get { return animator ? animator.name : ""; } } //match tag

            public M8.Animator.Animate animator;
            public string takeEnter;
            public string takeExit;

            public IEnumerator PlayEnter() {
                if(animator && !string.IsNullOrEmpty(takeEnter))
                    yield return animator.PlayWait(takeEnter);
            }

            public IEnumerator PlayExit() {
                if(animator && !string.IsNullOrEmpty(takeExit))
                    yield return animator.PlayWait(takeExit);
            }
        }

        [Header("Data")]
        public LevelData data;

        public Transform spawnPointDefault;
        public Transform spawnPointsRoot; //use name as match for material object

        public SpawnAnimation[] spawnAnimations; //use for stagger spawn mode

        private HUDClassify mHUD;

        private Transform[] mSpawnPoints;

        private bool mIsClassifyPressed;

        private M8.GenericParams mClassifySummaryParam = new M8.GenericParams();

        protected override void OnInstanceInit() {
            base.OnInstanceInit();

            var gameDat = GameData.instance;

            //initialize objects
            data.InitItemPools(1);

            //initialize HUD
            var hudGO = GameObject.FindGameObjectWithTag(gameDat.HUDClassifyTag);
            if(hudGO)
                mHUD = hudGO.GetComponent<HUDClassify>();

            mHUD.Init(data);

            mSpawnPoints = new Transform[spawnPointsRoot.childCount];
            for(int i = 0; i < mSpawnPoints.Length; i++)
                mSpawnPoints[i] = spawnPointsRoot.GetChild(i);

            gameDat.signalClassify.callback += OnClassify;
        }

        protected override void OnInstanceDeinit() {
            var gameDat = GameData.instance;

            gameDat.signalClassify.callback -= OnClassify;

            if(mHUD)
                mHUD.Deinit();

            data.DeinitItemPools();

            base.OnInstanceDeinit();
        }

        protected override IEnumerator Start() {
            yield return base.Start();

            yield return null;

            //dialog, animation

            mHUD.ShowPalette();
            while(mHUD.isBusy)
                yield return null;

            var items = data.items;

            for(int i = 0; i < items.Length; i++) {
                var matObjData = items[i].materialObject;

                var spawnPt = GetSpawnPoint(matObjData);
                var spawnAnim = GetSpawnAnimation(matObjData);

                //animation enter
                if(spawnAnim != null)
                    yield return spawnAnim.PlayEnter();

                //spawn object
                var obj = matObjData.Spawn(spawnPt.position, MaterialObjectEntity.State.Spawning, mHUD.dragWidget);

                while(obj.state == MaterialObjectEntity.State.Spawning)
                    yield return null;

                //animation exit
                if(spawnAnim != null)
                    yield return spawnAnim.PlayExit();

                //wait for object to be placed in palette
                while(obj.state != MaterialObjectEntity.State.None)
                    yield return null;
            }

            bool isDone = false;

            while(!isDone) {
                //wait for classify press
                mIsClassifyPressed = false;
                while(!mIsClassifyPressed) {
                    while(mHUD.isBusy)
                        yield return null;

                    //show classify if everything is placed in palette, hide otherwise
                    int spawnCount = 0;
                    for(int i = 0; i < data.items.Length; i++)
                        spawnCount += data.items[i].materialObject.spawnedCount;

                    if(spawnCount == 0) {
                        if(!mHUD.isClassifyVisible)
                            mHUD.ShowClassify();
                    }
                    else {
                        if(mHUD.isClassifyVisible)
                            mHUD.HideClassify();
                    }

                    yield return null;
                }

                //check if all palettes have matched objects
                if(mHUD.errorCount == 0)
                    break;

                yield return null;
            }

            mHUD.HideAll(false);
            while(mHUD.isBusy)
                yield return null;

            //dialog, etc.

            //show summary
            mClassifySummaryParam[ModalClassifySummary.parmLevelData] = data;
            M8.ModalManager.main.Open(GameData.instance.modalClassifySummary, mClassifySummaryParam);
        }

        void OnClassify() {
            mIsClassifyPressed = true;
        }

        private Transform GetSpawnPoint(MaterialObjectData dat) {
            for(int i = 0; i < mSpawnPoints.Length; i++) {
                var spawnPt = mSpawnPoints[i];
                if(spawnPt.name == dat.name)
                    return spawnPt;
            }

            return spawnPointDefault;
        }

        private SpawnAnimation GetSpawnAnimation(MaterialObjectData dat) {
            for(int i = 0; i < spawnAnimations.Length; i++) {
                var spawnAnim = spawnAnimations[i];
                if(dat.CompareTag(spawnAnim.tagName))
                    return spawnAnim;
            }

            return null;
        }
    }
}