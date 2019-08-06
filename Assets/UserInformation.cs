using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UserInformation : MonoBehaviour
{
    private static UserInformation instance = null;
    public static UserInformation Instance
    {
        get {
            if (instance == null) {
                // This is where the magic happens.
                instance = FindObjectOfType(typeof(UserInformation)) as UserInformation;
            }

            // If it is still null, create a new instance
            if (instance == null) {
                GameObject i = new GameObject("UserInformation");
                i.AddComponent(typeof(UserInformation));
                instance = i.GetComponent<UserInformation>();
            }
            return instance;
        }
    }
    public string userID;
    // Start is called before the first frame update
    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
    }
}
