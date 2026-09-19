using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace NFramework.UI
{
    public class InvertedMaskImage : Image
    {
        private Material _invertedMat;

        public override Material materialForRendering
        {
            get
            {
                var baseMat = base.materialForRendering;
                if (_invertedMat == null || _invertedMat.shader != baseMat.shader)
                {
                    if (_invertedMat != null)
                        DestroyImmediate(_invertedMat);

                    _invertedMat = new Material(baseMat);
                    _invertedMat.SetFloat("_StencilComp", (int)CompareFunction.NotEqual);
                }

                return _invertedMat;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_invertedMat != null)
            {
                DestroyImmediate(_invertedMat);
                _invertedMat = null;
            }
        }
    }
}
