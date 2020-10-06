using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware.K2PS2 {
    public class MaterialObjectDespawnTrigger : MonoBehaviour {
        [M8.TagSelector]
        public string tagFilter;

        void OnTriggerEnter2D(Collider2D collision) {
            if(collision.CompareTag(tagFilter)) {
                var objEnt = collision.GetComponent<MaterialObjectEntity>();
                if(objEnt && objEnt.state == MaterialObjectEntity.State.Normal)
                    objEnt.state = MaterialObjectEntity.State.Despawning;
            }
        }
    }
}