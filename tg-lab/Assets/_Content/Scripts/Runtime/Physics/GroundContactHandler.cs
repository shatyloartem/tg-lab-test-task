using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Physics
{
    public sealed class GroundContactHandler
    {
        private readonly HashSet<Collider> contacts = new();

        public bool IsGrounded => contacts.Count > 0;

        internal void Reset() => contacts.Clear();

        internal void Enter(Collision collision) => UpdateContact(collision);
        internal void Stay(Collision collision) => UpdateContact(collision);
        internal void Exit(Collision collision) => contacts.Remove(collision.collider);

        private void UpdateContact(Collision collision)
        {
            contacts.Remove(collision.collider);
            for (int i = 0; i < collision.contactCount; i++)
            {
                if (Vector3.Dot(collision.GetContact(i).normal, Vector3.up) <= 0.5f) continue;
                contacts.Add(collision.collider);
                return;
            }
        }
    }
}
