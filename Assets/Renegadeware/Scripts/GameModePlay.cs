using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LoLExt;

namespace Renegadeware.K2PS2 {
    public class GameModePlay : GameModeController<GameModePlay> {
        [System.Serializable]
        public struct Section {
            public Transform playerGoto;
            public Transform cameraGoto;
        }

        [Header("Data")]
        public GamePlayData data;

        [Header("HUD")]
        [M8.TagSelector]
        public string HUDTag;

        [Header("Game")]
        public Section[] nextSections;

        private HUDGame mHUD;

        private int mGoalCount;
        private int mGoalMaxCount;

        private int mNextSectionInd;

        private Transform mGameCamTrans;

        protected override void OnInstanceInit() {
            base.OnInstanceInit();

            //initialize Data
            data.InitItemPools();

            //initialize HUD
            var hudGO = GameObject.FindGameObjectWithTag(HUDTag);
            if(hudGO)
                mHUD = hudGO.GetComponent<HUDGame>();

            mHUD.Init(data);

            //initialize game
            mGoalCount = 0;

            var goalGOs = GameObject.FindGameObjectsWithTag(GameData.instance.goalTag);
            mGoalMaxCount = goalGOs.Length;

            mNextSectionInd = 0;

            var cam = Camera.main;
            mGameCamTrans = cam.transform.parent ? cam.transform.parent : cam.transform;

            //setup signals
            GameData.instance.signalGoal.callback += OnGoal;
        }

        protected override void OnInstanceDeinit() {
            GameData.instance.signalGoal.callback -= OnGoal;

            if(mHUD)
                mHUD.Deinit();

            //clean up Data
            data.DeinitItemPools();
        }

        protected override IEnumerator Start() {
            yield return base.Start();

            //spawn player
            GameData.instance.signalPlayerSpawn.Invoke();

            StartCoroutine(DoGamePlay());
        }

        void OnGoal() {
            mGoalCount++;
        }

        IEnumerator DoGamePlay() {
            var gameDat = GameData.instance;

            yield return null;

            //show HUD
            if(mHUD)
                mHUD.Show();

            //wait for goal
            var curGoalCount = mGoalCount;
            while(curGoalCount == mGoalCount)
                yield return null;

            if(mHUD)
                mHUD.Hide();

            yield return new WaitForSeconds(gameDat.goalDelay);

            //victory?
            if(mGoalCount >= mGoalMaxCount) {
                M8.ModalManager.main.Open(GameData.instance.modalVictory);
            }
            else {
                //move to next section
                if(mNextSectionInd < nextSections.Length) {
                    var items = data.items;

                    //despawn all objects
                    for(int i = 0; i < items.Length; i++)
                        items[i].materialObject.DespawnAll();

                    var nextSection = nextSections[mNextSectionInd];

                    //move player
                    gameDat.signalPlayerMoveTo.Invoke(nextSection.playerGoto.position);

                    //move camera
                    var camEaseFunc = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(gameDat.cameraMoveTween);
                    Vector2 camStartPos = mGameCamTrans.position;
                    Vector2 camEndPos = nextSection.cameraGoto.position;

                    var delay = gameDat.cameraMoveDelay;
                    var curTime = 0f;

                    while(curTime < delay) {
                        yield return null;

                        curTime += Time.deltaTime;

                        var t = camEaseFunc(curTime, delay, 0f, 0f);

                        mGameCamTrans.position = Vector2.Lerp(camStartPos, camEndPos, t);
                    }

                    //wait for all objects to fully despawn (fail-safe)
                    while(true) {
                        int curObjectCount = 0;
                        for(int i = 0; i < items.Length; i++)
                            curObjectCount += items[i].materialObject.spawnedCount;

                        if(curObjectCount == 0)
                            break;

                        yield return null;
                    }

                    mHUD.RefreshCurrentPalette();

                    mNextSectionInd++;
                }

                StartCoroutine(DoGamePlay());
            }
        }
    }
}