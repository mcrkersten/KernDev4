using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndGameMenu : MonoBehaviour
{
    public GameObject menu;
    public GameObject[] objectsToDisable;
    public Material materialColorOnForfeit;
    public Renderer groundRenderer;

    // Start is called before the first frame update
    void Awake() {
        ClientBehaviour.OnForfeit += OpenMenu;
    }

    void OpenMenu(bool isForfeit) {
        print(isForfeit);
        if (isForfeit) {
            for(int x = 0; x < objectsToDisable.Length; x++) {
                objectsToDisable[x].SetActive(false);
            }
            menu.SetActive(true);
            groundRenderer.material = materialColorOnForfeit;
        }
    }

    private void OnDestroy() {
        ClientBehaviour.OnForfeit -= OpenMenu;
    }
}
