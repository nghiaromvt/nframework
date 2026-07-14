using UnityEngine;

namespace NFramework
{
    public static class BoundsExtensions {
        /// <summary>
        /// Returns a new bounds that encapsulates both input bounds, enabling fluent chaining.
        /// </summary>
        /// <param name="bounds">The initial bounds.</param>
        /// <param name="other">The bounds to include.</param>
        /// <returns>A new bounds containing both <paramref name="bounds"/> and <paramref name="other"/>.</returns>
        /// <example>
        /// <code>
        /// Bounds combined = rendererA.bounds
        ///     .ExpandToInclude(rendererB.bounds)
        ///     .ExpandToInclude(rendererC.bounds);
        /// </code>
        /// </example>
        public static Bounds ExpandToInclude(this Bounds bounds, Bounds other) {
            bounds.Encapsulate(other);
            return bounds;
        }
    }
}