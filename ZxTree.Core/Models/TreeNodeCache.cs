using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ZxTree.Core.Models
{
    internal static class TreeNodeCache
    {
        private static readonly IDictionary<Guid, WeakReference<TreeNodeBase>> InternalCache =
            new ConcurrentDictionary<Guid, WeakReference<TreeNodeBase>>();

        internal static void Cache(TreeNodeBase nodeBase)
        {
            InternalCache[nodeBase.Id] = new WeakReference<TreeNodeBase>(nodeBase);
        }

        internal static void Remove(Guid id)
        {
            InternalCache.Remove(id);
        }

        internal static void Recycle()
        {
            var keys = InternalCache.Keys.ToList();
            foreach (var key in keys)
            {
                if (!InternalCache.TryGetValue(key, out var weakReference)) continue;
                if (weakReference.TryGetTarget(out _)) continue;
                InternalCache.Remove(key);
            }
        }

        /// <summary> 
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Can be null</returns>
        internal static TreeNodeBase Get(Guid id)
        {
            if (id == Guid.Empty) return null;
            if (!InternalCache.TryGetValue(id, out var weakReference)) return null;

            if (weakReference.TryGetTarget(out var node))
            {
                return node;
            }

            // remove from cache if the node is no longer available
            InternalCache.Remove(id);
            return null;
        }
    }
}