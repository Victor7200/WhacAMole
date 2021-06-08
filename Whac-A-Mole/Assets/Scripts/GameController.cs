using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using TMPro;

public class GameController : MonoBehaviour
{
    public static GameController instance;
    public GameObject mainMenu, inGameUI, endScreen, recordPanel, padreTopo;

    public Transform molesParent;
    private MoleBehaviour[] moles;

    public bool playing = false;
    public bool isSlowed = false;

    public float gameDuration = 60f;
    public float timePlayed;

    int points = 0;
    int clicks = 0;
    int record = 0;
    int clicksTotales;
    int failedClicks;
    public float powerUpBomba;

    public TMP_InputField nameField;

    public TextMeshProUGUI infoGame, pointsText,timeText,recordText,highScoreKeyText;

    void Awake()
    {
        if (GameController.instance == null)
        {
            ConfigureInstance();
        }
        else
        {
            Destroy(this);
        }

        /*powerUpBomba = Random.Range(0.0f, 1.0f);

        if (powerUpBomba > 0.5f && timePlayed > 3)
        {
            PowerUpAnimation.instance.MoverIzquierda();
        }*/
    }

    void ConfigureInstance()
    {
        //Configura acceso a moles
        moles = new MoleBehaviour[molesParent.childCount];
        for (int i = 0; i < molesParent.childCount; i++)
        {
            moles[i] = molesParent.GetChild(i).GetComponent<MoleBehaviour>();
        }

        //Inicia los puntos
        points = 0;
        clicks = 0;
        failedClicks = 0;

        //Activa la UI inicial
        inGameUI.SetActive(false);
        mainMenu.SetActive(true);
        endScreen.SetActive(false);
        recordPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

        if (playing == true)
        {
            timePlayed += Time.deltaTime;

            if (timePlayed >= gameDuration)
            {

                ShowEndScreen();
                playing = false;
                for (int i = 0; i < moles.Length; i++)
                {
                    moles[i].StopMole();
                }      
            }

            else
            {
                CheckClicks();
            }         
        }

        if (playing == true && isSlowed == true)
        {
            for(float i = 0; i < 5; i++)
            {
                i+= Time.deltaTime;
                timeText.text = "Tiempo de partida: " + Mathf.FloorToInt(timePlayed/2.0f);
            }
        }

        pointsText.text = "Puntuación: " + points;

        timeText.text = "Tiempo de partida: " + Mathf.FloorToInt(timePlayed);

        clicksTotales = clicks + failedClicks;

        PlayerPrefs.SetString("playerName", nameField.ToString());
        PlayerPrefs.Save();

        Debug.Log("Record de " + PlayerPrefs.GetString("playerName"));

        highScoreKeyText.text = PlayerPrefs.GetString("playerName") + " : " + PlayerPrefs.GetInt("highScoreKey");

        recordText.text = "Récord: " + PlayerPrefs.GetInt("highScoreKey") + " por " + PlayerPrefs.GetString("playerName");

        SaveRecord();
    }


    void ShowEndScreen()
    {
        endScreen.SetActive(true);
        infoGame.text = " Total points : " + points + "\n " +  "\n " + (clicks*100/clicksTotales) + "% clicks acertados \n" + failedClicks + " clicks fallados";
    }

    /// <summary>
    /// Function called from End Screen when players hits Retry button
    /// </summary>
    public void Retry()
    {
        //Guardar record si es necesario

        //Acceso al texto escrito

        //Reinicia información del juego
        ResetGame();
        //Cambia las pantallas
        inGameUI.SetActive(true);
        mainMenu.SetActive(false);
        endScreen.SetActive(false);
        //Activa juego
        playing = true;

        //Reinicia moles
        for (int i = 0; i < moles.Length; i++)
        {
            moles[i].ResetMole();
        }
    }

    /// <summary>
    /// Restarts all info game
    /// </summary>
    void ResetGame()
    {
        for (int i = 0; i < moles.Length; i++)
        {
            moles[i].StopMole();
        }

        timePlayed = 0.0f;
        points = 0;
        clicks = 0;
        failedClicks = 0;
    }

    public void EnterMainScreen()
    {
        //Reinicia información del juego
        ResetGame();
        //Cambia las pantallas
        inGameUI.SetActive(false);
        mainMenu.SetActive(true);
        endScreen.SetActive(false);
        recordPanel.SetActive(false);

    }

    /// <summary>
    /// Used to check if players hits or not the moles/powerups
    /// </summary>
    public void CheckClicks()
    {
        if ((Input.touchCount >= 1 && Input.GetTouch(0).phase == TouchPhase.Ended) || (Input.GetMouseButtonUp(0)))
        {
          
            Vector3 pos = Input.mousePosition;
            if (Application.platform == RuntimePlatform.Android)
            {
                pos = Input.GetTouch(0).position;
            }

            Ray rayo = Camera.main.ScreenPointToRay(pos);
            RaycastHit hitInfo;
            if (Physics.Raycast(rayo, out hitInfo))
            {
                if (hitInfo.collider.tag.Equals("Mole"))
                {
                    MoleBehaviour mole = hitInfo.collider.GetComponent<MoleBehaviour>();
                    if (mole != null)
                    {
                        mole.OnHitMole();
                        points += 100;
                    }

                    clicks++;
                }
                else
                {
                    failedClicks++;
                }

                if (hitInfo.collider.tag.Equals("Bomba"))
                {
                    padreTopo.GetComponentInChildren<MoleBehaviour>();
                    if (padreTopo != null)
                    {
                        for(int i = 0; i<9; i++)
                        {
                            if (MoleBehaviour.instance.originalPosition.y > 0)
                            {
                                points += 100;
                            }
                        }
                    }

                    clicks++;
                }

                if (hitInfo.collider.tag.Equals("Tiempo"))
                {
                    isSlowed = true;

                    clicks++;
                }
            }
        }
    }

    public void OnGameStart()
    {
        mainMenu.SetActive(false);
        inGameUI.SetActive(true);
        points = 0;
        for (int i = 0; i < moles.Length; i++)
        {
            moles[i].ResetMole(moles[i].initTimeMin, moles[i].initTimeMax);
        }
        playing = true;
    }

    public void SaveRecord()
    {
        if (points > PlayerPrefs.GetInt("highScoreKey", record))
        {
            recordPanel.SetActive(true);

            PlayerPrefs.SetInt("highScoreKey", points);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// Funcion para entrar en pausa, pone playing en false y muestra la pantalla de pausa.
    /// </summary>
    public void EnterOnPause()
    { 
    
    
    }
}
