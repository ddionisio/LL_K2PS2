using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LoLExt;

namespace Renegadeware.K2PS2 {
    public class GameModeClassify : GameModeController<GameModeClassify> {
        [System.Serializable]
        public class SpawnAnimation {
            public string tagName; //match tag

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

        [Header("Music")]
        [M8.MusicPlaylist]
        public string music;

        public HUDClassify HUD { get { return mHUD; } }

        private HUDClassify mHUD;

        private Transform[] mSpawnPoints;

        private bool mIsClassifyPressed;

        private GameModeFlow mFlow;

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

            if(spawnPointsRoot) {
                mSpawnPoints = new Transform[spawnPointsRoot.childCount];
                for(int i = 0; i < mSpawnPoints.Length; i++)
                    mSpawnPoints[i] = spawnPointsRoot.GetChild(i);
            }
            else
                mSpawnPoints = new Transform[0];

            mFlow = GetComponent<GameModeFlow>();

            gameDat.signalClassify.callback += OnClassify;
            gameDat.signalDragBegin.callback += OnDragBegin;
            gameDat.signalDragEnd.callback += OnDragEnd;
        }

        protected override void OnInstanceDeinit() {
            var gameDat = GameData.instance;

            gameDat.signalClassify.callback -= OnClassify;
            gameDat.signalDragBegin.callback -= OnDragBegin;
            gameDat.signalDragEnd.callback -= OnDragEnd;

            if(mHUD)
                mHUD.Deinit();

            data.DeinitItemPools();

            base.OnInstanceDeinit();
        }

        protected override IEnumerator Start() {
            yield return base.Start();

            M8.MusicPlaylist.instance.Play(music, true, false);

            //dialog, animation
            if(mFlow)
                yield return mFlow.Intro();

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

                if(mFlow)
                    yield return mFlow.SectionBegin(i);

                //wait for all objects to be placed
                int spawnCount;
                do {
                    yield return null;

                    spawnCount = 0;
                    for(int j = 0; j < data.items.Length; j++)
                        spawnCount += data.items[j].materialObject.spawnedCount;
                } while(spawnCount > 0);

                if(mFlow)
                    yield return mFlow.SectionEnd(i);

                //while(obj.state != MaterialObjectEntity.State.None)
                //yield return null;
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

            M8.SoundPlaylist.instance.Play(mHUD.sfxCorrect, false);

            mHUD.HideAll(false);
            while(mHUD.isBusy)
                yield return null;

            //dialog, etc.
            //dialog, animation
            if(mFlow)
                yield return mFlow.Outro();

            //show summary
            mClassifySummaryParam[ModalClassifySummary.parmLevelData] = data;
            M8.ModalManager.main.Open(GameData.instance.modalClassifySummary, mClassifySummaryParam);
        }

        void OnDragBegin() {
            M8.SceneManager.instance.Pause();
        }

        void OnDragEnd() {
            M8.SceneManager.instance.Resume();
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