using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LoLExt;

namespace Renegadeware.K2PS2 {
    public class GameModeFlowLevel03 : GameModeFlow {
        public ModalDialogController introDialog;

        public AnimatorEnterExit buoyancyIllustrate;
        public ModalDialogController buoyancyDialog;

        public ModalDialogController beginDialog;

        public override IEnumerator Intro() {
            yield return introDialog.PlayWait();

            buoyancyIllustrate.gameObject.SetActive(true);
            yield return buoyancyIllustrate.PlayEnterWait();

            yield return buoyancyDialog.PlayWait();

            yield return buoyancyIllustrate.PlayExitWait();

            yield return beginDialog.PlayWait();
        }

        public override IEnumerator SectionBegin(int index) {
            yield return null;
        }

        public override IEnumerator SectionEnd(int index) {
            yield return null;
        }

        public override IEnumerator Outro() {
            yield return null;
        }

        void Awake() {
            buoyancyIllustrate.gameObject.SetActive(false);
        }
    }
}