using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NFramework
{
    [ExecuteInEditMode]
    public class UIGrayscale : BaseMeshEffect
    {
        private Image _image;
        private Image image
        {
            get
            {
                if (!_image)
                {
                    _image = GetComponent<Image>();
                }
                return _image;
            }
        }

        protected override void OnEnable()
        {
            var material = Resources.Load<Material>("Greyscale");
            image.material = material;
        }

        protected override void OnDisable()
        {
            image.material = null;
        }

        protected override void Start()
        {
            base.Start();

            var material = Resources.Load<Material>("Greyscale");
            image.material = material;
        }

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive())
            {
                return;
            }

            var verts = new List<UIVertex>();
            vh.GetUIVertexStream(verts);

            for (var i = 0; i < verts.Count; i++)
            {
                var v = verts[i];
                v.uv3 = new Vector4(1, 1, 1, 1);
                verts[i] = v;
            }

            vh.Clear();
            vh.AddUIVertexTriangleStream(verts);
        }
    }
}