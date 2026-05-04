using System;
using System.Collections.Generic;

namespace UncertainLuei.CaudexLib.Util
{
    public static class QueuedActions
    {
        private static readonly List<Action> queuedActions = [];

        internal static void RunQueuedActions()
        {
            foreach (Action action in queuedActions)
                action.Invoke();
            queuedActions.Clear();
        }

        public static void QueueAction(Action action)
            => queuedActions.Add(action);   
    }
}
