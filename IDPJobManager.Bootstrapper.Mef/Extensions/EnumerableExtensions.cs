using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;

namespace IDPJobManager.Bootstrapper.Mef.Extensions
{

    static class EnumerableExtensions
    {

        /// <summary>
        /// Prepends <paramref name="item"/> to the enumerable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> self, T item)
        {
            Debug.Assert(self != null);
            Debug.Assert(item != null);

            yield return item;
            foreach (var i in self)
                yield return i;
        }

        /// <summary>
        /// Recurses into the given object, obtaining a single child using the given function.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        public static IEnumerable<T> Recurse<T>(this T self, Func<T, T> node)
        {
            Debug.Assert(self != null);
            Debug.Assert(node != null);

            for (var i = self; i != null; i = node(i))
                yield return i;
        }

        /// <summary>
        /// Recurses into the given object, obtaining children using the given function.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="nodes"></param>
        /// <returns></returns>
        public static IEnumerable<T> Recurse<T>(this T self, Func<T, IEnumerable<T>> nodes)
        {
            Debug.Assert(self != null);
            Debug.Assert(nodes != null);

            yield return self;

            foreach (var i in nodes(self) ?? Enumerable.Empty<T>())
                if (i != null)
                    yield return i;
        }

        /// <summary>
        /// Recurses into each of the given objects, obtaining children using the given function.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="nodes"></param>
        /// <returns></returns>
        public static IEnumerable<T> Recurse<T>(this IEnumerable<T> self, Func<T, IEnumerable<T>> nodes)
        {
            Debug.Assert(self != null);
            Debug.Assert(nodes != null);

            foreach (var i in self)
                foreach (var j in i.Recurse(nodes))
                    yield return j;
        }

        /// <summary>
        /// Calls ToList on the enumerable if in DEBUG mode.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        public static IEnumerable<T> ToDebugList<T>(this IEnumerable<T> self)
        {
            Debug.Assert(self != null);

            // this method gets patched out if not in debug mode
            ToDebugListImpl<T>(ref self);

            return self;
        }

        [Conditional("DEBUG")]
        static void ToDebugListImpl<T>(ref IEnumerable<T> self)
        {
            Debug.Assert(self != null);

            self = self.ToList();
        }

        /// <summary>
        /// Returns an empty <see cref="IEnumerable"/> if <paramref name="source"/> is null.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IEnumerable EmptyIfNull(this IEnumerable source)
        {
            return source ?? Enumerable.Empty<object>();
        }

    }

}
