using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware.K2PS2 {
    public class HUDClassify : MonoBehaviour {
        [Header("Display")]
        public GameObject displayRootGO;
        public GameObject tagsRootGO;
        public GameObject classifyGO;
        public GameObject dragRootGO;
        public GameObject placementRootGO;        

        [Header("Tags")]
        public MaterialTagClassifyWidget[] tagWidgets;

        public bool isBusy { get { return mRout != null; } }

        public MaterialTagClassifyWidget attachedTag { get; private set; }

        private LevelData mData;

        private Coroutine mRout;

        public void Init(LevelData data) {
            var gameDat = GameData.instance;

            mData = data;

            //initialize tags
            int tagCount = Mathf.Min(data.tags.Length, tagWidgets.Length);

            for(int i = 0; i < tagCount; i++) {
                var tag = tagWidgets[i];

                tag.gameObject.SetActive(true);
                tag.Setup(data.tags[i], dragRootGO.transform, placementRootGO.transform);
            }

            //hide excess tags
            for(int i = tagCount; i < tagWidgets.Length; i++)
                tagWidgets[i].gameObject.SetActive(false);

            gameDat.signalDragBegin.callback += OnDragBegin;
            gameDat.signalDragEnd.callback += OnDragEnd;
            gameDat.signalClassify.callback += OnClassify;
        }

        public void Deinit() {
            var gameDat = GameData.instance;

            gameDat.signalDragBegin.callback -= OnDragBegin;
            gameDat.signalDragEnd.callback -= OnDragEnd;
            gameDat.signalClassify.callback -= OnClassify;

            ClearRout();

            DetachTag();

            for(int i = 0; i < tagWidgets.Length; i++)
                tagWidgets[i].gameObject.SetActive(false);

            HideAll();

            mData = null;
        }

        public void Show() {
            //animation

            if(displayRootGO) displayRootGO.SetActive(true);
        }

        public void Hide() {
            //animation

            HideAll();
        }

        public void ShowTags() {
            if(tagsRootGO) tagsRootGO.SetActive(true);

            //animation
        }

        public void HideTags() {
            //animation

            if(tagsRootGO) tagsRootGO.SetActive(false);
        }

        public void DetachTag() {
            if(attachedTag) {
                attachedTag.Detach();
                attachedTag = null;
            }
        }

        void Awake() {
            HideAll();
        }

        void OnDragBegin() {
            if(dragRootGO) dragRootGO.SetActive(true);
        }

        void OnDragEnd() {
            if(dragRootGO) dragRootGO.SetActive(false);

            MaterialTagClassifyWidget newAttachedTag = null;
            for(int i = 0; i < tagWidgets.Length; i++) {
                if(tagWidgets[i].isAttached) {
                    newAttachedTag = tagWidgets[i];
                    break;
                }
            }

            //check if first time attached
            if(!attachedTag) {
                //show classify
                ShowClassify();
            }
            else if(attachedTag != newAttachedTag) {
                //check if attachment changed
                attachedTag.Detach();
            }

            attachedTag = newAttachedTag;
        }

        void OnClassify() {
            HideClassify();
        }

        private void ShowClassify() {
            if(classifyGO) classifyGO.SetActive(true);
            if(placementRootGO) placementRootGO.SetActive(true);

            //show animation
        }

        private void HideClassify() {
            //hide animation

            if(classifyGO) classifyGO.SetActive(false);
            if(placementRootGO) placementRootGO.SetActive(false);
        }

        private void HideAll() {
            if(displayRootGO) displayRootGO.SetActive(false);
            if(tagsRootGO) tagsRootGO.SetActive(false);
            if(classifyGO) classifyGO.SetActive(false);
            if(dragRootGO) dragRootGO.SetActive(false);
            if(placementRootGO) placementRootGO.SetActive(false);            
        }

        private void ClearRout() {
            if(mRout != null) {
                StopCoroutine(mRout);
                mRout = null;
            }
        }
    }
}