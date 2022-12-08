using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LoLExt;

namespace Renegadeware.K2PS2 {
    public class GameModeFlowLevel02 : GameModeFlow {
        [Header("Flow")]
        public ModalDialogController introDialog;

        public AnimatorEnterExit lightTitle;
        public ModalDialogController lightDialog;

        public AnimatorEnterExit heavyTitle;
        public ModalDialogController heavyDialog;

        public ModalDialogController beginDialog;

        [Header("Material Objects")]
        public MaterialObjectData lightObject;
        public MaterialObjectData heavyObject;

        [Header("Block")]
        public Transform blockRoot;

        [Header("Weight Tags")]
        public M8.UI.Transforms.AttachTo weightTagLight;
        public M8.UI.Transforms.AttachTo weightTagHeavy;
        public M8.UI.Transforms.AttachTo weightTagBlock;

        public override IEnumerator Intro() {
            yield return introDialog.PlayWait();

            var gameMode = GameModeClassify.instance;

            Transform spawnPt;

            //spawn light object
            spawnPt = gameMode.GetSpawnPoint(lightObject);
            var lightEnt = lightObject.Spawn(spawnPt.position, MaterialObjectEntity.State.Spawning, null);

            while(lightEnt.state == MaterialObjectEntity.State.Spawning)
                yield return null;

            yield return new WaitForSeconds(1f);

            lightTitle.gameObject.SetActive(true);
            yield return lightTitle.PlayEnterWait();

            weightTagLight.target = lightEnt.transform;
            weightTagLight.gameObject.SetActive(true);

            weightTagBlock.target = blockRoot;
            weightTagBlock.gameObject.SetActive(true);

            yield return lightDialog.PlayWait();

            //despawn light object
            weightTagLight.gameObject.SetActive(false);

            lightEnt.state = MaterialObjectEntity.State.Despawning;
            while(lightEnt.state == MaterialObjectEntity.State.Despawning)
                yield return null;

            yield return lightTitle.PlayExitWait();

            //spawn heavy object
            spawnPt = gameMode.GetSpawnPoint(heavyObject);
            var heavyEnt = heavyObject.Spawn(spawnPt.position, MaterialObjectEntity.State.Spawning, null);

            while(heavyEnt.state == MaterialObjectEntity.State.Spawning)
                yield return null;

            yield return new WaitForSeconds(1f);

            heavyTitle.gameObject.SetActive(true);
            yield return heavyTitle.PlayEnterWait();

            weightTagHeavy.target = heavyEnt.transform;
            weightTagHeavy.gameObject.SetActive(true);

            yield return heavyDialog.PlayWait();

            //despawn heavy object
            weightTagHeavy.gameObject.SetActive(false);
            weightTagBlock.gameObject.SetActive(false);

            heavyEnt.state = MaterialObjectEntity.State.Despawning;
            while(heavyEnt.state == MaterialObjectEntity.State.Despawning)
                yield return null;

            yield return heavyTitle.PlayExitWait();

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
            lightTitle.gameObject.SetActive(false);
            heavyTitle.gameObject.SetActive(false);

            weightTagLight.gameObject.SetActive(false);
            weightTagHeavy.gameObject.SetActive(false);
            weightTagBlock.gameObject.SetActive(false);
        }
    }
}