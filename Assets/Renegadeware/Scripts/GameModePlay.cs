using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LoLExt;
using System.Data;

namespace Renegadeware.K2PS2 {
    public class GameModePlay : GameModeController<GameModePlay> {
        [Header("Data")]
        public LevelData data;

        [Header("Game")]
        public Transform sectionRoot;

        [Header("Music")]
        [M8.MusicPlaylist]
        public string music;

        [Header("Debug")]
        public int debugStartSectionInd;

        public HUDGame HUD { get { return mHUD; } }

        private HUDGame mHUD;

        private GamePlaySection[] mSections;
        private int mCurSectionInd;
        private int mNextSectionInd;
        
        private Transform mGameCamTrans;

        private GameModeFlow mFlow;

        private Coroutine mHintShowRout;

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
            for(int i = 0; i < mSections.Length; i++) {
                mSections[i] = sectionRoot.GetChild(i).GetComponent<GamePlaySection>();
                mSections[i].gameObject.SetActive(false);
                mSections[i].SetHintVisible(false);
            }

            int startSectionInd = 0;

#if UNITY_EDITOR
            startSectionInd = Mathf.Clamp(debugStartSectionInd, 0, mSections.Length - 1);
#endif

            mCurSectionInd = mNextSectionInd = startSectionInd;

            var startSection = mSections[mCurSectionInd];

            startSection.gameObject.SetActive(true);

            var cam = Camera.main;
            mGameCamTrans = cam.transform.parent ? cam.transform.parent : cam.transform;

            mGameCamTrans.position = startSection.cameraPoint.position;

            var playerGO = GameObject.FindGameObjectWithTag(gameDat.playerTag);
            var playerEnt = playerGO.GetComponent<PlayerEntity>();

            playerEnt.transform.position = startSection.playerStart.position;
            playerEnt.startPosition = startSection.playerStart.position;

            mFlow = GetComponent<GameModeFlow>();

            //setup signals
            gameDat.signalGoal.callback += OnGoal;
            gameDat.signalDragBegin.callback += OnDragBegin;
            gameDat.signalDragEnd.callback += OnDragEnd;
            gameDat.signalReset.callback += OnReset;
            gameDat.signalPlayerDeath.callback += OnPlayerDeath;
            gameDat.signalGamePlay.callback += OnGamePlay;
            gameDat.signalHint.callback += OnHintShow;
        }

        protected override void OnInstanceDeinit() {
            var gameDat = GameData.instance;

            gameDat.signalGoal.callback -= OnGoal;
            gameDat.signalDragBegin.callback -= OnDragBegin;
            gameDat.signalDragEnd.callback -= OnDragEnd;
            gameDat.signalReset.callback -= OnReset;
            gameDat.signalPlayerDeath.callback -= OnPlayerDeath;
            gameDat.signalGamePlay.callback -= OnGamePlay;
            gameDat.signalHint.callback -= OnHintShow;

            if(mHUD)
                mHUD.Deinit();

            //clean up Data
            data.DeinitItemPools();

            HintClearRout();

            base.OnInstanceDeinit();
        }

        protected override IEnumerator Start() {
            yield return base.Start();

            M8.MusicPlaylist.instance.Play(music, true, false);

            //spawn player
            GameData.instance.signalPlayerSpawn.Invoke();

            //intro
            if(mFlow)
                yield return mFlow.Intro();

            StartCoroutine(DoGamePlay());
        }

        void Update() {
            //update hud
            if(mHUD) {
                //update reset active
                bool isResetActive = false;

                if(mHUD.isPaletteActive && !mHUD.isBusy) {
                    //check if there is any spawned objects
                    if(data.placedCount > 0)
                        isResetActive = true;
                }

                mHUD.resetButton.interactable = isResetActive;
            }
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

        void OnPlayerDeath() {
            if(mHUD.stopGlowGO)
                mHUD.stopGlowGO.SetActive(true);
        }

        void OnGamePlay() {
            mSections[mCurSectionInd].SetHintVisible(false);
        }

        void OnHintShow() {
            mSections[mCurSectionInd].SetHintVisible(true);
        }

        IEnumerator DoGamePlay() {
            var gameDat = GameData.instance;

            //show HUD
            mHUD.Show();

            if(mFlow)
                yield return mFlow.SectionBegin(mCurSectionInd);

            //hint button delay
            HintShowButtonDelay();

            //wait for goal
            while(mCurSectionInd == mNextSectionInd)
                yield return null;

            //hide hints
            mSections[mCurSectionInd].SetHintVisible(false);
            mHUD.hintVisible = false;
            HintClearRout();

            if(mFlow)
                yield return mFlow.SectionEnd(mCurSectionInd);
            
            mHUD.Hide();

            //victory?
            if(mNextSectionInd >= mSections.Length) {
                if(mFlow)
                    yield return mFlow.Outro();

                M8.ModalManager.main.Open(gameDat.modalVictory);
            }
            else {
                yield return new WaitForSeconds(gameDat.goalDelay);

                var prevSection = mSections[mCurSectionInd];

                //move to next section
                mCurSectionInd = mNextSectionInd;

                var curSection = mSections[mCurSectionInd];

                curSection.gameObject.SetActive(true);

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

                if(prevSection)
                    prevSection.gameObject.SetActive(false);

                //wait for all objects to fully despawn (fail-safe)
                while(data.spawnedCount > 0)
                    yield return null;

                mHUD.RefreshCurrentPalette();

                StartCoroutine(DoGamePlay());
            }
        }

        IEnumerator DoHintButtonShow() {
            var delay = GameData.instance.hintShowDelay;
            float lastTime = Time.realtimeSinceStartup;
            while(Time.realtimeSinceStartup - lastTime < delay)
                yield return null;

            mHUD.hintVisible = true;

            mHintShowRout = null;
        }

        private void HintShowButtonDelay() {
            HintClearRout();
            mHintShowRout = StartCoroutine(DoHintButtonShow());
        }

        private void HintClearRout() {
            if(mHintShowRout != null) {
                StopCoroutine(mHintShowRout);
                mHintShowRout = null;
            }
        }
    }
}