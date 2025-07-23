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

        private bool _isRunning = false;
        public bool IsRunning => _isRunning;

        public MNMCoroutine(IEnumerator generator, MonoBehaviour owner)
        {
            this.generator = generator;
            this.owner = owner;
            _isRunning = false;
        }

        // Stop the coroutine form being called again
        public void Stop()
        {
            generator = null;
            _isRunning = false;
        }

        public void Start()
        {
            _isRunning = true;
            owner.StartCoroutine(this);
        }

        // IEnumerator.MoveNext
        public bool MoveNext()
        {
            if (generator != null)
            {
                var hasNext = generator.MoveNext();
                if (!hasNext)
                {
                    _isRunning = false;
                }
                return hasNext;
            }
            else
            {
                _isRunning = false;
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