using UnityEngine;

namespace MelenitasDev.SoundsGood.Demo
{
    public class SG_PlayerInteract : MonoBehaviour
    {
        // ----- Serialized Fields
        [Header("Raycast")]
        [SerializeField] private Transform rayOrigin;
        [SerializeField] private float interactDistance = 3f;

        [Header("Input")]
        [SerializeField] private KeyCode interactKey = KeyCode.E;

        // ----- Fields
        private IInteractive currentInteractive;
        private RaycastHit lastHit;

        // ----- Unity Events
        void Awake ()
        {
            if (rayOrigin == null && Camera.main != null)
            {
                rayOrigin = Camera.main.transform;
            }
        }

        void Update ()
        {
            UpdateInteractiveTarget();
            HandleInteractionInput();
        }

        // ----- Private Methods
        private void UpdateInteractiveTarget ()
        {
            currentInteractive = null;

            if (rayOrigin == null)
            {
                return;
            }

            Vector3 origin = rayOrigin.position;
            Vector3 direction = rayOrigin.forward;

            if (Physics.Raycast(origin, direction, out lastHit, interactDistance))
            {
                currentInteractive = lastHit.collider.GetComponentInParent<IInteractive>();
            }
        }

        private void HandleInteractionInput ()
        {
            if (currentInteractive == null)
            {
                return;
            }

            if (Input.GetKeyDown(interactKey))
            {
                currentInteractive.Interact();
            }
        }
    }
}