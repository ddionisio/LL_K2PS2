using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LoLExt;

namespace Renegadeware.K2PS2 {
    public class GameModePlay : GameModeController<GameModePlay> {
        [Header("Data")]
        public LevelData data;

        [Header("Game")]
        public Transform sectionRoot;

        [Header("Debug")]
        public int debugStartSectionInd;

        private HUDGame mHUD;

        private GamePlaySection[] mSections;
        private int mCurSectionInd;
        private int mNextSectionInd;
        
        private Transform mGameCamTrans;

        protected override void OnInstanceInit() {
            base.OnInstanceInit();

            var gameDat = GameData.instance;

            //initialize Data
            data.InitItemPools();

            //initialize HUD
            var hudGO = GameObject.FindGameObjectWithTag(gameDat.HUDGameTag);
            if(hudGO)
                mHUD = hudGO.GetComponent<HUDGame>();

            mHUD.Init(data);

            //initialize game
            mSections = new GamePlaySection[sectionRoot.childCount];
            for(int i = 0; i < mSections.Length; i++)
                mSections[i] = sectionRoot.GetChild(i).GetComponent<GamePlaySection>();

            int startSectionInd = 0;

#if UNITY_EDITOR
            startSectionInd = Mathf.Clamp(debugStartSectionInd, 0, mSections.Length - 1);

            for(int i = 0; i < startSectionInd; i++) {
                if(mSections[i])
                    mSections[i].gameObject.SetActive(false);
            }
#endif

            mCurSectionInd = mNextSectionInd = startSectionInd;

            var startSection = mSections[mCurSectionInd];

            var cam = Camera.main;
            mGameCamTrans = cam.transform.parent ? cam.transform.parent : cam.transform;

            mGameCamTrans.position = startSection.cameraPoint.position;

            var playerGO = GameObject.FindGameObjectWithTag(gameDat.playerTag);
            var playerEnt = playerGO.GetComponent<PlayerEntity>();

            playerEnt.transform.position = startSection.playerStart.position;
            playerEnt.startPosition = startSection.playerStart.position;

            //setup signals
            gameDat.signalGoal.callback += OnGoal;
            gameDat.signalDragBegin.callback += OnDragBegin;
            gameDat.signalDragEnd.callback += OnDragEnd;
            gameDat.signalReset.callback += OnReset;
        }

        protected override void OnInstanceDeinit() {
            var gameDat = GameData.instance;

            gameDat.signalGoal.callback -= OnGoal;
            gameDat.signalDragBegin.callback -= OnDragBegin;
            gameDat.signalDragEnd.callback -= OnDragEnd;
            gameDat.signalReset.callback -= OnReset;

            if(mHUD)
                mHUD.Deinit();

            //clean up Data
            data.DeinitItemPools();

            base.OnInstanceDeinit();
        }

        protected override IEnumerator Start() {
            yield return base.Start();

            //spawn player
            GameData.instance.signalPlayerSpawn.Invoke();

            StartCoroutine(DoGamePlay());
        }

        void OnGoal() {
            mNextSectionInd++;
        }

        void OnDragBegin() {
            M8.SceneManager.instance.Pause();
        }

        void OnDragEnd() {
            M8.SceneManager.instance.Resume();
        }

        void OnReset() {
            data.DespawnAll();
        }

        IEnumerator DoGamePlay() {
            var gameDat = GameData.instance;

            yield return null;

            //show HUD
            if(mHUD)
                mHUD.Show();

            //wait for goal
            while(mCurSectionInd == mNextSectionInd)
                yield return null;

            if(mHUD)
                mHUD.Hide();

            yield return new WaitForSeconds(gameDat.goalDelay);

            //victory?
            if(mNextSectionInd >= mSections.Length) {
                M8.ModalManager.main.Open(gameDat.modalVictory);
            }
            else {
                //move to next section
                mCurSectionInd = mNextSectionInd;

                var curSection = mSections[mCurSectionInd];

                var items = data.items;

                //despawn all objects
                for(int i = 0; i < items.Length; i++)
                    items[i].materialObject.DespawnAll();

                //move player
                gameDat.signalPlayerMoveTo.Invoke(curSection.playerStart.position);

                //move camera
                var camEaseFunc = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(gameDat.cameraMoveTween);
                Vector2 camStartPos = mGameCamTrans.position;
                Vector2 camEndPos = curSection.cameraPoint.position;

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

                StartCoroutine(DoGamePlay());
            }
        }
    }
}