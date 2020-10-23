using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LoLExt;

namespace Renegadeware.K2PS2 {
    public class GameModeFlowLevel04 : GameModeFlow {
        public ModalDialogController introDialog;

        public AnimatorEnterExit conductIllustrate;
        public ModalDialogController conductDialog;

        public AnimatorEnterExit nonConductIllustrate;
        public ModalDialogController nonConductDialog;

        public ModalDialogController beginDialog;

        public override IEnumerator Intro() {
            yield return introDialog.PlayWait();

            conductIllustrate.gameObject.SetActive(true);
            yield return conductIllustrate.PlayEnterWait();

            yield return conductDialog.PlayWait();

            yield return conductIllustrate.PlayExitWait();

            nonConductIllustrate.gameObject.SetActive(true);
            yield return nonConductIllustrate.PlayEnterWait();

            yield return nonConductDialog.PlayWait();

            yield return nonConductIllustrate.PlayExitWait();

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
            conductIllustrate.gameObject.SetActive(false);
            nonConductIllustrate.gameObject.SetActive(false);
        }
    }
}