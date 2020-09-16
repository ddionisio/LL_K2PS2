using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Renegadeware.K2PS2 {
    public class MaterialObjectDragWidget : MonoBehaviour {
        [Header("Display")]
        public TMP_Text titleText;

        public void Setup(MaterialObjectData data) {
            titleText.text = data.label;
        }
    }
}