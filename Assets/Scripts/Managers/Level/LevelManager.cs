using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    public static float transitionDuration = 0f;

    public static Dictionary<string, string> sceneMusic = new Dictionary<string, string>();
    
    void Awake(){
        if(instance == null){
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else{
            Destroy(gameObject);
            return;
        }

        sceneMusic.Add("StartMenu", "TitleMusic");
        sceneMusic.Add("MainScene", "BattleMusic");
    }

    public async void LoadScene(string sceneName){
        await Task.Delay((int)transitionDuration * 1000);
        if(sceneMusic.ContainsKey(SceneManager.GetActiveScene().name)) {
            FindObjectOfType<AudioManager>().Stop(sceneMusic[SceneManager.GetActiveScene().name]);
        }
        SceneManager.LoadSceneAsync(sceneName);
    }
}