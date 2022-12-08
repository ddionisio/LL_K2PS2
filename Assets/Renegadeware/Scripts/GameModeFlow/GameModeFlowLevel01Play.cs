using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LoLExt;

namespace Renegadeware.K2PS2 {
    public class GameModeFlowLevel01Play : GameModeFlow {
        [Header("Intro")]
        public ModalDialogController introDialog;

        //Drag/drop instruction
        [Header("Drag Drop Instruction")]
        public AnimatorEnterExit dragDropInstructionAnimate;
        public Transform dragDropWorldPoint; //use for play
        public MaterialObjectData dragDropMatObj; //use for play

        //Palette switch instruction
        [Header("Palette Switch")]
        public AnimatorEnterExit paletteSwitchInstructionAnimate;
        public int paletteSwitchIndex;

        private bool mIsDragGuideShown;
        private Vector2 mDragGuideStartPos;
        private Vector2 mDragGuideEndPos;

        public override IEnumerator Intro() {
            yield return introDialog.PlayWait();
        }

        public override IEnumerator SectionBegin(int index) {
            if(index != 0)
                yield break;

            var gameMode = GameModePlay.instance;
            var hud = gameMode.HUD;

            //show palette switch tutorial
            while(hud.isBusy)
                yield return null;

            //block other interacts
            if(hud.paletteBlockerGO)
                hud.paletteBlockerGO.SetActive(true);

            if(hud.playButton)
                hud.playButton.interactable = false;

            //show instructions
            if(hud.paletteWidgets[paletteSwitchIndex].clickInstructGO)
                hud.paletteWidgets[paletteSwitchIndex].clickInstructGO.SetActive(true);

            paletteSwitchInstructionAnimate.gameObject.SetActive(true);
            paletteSwitchInstructionAnimate.PlayEnter();

            //wait for palette switch
            while(hud.paletteActiveIndex != paletteSwitchIndex)
                yield return null;

            //hide instructions
            yield return paletteSwitchInstructionAnimate.PlayExitWait();

            if(hud.paletteWidgets[paletteSwitchIndex].clickInstructGO)
                hud.paletteWidgets[paletteSwitchIndex].clickInstructGO.SetActive(false);

            //unblock other interacts
            if(hud.paletteBlockerGO)
                hud.paletteBlockerGO.SetActive(false);
            //

            //show drag drop tutorial
            //block other interacts
            for(int i = 0; i < hud.paletteWidgets.Length; i++)
                hud.paletteWidgets[i].tagWidget.button.interactable = false;

            //show instruction
            dragDropInstructionAnimate.gameObject.SetActive(true);
            dragDropInstructionAnimate.PlayEnter();

            dragDropWorldPoint.gameObject.SetActive(true);

            var dragDropGuide = GameData.instance.GetDragGuide();

            var itmWidget = hud.GetMaterialObjectWidget(dragDropMatObj);

            var cam = Camera.main;

            mDragGuideStartPos = itmWidget.transform.position;
            mDragGuideEndPos = cam.WorldToScreenPoint(dragDropWorldPoint.position);

            dragDropGuide.Show(false, mDragGuideStartPos, mDragGuideEndPos);

            mIsDragGuideShown = true;

            //wait for an object placed
            while(!gameMode.data.IsAnyPlaced(dragDropMatObj))
                yield return null;

            //hide instruction
            mIsDragGuideShown = false;
            GameData.instance.GetDragGuide().Hide();

            dragDropWorldPoint.gameObject.SetActive(false);

            dragDropInstructionAnimate.PlayExit();

            //unblock other interacts
            if(hud.playButton)
                hud.playButton.interactable = true;

            for(int i = 0; i < hud.paletteWidgets.Length; i++)
                hud.paletteWidgets[i].tagWidget.button.interactable = true;
            //

            //show play instruction
            while(hud.isBusy)
                yield return null;

            if(hud.playInstructionGO)
                hud.playInstructionGO.SetActive(true);
        }

        public override IEnumerator SectionEnd(int index) {
            yield return null;
        }

        public override IEnumerator Outro() {
            yield return null;
        }

        void OnDragBegin() {
            if(mIsDragGuideShown)
                GameData.instance.GetDragGuide().Hide();
        }

        void OnDragEnd() {
            if(mIsDragGuideShown)
                GameData.instance.GetDragGuide().Show(false, mDragGuideStartPos, mDragGuideEndPos);
        }

        void OnDestroy() {
            var gameDat = GameData.instance;
            gameDat.signalDragBegin.callback -= OnDragBegin;
            gameDat.signalDragEnd.callback -= OnDragEnd;
        }

        void Awake() {
            dragDropInstructionAnimate.gameObject.SetActive(false);
            dragDropWorldPoint.gameObject.SetActive(false);
            paletteSwitchInstructionAnimate.gameObject.SetActive(false);

            var gameDat = GameData.instance;
            gameDat.signalDragBegin.callback += OnDragBegin;
            gameDat.signalDragEnd.callback += OnDragEnd;
        }
    }
}