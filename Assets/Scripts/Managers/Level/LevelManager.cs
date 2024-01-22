using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    public static float transitionDuration = 0f;
    
    void Awake(){
        if(instance == null){
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else{
            Destroy(gameObject);
        }
    }

    public async void LoadScene(string sceneName){
        await Task.Delay((int)transitionDuration * 1000);

        SceneManager.LoadSceneAsync(sceneName);
    }
}