using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LoLExt;

namespace Renegadeware.K2PS2 {
    public class GameModeFlowLevel02 : GameModeFlow {
        public ModalDialogController introDialog;

        public AnimatorEnterExit lightIllustrate;
        public ModalDialogController lightDialog;

        public AnimatorEnterExit heavyIllustrate;
        public ModalDialogController heavyDialog;

        public ModalDialogController beginDialog;

        public override IEnumerator Intro() {
            yield return introDialog.PlayWait();

            lightIllustrate.gameObject.SetActive(true);
            yield return lightIllustrate.PlayEnterWait();

            yield return lightDialog.PlayWait();

            yield return lightIllustrate.PlayExitWait();

            heavyIllustrate.gameObject.SetActive(true);
            yield return heavyIllustrate.PlayEnterWait();

            yield return heavyDialog.PlayWait();

            yield return heavyIllustrate.PlayExitWait();

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
            lightIllustrate.gameObject.SetActive(false);
            heavyIllustrate.gameObject.SetActive(false);
        }
    }
}