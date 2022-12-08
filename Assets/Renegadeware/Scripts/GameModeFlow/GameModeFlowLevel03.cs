using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LoLExt;

namespace Renegadeware.K2PS2 {
    public class GameModeFlowLevel03 : GameModeFlow {
        [Header("Flow")]
        public ModalDialogController introDialog;

        public AnimatorEnterExit buoyancyTitle;
        public ModalDialogController buoyancyDialog;

        public ModalDialogController beginDialog;

        [Header("Material Objects")]
        public MaterialObjectData floatObject;
        public Transform floatSpawnPt;

        public MaterialObjectData sinkObject;
        public Transform sinkSpawnPt;

        [Header("Tags")]
        public M8.UI.Transforms.AttachTo tagFloat;
        public M8.UI.Transforms.AttachTo tagSink;

        public override IEnumerator Intro() {
            yield return introDialog.PlayWait();

            buoyancyTitle.gameObject.SetActive(true);
            yield return buoyancyTitle.PlayEnterWait();

            //spawn objects
            var floatEnt = floatObject.Spawn(floatSpawnPt.position, MaterialObjectEntity.State.Spawning, null);
            var sinkEnt = sinkObject.Spawn(sinkSpawnPt.position, MaterialObjectEntity.State.Spawning, null);

            while(floatEnt.state == MaterialObjectEntity.State.Spawning || sinkEnt.state == MaterialObjectEntity.State.Spawning)
                yield return null;

            yield return new WaitForSeconds(1f);

            tagFloat.target = floatEnt.transform;
            tagFloat.gameObject.SetActive(true);

            tagSink.target = sinkEnt.transform;
            tagSink.gameObject.SetActive(true);

            yield return buoyancyDialog.PlayWait();

            tagFloat.gameObject.SetActive(false);
            tagSink.gameObject.SetActive(false);

            floatEnt.state = MaterialObjectEntity.State.Despawning;
            sinkEnt.state = MaterialObjectEntity.State.Despawning;

            while(floatEnt.state == MaterialObjectEntity.State.Despawning || sinkEnt.state == MaterialObjectEntity.State.Despawning)
                yield return null;

            yield return buoyancyTitle.PlayExitWait();
            buoyancyTitle.gameObject.SetActive(false);

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
            buoyancyTitle.gameObject.SetActive(false);

            tagFloat.gameObject.SetActive(false);
            tagSink.gameObject.SetActive(false);
        }
    }
}