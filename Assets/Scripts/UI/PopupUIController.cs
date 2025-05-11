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
        [SerializeField] public PlayerDetailUIController playerDetailUIController;
        [SerializeField] public UpgradeStatUIController upgradeStatUIController;
        [SerializeField] public ProductUIController productUIController;
        [SerializeField] public ClearUIController clearUIController;
        [SerializeField] public GameResultUIController gameResultUIController;
        
        void OnEnable()
        {
            UIManager.Instance.SetPopupUIController(this);
            artifactInventoryUIController.Initialized();
            productUIController.Initialized();
        }

        public void HideAllPopupAvailableUI()
        {
            pauseMenuUIController.HideUI();
            optionUIController.HideUI();
            confirmPopupUIController.HideUI();
            artifactInventoryUIController.HideUI();
            playerDetailUIController.HideUI();
            upgradeStatUIController.HideUI();
            productUIController.HideUI();
        }
    }
}

