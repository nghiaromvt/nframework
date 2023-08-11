using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace NFramework
{
    [RequireComponent(typeof(Mask))]
    public class CutOutMask : Image
    {
        public override Material materialForRendering
        {
            get
            {
                Material material = new Material(base.materialForRendering);
                material.SetInt("_StencilComp", (int)CompareFunction.NotEqual);
                return material;
            }
        }
    }
}

