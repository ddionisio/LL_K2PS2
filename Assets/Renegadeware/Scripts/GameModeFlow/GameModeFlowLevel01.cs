using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LoLExt;

namespace Renegadeware.K2PS2 {
    public class GameModeFlowLevel01 : GameModeFlow {
        public ModalDialogController introDialog;

        public AnimatorEnterExit propertyIllustrate;
        public ModalDialogController propertyDialog;

        public AnimatorEnterExit classifyIllustrate;
        public ModalDialogController classifyDialog;

        public AnimatorEnterExit shapeSizeIllustrate;
        public ModalDialogController shapeSizeDialog;

        public ModalDialogController beginDialog;

        public AnimatorEnterExit dragDropInstructionAnimate;

        public override IEnumerator Intro() {
            yield return introDialog.PlayWait();

            //property
            propertyIllustrate.gameObject.SetActive(true);

            yield return propertyIllustrate.PlayEnterWait();

            yield return propertyDialog.PlayWait();

            yield return propertyIllustrate.PlayExitWait();

            //classify
            classifyIllustrate.gameObject.SetActive(true);

            yield return classifyIllustrate.PlayEnterWait();

            yield return classifyDialog.PlayWait();

            yield return classifyIllustrate.PlayExitWait();

            //shape size
            shapeSizeIllustrate.gameObject.SetActive(true);

            yield return shapeSizeIllustrate.PlayEnterWait();

            yield return shapeSizeDialog.PlayWait();

            yield return shapeSizeIllustrate.PlayExitWait();


            yield return beginDialog.PlayWait();
        }

        public override IEnumerator SectionBegin(int index) {
            //drag drop instruction (first object)
            if(index == 0) {
                var gameMode = GameModeClassify.instance;

                var obj = gameMode.data.items[index].materialObject.GetPlacedEntity();
                
                var palWidget = GameModeClassify.instance.HUD.GetPaletteMatch(obj.data);
                if(palWidget) { //fail-safe
                    dragDropInstructionAnimate.gameObject.SetActive(true);
                    dragDropInstructionAnimate.PlayEnter();

                    var cam = Camera.main;

                    Vector2 startPos = cam.WorldToScreenPoint(obj.position);
                    Vector2 endPos = palWidget.transform.position;

                    GameData.instance.GetDragGuide().Show(false, startPos, endPos);
                }

                yield return null;
            }
        }

        public override IEnumerator SectionEnd(int index) {
            if(index == 0) {
                //hide drag drop instruction
                GameData.instance.GetDragGuide().Hide();

                dragDropInstructionAnimate.PlayExit();

                yield return null;
            }
        }

        public override IEnumerator Outro() {
            yield return null;
        }

        void Awake() {
            propertyIllustrate.gameObject.SetActive(false);
            classifyIllustrate.gameObject.SetActive(false);
            shapeSizeIllustrate.gameObject.SetActive(false);

            dragDropInstructionAnimate.gameObject.SetActive(false);
        }
    }
}