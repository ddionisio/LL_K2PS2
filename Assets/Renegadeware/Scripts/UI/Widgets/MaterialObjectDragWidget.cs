using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Renegadeware.K2PS2 {
    public class MaterialObjectDragWidget : MonoBehaviour {
        [Header("Display")]
        public TMP_Text titleText;
        public Image iconImage;

        public M8.UI.Graphics.ColorGroup colorGroup;

        public bool allowValid;

        public void SetValid(bool isValid) {
            if(!allowValid)
                return;

            if(isValid)
                colorGroup.Revert();
            else
                colorGroup.ApplyColor(GameData.instance.objectDragInvalidColor);
        }

        public void Setup(MaterialObjectData data) {
            if(titleText)
                titleText.text = data.label;

            if(iconImage)
                iconImage.sprite = data.icon;
        }

        void Awake() {
            colorGroup.Init();
        }
    }
}