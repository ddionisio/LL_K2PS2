using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Renegadeware.K2PS2 {
    public class ModalClassifySummary : M8.ModalController, M8.IModalPush, M8.IModalPop {
        public const string parmLevelData = "ld";

        [Header("Material Object")]
        public GameObject materialObjectRootGO;

        public Image materialObjectIcon;
        public TMP_Text materialObjectTitle;

        public M8.Animator.Animate materialObjectAnimator;
        [M8.Animator.TakeSelector(animatorField = "materialObjectAnimator")]
        public string materialObjectTakeEnter;
        [M8.Animator.TakeSelector(animatorField = "materialObjectAnimator")]
        public string materialObjectTakeExit;

        [Header("Tags")]
        public MaterialTagWidget tagWidgetTemplate;
        public Transform tagRoot;

        [Header("Next")]
        public GameObject nextRootGO;

        public M8.Animator.Animate nextAnimator;
        [M8.Animator.TakeSelector(animatorField = "nextAnimator")]
        public string nextTakeEnter;
        [M8.Animator.TakeSelector(animatorField = "nextAnimator")]
        public string nextTakeExit;

        [Header("Finish")]
        public GameObject finishRootGO;

        public M8.Animator.Animate finishAnimator;
        [M8.Animator.TakeSelector(animatorField = "finishAnimator")]
        public string finishTakeEnter;
        [M8.Animator.TakeSelector(animatorField = "finishAnimator")]
        public string finishTakeExit;

        [Header("SFX")]
        [M8.SoundPlaylist]
        public string sfxNewTag;

        private const int tagCapacity = 3;

        private M8.CacheList<MaterialTagWidget> mTags = new M8.CacheList<MaterialTagWidget>(tagCapacity);
        private M8.CacheList<MaterialTagWidget> mTagCache = new M8.CacheList<MaterialTagWidget>(tagCapacity);

        private LevelData mLevelData;
        private int mCurMatObjInd;

        public void Next() {
            StartCoroutine(DoMaterialNext());
        }

        public void Finish() {
            GameData.instance.Progress();

            //exit material object
            if(materialObjectAnimator && !string.IsNullOrEmpty(materialObjectTakeExit))
                materialObjectAnimator.Play(materialObjectTakeExit);

            //exit finish
            if(finishAnimator && !string.IsNullOrEmpty(finishTakeExit))
                finishAnimator.Play(finishTakeExit);

            Close();
        }

        void M8.IModalPop.Pop() {

        }

        void M8.IModalPush.Push(M8.GenericParams parms) {
            mLevelData = null;

            if(parms != null) {
                if(parms.ContainsKey(parmLevelData))
                    mLevelData = parms.GetValue<LevelData>(parmLevelData);
            }

            mCurMatObjInd = 0;

            if(materialObjectRootGO) materialObjectRootGO.SetActive(false);
            if(nextRootGO) nextRootGO.SetActive(false);
            if(finishRootGO) finishRootGO.SetActive(false);

            if(mLevelData)
                StartCoroutine(DoMaterialCurrent());
        }

        void Awake() {
            for(int i = 0; i < tagCapacity; i++) {
                var tag = Instantiate(tagWidgetTemplate, tagRoot);
                tag.gameObject.SetActive(false);
                mTagCache.Add(tag);
            }

            tagWidgetTemplate.gameObject.SetActive(false);
        }

        IEnumerator DoMaterialCurrent() {
            var matObjDat = mLevelData.items[mCurMatObjInd].materialObject;

            //initialize material object info
            materialObjectIcon.sprite = matObjDat.icon;
            materialObjectTitle.text = matObjDat.label;

            //initialize material object tags, except last match
            ClearTags();

            int lastTagIndex = -1;

            for(int i = 0; i < matObjDat.tags.Length; i++) {
                var tag = matObjDat.tags[i];

                if(mLevelData.IsTagMatch(tag)) {
                    lastTagIndex = i;
                    break;
                }

                AddTag(tag);
            }

            //show panel
            if(materialObjectRootGO) materialObjectRootGO.SetActive(true);

            if(materialObjectAnimator && !string.IsNullOrEmpty(materialObjectTakeEnter))
                yield return materialObjectAnimator.PlayWait(materialObjectTakeEnter);


            //show last tag
            if(lastTagIndex != -1) {
                M8.SoundPlaylist.instance.Play(sfxNewTag, false);

                var tagWidget = AddTag(matObjDat.tags[lastTagIndex]);

                //play tag enter
                yield return tagWidget.PlayEnterWait();
            }
            
            if(mCurMatObjInd == mLevelData.items.Length - 1) {
                //show finish
                if(finishRootGO) finishRootGO.SetActive(true);

                if(finishAnimator && !string.IsNullOrEmpty(finishTakeEnter))
                    finishAnimator.Play(finishTakeEnter);
            }
            else {
                //show next
                if(nextRootGO) nextRootGO.SetActive(true);

                if(nextAnimator && !string.IsNullOrEmpty(nextTakeEnter))
                    nextAnimator.Play(nextTakeEnter);
            }
        }

        IEnumerator DoMaterialNext() {
            //hide next
            if(nextAnimator && !string.IsNullOrEmpty(nextTakeExit))
                nextAnimator.Play(nextTakeExit);

            //hide material object panel
            if(materialObjectAnimator && !string.IsNullOrEmpty(materialObjectTakeExit))
                materialObjectAnimator.Play(materialObjectTakeExit);

            //wait
            while((nextAnimator && nextAnimator.isPlaying) || (materialObjectAnimator && materialObjectAnimator.isPlaying))
                yield return null;

            if(materialObjectRootGO) materialObjectRootGO.SetActive(false);
            if(nextRootGO) nextRootGO.SetActive(false);

            mCurMatObjInd++;

            StartCoroutine(DoMaterialCurrent());
        }

        private MaterialTagWidget AddTag(MaterialTagData tagData) {
            if(mTagCache.Count == 0)
                return null;

            var newTag = mTagCache.RemoveLast();

            newTag.Setup(tagData);
            newTag.transform.SetAsLastSibling();
            newTag.gameObject.SetActive(true);

            mTags.Add(newTag);

            return newTag;
        }

        private void ClearTags() {
            for(int i = 0; i < mTags.Count; i++) {
                mTags[i].gameObject.SetActive(false);
                mTagCache.Add(mTags[i]);
            }

            mTags.Clear();
        }
    }
}