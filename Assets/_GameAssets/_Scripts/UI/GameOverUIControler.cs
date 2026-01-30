using UnityEngine;
using UnityEngine.UI;

public class GameOverUIControler : MonoBehaviour {

    [SerializeField] private Button continueButton;


    private void Start() {
        this.continueButton.onClick.AddListener(OnContinueClicked);
    }

    private void OnDestroy() {
        this.continueButton.onClick.RemoveListener(OnContinueClicked);

    }
    void OnContinueClicked() {
        GameManager.Instance.ContinueGame();
    }

}
