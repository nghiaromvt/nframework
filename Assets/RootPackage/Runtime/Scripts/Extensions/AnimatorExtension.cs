using System.Linq;
using UnityEngine;

namespace NFramework
{
    public static class AnimatorExtension
    {
        /// <summary>
        /// Determines if an animator contains a certain parameter, based on a type and a name
        /// </summary>
        /// <returns><c>true</c> if it has parameter of type the specified self name type; otherwise, <c>false</c>.</returns>
        public static bool HasParameterOfType(this Animator animator, string paramName, AnimatorControllerParameterType paramType)
        {
            if (string.IsNullOrEmpty(paramName))
                return false;

            AnimatorControllerParameter[] parameters = animator.parameters;
            return parameters.Any(currParam => currParam.type == paramType && currParam.name == paramName);
        }

        public static bool ContainsAnimation(this Animator animator, string animationName)
        {
            RuntimeAnimatorController ac = animator.runtimeAnimatorController;
            string lowerCase = animationName.ToLowerInvariant();
            return ac.animationClips.Any(animationClip => animationClip.name.ToLowerInvariant() == lowerCase);
        }

        public static bool ContainsAnimation(this Animator animator, AnimationClip animationClip)
        {
            RuntimeAnimatorController ac = animator.runtimeAnimatorController;
            return ac.animationClips.Any(t => t == animationClip);
        }

        public static bool TryGetAnimation(this Animator animator, string animationName, out AnimationClip animationClip)
        {
            animationClip = null;
            RuntimeAnimatorController ac = animator.runtimeAnimatorController;
            string lowerCase = animationName.ToLowerInvariant();
            foreach (var curAnimationClip in ac.animationClips)
            {
                if (curAnimationClip.name.ToLowerInvariant() == lowerCase)
                {
                    animationClip = curAnimationClip;
                    return true;
                }
            }
            return false;
        }
    }
}
