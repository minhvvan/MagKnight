using UnityEngine;
using UnityEngine.UI;

namespace Moon
{
    //popup ui handler
    public class PopupUIController : MonoBehaviour
    {
        [SerializeField] public Image backgroundImage;
        [SerializeField] public PauseMenuUIController pauseMenuUIController;
        [SerializeField] public OptionUIController optionUIController;
        [SerializeField] public ConfirmPopupUIController confirmPopupUIController;


        void OnEnable()
        {
            UIManager.Instance.SetPopupUIController(this);
        }

        void OnDisable()
        {
            UIManager.Instance.ReleasePopupUIController();
        }
    }
}

