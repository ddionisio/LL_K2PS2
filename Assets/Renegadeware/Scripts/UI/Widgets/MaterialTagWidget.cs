using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Renegadeware.K2PS2 {
    public class MaterialTagWidget : MonoBehaviour {
        [Header("Display")]
        public Image tagIcon;
        public TMP_Text tagText;

        public void Setup(MaterialTagData tag) {
            if(tagIcon) tagIcon.sprite = tag.icon;
            if(tagText) tagText.text = tag.label;
        }
    }
}