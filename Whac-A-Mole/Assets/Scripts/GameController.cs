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
    public float timeScale = -0.5f;

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

        //Intento de ajustar el tiempo y la aleatoriedad del powerup
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

        Debug.Log("Record de " + PlayerPrefs.GetString("playerName"));
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

        //Powerup del tiempo
        if (playing == true && isSlowed == true)
        {
            for(float i = 0; i < 5; i++)
            {
                timePlayed = timePlayed + (Time.deltaTime * Time.timeScale);
                timeText.text = "Tiempo de partida: " + Mathf.FloorToInt(timePlayed);

                if (i > 4)
                {
                    isSlowed = false;
                }
            }
        }

        //Actualización de textos y almacenamiento del nombre del jugador junto al récord en la función Save Record
        pointsText.text = "Puntuación: " + points;

        timeText.text = "Tiempo de partida: " + Mathf.FloorToInt(timePlayed);

        //Se suman los clicks fallidos y clicks para así poder calcular el porcentaje
        clicksTotales = clicks + failedClicks;

        PlayerPrefs.SetString("playerName", nameField.ToString());
        PlayerPrefs.Save();

        highScoreKeyText.text = PlayerPrefs.GetString("playerName") + " : " + PlayerPrefs.GetInt("highScoreKey");

        recordText.text = "Récord: " + PlayerPrefs.GetInt("highScoreKey") + " por " + PlayerPrefs.GetString("playerName");

        SaveRecord();
    }


    void ShowEndScreen()
    {
        //Pantalla de final de partida con toda la información correspondiente y el cálculo de clicks fallados y porcentaje acertado
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
                    //Se suma 1 al contador de clicks
                    clicks++;
                }
                else
                {
                    //Se suma uno al contador de fallos
                    failedClicks++;
                }

                //Powerup Bomba
                if (hitInfo.collider.tag.Equals("Bomba"))
                {
                    padreTopo.GetComponentInChildren<MoleBehaviour>();
                    if (padreTopo != null)
                    {
                        padreTopo.GetComponentInChildren<MoleBehaviour>().isShown = false;
                    }

                    clicks++;
                }

                //Tag del tiempo para activar este bool y que se reproduzca la función que hay en
                //El update del powerup
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

    //Función encargada de actualizar y almacenar el récord
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
