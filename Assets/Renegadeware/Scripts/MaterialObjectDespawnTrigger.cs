using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware.K2PS2 {
    public class MaterialObjectDespawnTrigger : MonoBehaviour {
        [M8.TagSelector]
        public string tagFilter;

        void OnTriggerEnter2D(Collider2D collision) {
            MaterialObjectEntity objEnt = null;

            if(collision.CompareTag(tagFilter))
                objEnt = collision.GetComponentInParent<MaterialObjectEntity>();

            if(objEnt) {
                if(objEnt && objEnt.state == MaterialObjectEntity.State.Normal)
                    objEnt.state = MaterialObjectEntity.State.Despawning;
            }
        }
    }
}