using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NFramework
{
    public class PopupView : UIView
    {
        [SerializeField] protected List<Button> btnCloses;

        protected virtual void Start()
        {
            foreach (var btn in btnCloses)
            {
                btn.onClick.AddListener(() => CloseSelf());
            }
        }
    }
}
