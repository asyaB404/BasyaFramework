using BasyaFramework.Core;
using UnityEngine;
using UnityEngine.EventSystems;


namespace BasyaFramework.UI
{
    public class BxUIPanel : BxBehaviour
    {
        [SerializeField] private UIBehaviour[] uiBehaviours;
        [SerializeField] private GameObject[] uiGameObjects;

        public UIBehaviour[] UIBehaviours => uiBehaviours;
        public GameObject[] UIGameObjects => uiGameObjects;

        [SerializeField] private CanvasGroup _canvasGroup;

        public virtual void InitializeUIComponents()
        {
        }

        public CanvasGroup CanvasGroup
        {
            get
            {
                if (gameObject.TryGetComponent(out _canvasGroup) && _canvasGroup == null)
                    _canvasGroup = gameObject.AddComponent<CanvasGroup>();
                return _canvasGroup;
            }
        }
    }
}