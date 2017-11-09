using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            NotACarController.totalLost = 0;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
