using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware.K2PS2 {
    public class HUDClassify : MonoBehaviour {
        [Header("Display")]
        public GameObject displayRootGO;
        public GameObject dragRootGO;
        public GameObject placementRootGO;

        [Header("Tags")]
        public MaterialTagClassifyWidget[] tagWidgets;

        private LevelData mData;

        public void Init(LevelData data) {
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
        }

        public void Deinit() {
            for(int i = 0; i < tagWidgets.Length; i++)
                tagWidgets[i].gameObject.SetActive(false);

            HideAll();

            mData = null;
        }

        public void Show() {

        }

        public void Hide() {

        }

        void Awake() {
            HideAll();
        }

        private void HideAll() {
            if(displayRootGO) displayRootGO.SetActive(false);
            if(dragRootGO) dragRootGO.SetActive(false);
            if(placementRootGO) placementRootGO.SetActive(false);
        }
    }
}