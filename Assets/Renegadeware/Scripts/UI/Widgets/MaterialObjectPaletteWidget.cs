using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Renegadeware.K2PS2 {
    public class MaterialObjectPaletteWidget : MonoBehaviour {
        [Header("Tag Display")]
        public Image tagIcon;
        public TMP_Text tagText;

        [Header("Item Display")]
        public MaterialObjectWidget[] itemWidgets;

        public void Setup(GamePlayData data, MaterialTagData tag) {

        }


    }
}