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
        [SerializeField] public ArtifactInventoryUIController artifactInventoryUIController;
        [SerializeField] public GameOverUIController gameOverUIController;

        void OnEnable()
        {
            UIManager.Instance.SetPopupUIController(this);
            artifactInventoryUIController.Initialized();
        }
    }
}

