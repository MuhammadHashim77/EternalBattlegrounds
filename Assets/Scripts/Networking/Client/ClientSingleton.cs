using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ClientSingleton : MonoBehaviour
{
    private static ClientSingleton instance;

    private ClientGameManager gameManager;

    public static ClientSingleton Instance
    {
        get
        {
            if(instance != null) { return instance; }

            instance = FindObjectOfType<ClientSingleton>();

            if(instance == null)
            {
                Debug.Log("No client singleton in the scene");
                return null;
            }

            return instance;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public async Task CreateClient()
    {
        gameManager = new ClientGameManager();

        await gameManager.InitAsync();
    }
}
