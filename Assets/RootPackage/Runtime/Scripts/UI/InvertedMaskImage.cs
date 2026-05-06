using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace NFramework
{
    public class InvertedMaskImage : Image
    {
        public override Material materialForRendering
        {
            get
            {
                Material mat = new Material(base.materialForRendering);
                mat.SetFloat("_StencilComp", (int)CompareFunction.NotEqual);
                return mat;
            }
        }
    }
}
