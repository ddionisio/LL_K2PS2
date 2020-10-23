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

        [Header("Animation")]
        public M8.Animator.Animate animator;
        [M8.Animator.TakeSelector(animatorField = "animator")]
        public string takeEnter;

        public Button button {
            get {
                if(!mButton)
                    mButton = GetComponent<Button>();
                return mButton;
            }
        }

        private Button mButton;

        public void Setup(MaterialTagData tag) {
            if(tagIcon) tagIcon.sprite = tag.icon;
            if(tagText) tagText.text = tag.label;
        }

        public IEnumerator PlayEnterWait() {
            if(animator && !string.IsNullOrEmpty(takeEnter))
                yield return animator.PlayWait(takeEnter);
        }
    }
}