using System;
using System.Threading;

namespace Vrh.EventHub.Core
{
    /// <summary>
    /// Egy Call hívással elküldött request válaszára való várakozást reprezentáló objektum
    /// </summary>
    internal class RegisteredCallWait
    {
        /// <summary>
        /// Azonosító, amely az adott várakozáshoz köti
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Ide kell elhelyezni a kapott választ (A semafor signálása előtt!)
        /// </summary>
        public object Response { get; set; }

        /// <summary>
        /// Semafor, melyet signálni kell hogy a küldő processz várakozása megszakadjon, és kivegye majd visszadja a kapott választ
        /// </summary>
        public SemaphoreSlim WaitResponseSemaforSlim { get; } = new SemaphoreSlim(0);
    }
}
