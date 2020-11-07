using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LoLExt;

namespace Renegadeware.K2PS2 {
    public class GameModeFlowLevel04 : GameModeFlow {
        [Header("Flow")]
        public ModalDialogController introDialog;

        public AnimatorEnterExit conductTitle;
        public ModalDialogController conductDialog;

        public AnimatorEnterExit nonConductTitle;
        public ModalDialogController nonConductDialog;

        public ModalDialogController beginDialog;

        [Header("Material Object")]
        public MaterialObjectData conductiveObject;
        public MaterialObjectData nonConductiveObject;

        public override IEnumerator Intro() {
            yield return introDialog.PlayWait();

            var gameMode = GameModeClassify.instance;

            Transform spawnPt;

            //spawn conductive
            spawnPt = gameMode.GetSpawnPoint(conductiveObject);
            var conductiveEnt = conductiveObject.Spawn(spawnPt.position, MaterialObjectEntity.State.Spawning, null);

            while(conductiveEnt.state == MaterialObjectEntity.State.Spawning)
                yield return null;

            yield return new WaitForSeconds(1f);

            conductTitle.gameObject.SetActive(true);
            yield return conductTitle.PlayEnterWait();

            yield return conductDialog.PlayWait();

            //despawn conductive
            conductiveEnt.state = MaterialObjectEntity.State.Despawning;

            while(conductiveEnt.state == MaterialObjectEntity.State.Despawning)
                yield return null;

            yield return conductTitle.PlayExitWait();
            conductTitle.gameObject.SetActive(false);

            //spawn non-conductive
            spawnPt = gameMode.GetSpawnPoint(nonConductiveObject);
            var nonConductiveEnt = nonConductiveObject.Spawn(spawnPt.position, MaterialObjectEntity.State.Spawning, null);

            while(nonConductiveEnt.state == MaterialObjectEntity.State.Spawning)
                yield return null;

            yield return new WaitForSeconds(1f);

            nonConductTitle.gameObject.SetActive(true);
            yield return nonConductTitle.PlayEnterWait();

            yield return nonConductDialog.PlayWait();

            //despawn non-conductive
            nonConductiveEnt.state = MaterialObjectEntity.State.Despawning;

            while(nonConductiveEnt.state == MaterialObjectEntity.State.Despawning)
                yield return null;

            yield return nonConductTitle.PlayExitWait();
            nonConductTitle.gameObject.SetActive(false);

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
            conductTitle.gameObject.SetActive(false);
            nonConductTitle.gameObject.SetActive(false);
        }
    }
}