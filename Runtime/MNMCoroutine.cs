using System;
using System.Collections;
using UnityEngine;

namespace Minimoo
{
    /// <summary>
    /// Wrapper around coroutines that allows to start them without using
    /// their lexical name while still being able to stop them.
    /// </summary>
    public class MNMCoroutine : IEnumerator
    {
        // Wrapped generator method
        protected IEnumerator generator;
        protected MonoBehaviour owner;

        public MNMCoroutine(IEnumerator generator, MonoBehaviour owner)
        {
            this.generator = generator;
            this.owner = owner;
        }

        // Stop the coroutine form being called again
        public void Stop()
        {
            generator = null;
        }

        public void Start()
        {
            owner.StartCoroutine(this);
        }

        public bool MoveNext()
        {
            if (generator != null)
            {
                return generator.MoveNext();
            }
            else
            {
                return false;
            }
        }

        // IEnumerator.Reset
        public void Reset()
        {
            if (generator != null)
            {
                generator.Reset();
            }
        }

        // IEnumerator.Current
        public object Current
        {
            get
            {
                if (generator != null)
                {
                    return generator.Current;
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }
}