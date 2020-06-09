using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public Text MonsterHealthText;
    public GameObject MonsterHealthBar;
    public GameObject MonsterHealthBarBG;

    public TextMeshPro MonsterHealth3D;
    
    public Text PlayerHealthText;
    public GameObject PlayerHealthBar;
    public GameObject PlayerHealthBarBG;

    public TextMeshProUGUI RespawnText;
    public GameObject RespawnParent;

    public GameObject CountDownParent;
    public TextMeshProUGUI CountdownText;

    public TextMeshProUGUI GameTimeText;
    public TextMeshPro GameTimeText3D;

    public TextMeshProUGUI GameOverText;

    protected float respawnTimer;
    protected int respawnTimeAsInt;

    public Button powerUpButton;
    public Text currentGunText;
    
    protected int gameTimeAsInt;
    
    public void Start()
    {
        BoltManager.Instance.OnVREntityRegistered += OnVRPlayer;
        BoltManager.Instance.OnPlayerEntityRegistered += OnPlayer;
        BoltManager.Instance.OnGameOver += OnGameOver;
        
        BoltManager.Instance.OnItemChanged += UpdateInventoryText;
        
        BoltManager.Instance.OnGameStateRegistered += OnGameStateRegistered;

        RespawnParent.gameObject.SetActive(false);
        GameOverText.gameObject.SetActive(false);
        CountDownParent.SetActive(false);

        GameTimeText.text = GameStateManager.GAME_DURATION.ToString();
    }

    protected void OnVRPlayer()
    {
        BoltManager.Instance.vrPlayer.gameObject.GetComponent<VRPlayer>().OnHealthChanged += OnVRHealthChanged;
        BoltManager.Instance.vrPlayer.gameObject.GetComponent<VRPlayer>().OnDeath += OnVRPlayerDeath; 
    }

    protected void OnPlayer(BoltEntity boltEntity)
    {
        if (boltEntity.IsOwner)
        {
            var player = boltEntity.GetComponent<Player>();
            player.OnHealthChanged += OnPlayerHealthChanged;
            player.OnDeath += OnPlayerDeath;
        }
    }

    protected void OnVRHealthChanged(int health, int maxHealth)
    {
        var fullSize = MonsterHealthBarBG.GetComponent<RectTransform>().sizeDelta;
        var width = fullSize.x * (health / (float) maxHealth);
        MonsterHealthBar.GetComponent<RectTransform>().sizeDelta = new Vector2(width, fullSize.y);
        MonsterHealthText.text = health.ToString();
        MonsterHealth3D.text = health.ToString();
    }
    
    protected void OnPlayerHealthChanged(int health, int maxHealth)
    {
        var fullSize = PlayerHealthBarBG.GetComponent<RectTransform>().sizeDelta;
        var width = fullSize.x * (health / (float) maxHealth);
        PlayerHealthBar.GetComponent<RectTransform>().sizeDelta = new Vector2(width, fullSize.y);
        PlayerHealthText.text = health.ToString();
    }

    protected void OnGameStateRegistered()
    {
        GameStateManager.Instance.OnGameStateChanged += OnGameStateChanged;
        OnGameStateChanged();
    }

    public void OnClickUsePowerup()
    {
        foreach (var player in BoltManager.Instance.playerList)
        {
            if (player.IsOwner)
            {
                player.GetComponent<Player>().UseCurrentPowerUp();
            }
        }
    }

    public void OnPlayerDeath(float respawnDelay)
    {
        respawnTimer = respawnDelay;
        RespawnParent.SetActive(true);
        respawnTimeAsInt = Mathf.RoundToInt(respawnTimer);
        RespawnText.text = respawnTimeAsInt.ToString();
    }

    public void OnVRPlayerDeath()
    {
    }

    public void OnGameOver(string winType)
    {
        foreach (var player in BoltManager.Instance.playerList)
        {
            if (player.IsOwner)
            {
                GameOverText.gameObject.SetActive(true);
                GameOverText.text = winType == "VRDead" ? "You Win!" : "Titan Wins!";
                return;
            }
        }
        
        // TODO
        // what does the vr player see?
        GameOverText.gameObject.SetActive(true);
        GameOverText.text = winType == "VRDead" ? "You Lost!" : "Titan Wins!";
        GameTimeText3D.text = GameOverText.text;
    }

    void Update()
    {
        if (respawnTimer > 0)
        {
            respawnTimer -= Time.deltaTime;
            var oldInt = respawnTimeAsInt;
            respawnTimeAsInt = Mathf.RoundToInt(respawnTimer);
            if (oldInt != respawnTimeAsInt)
            {
                RespawnText.text = respawnTimeAsInt.ToString();
            }

            if (respawnTimer <= 0)
            {
                RespawnParent.SetActive(false);
                respawnTimer = 0;
            }
        }

        
        if (GameStateManager.Instance != null && GameStateManager.Instance.DoesStateEnd())
        {
            var gameState = GameStateManager.Instance.GetGameState();
            var oldTime = gameTimeAsInt;
            var now = GameStateManager.GetNow();
            var endTime = GameStateManager.Instance.GetStateEndTime();
            gameTimeAsInt = Mathf.RoundToInt((endTime - now));
//            BoltConsole.Write("Now " + now + " end " + endTime + " as int " + gameTimeAsInt);
            if (oldTime != gameTimeAsInt)
            {
                if (gameState == GameStateManager.GameState.Running)
                {
                    GameTimeText.text = gameTimeAsInt.ToString();
                    GameTimeText3D.text = gameTimeAsInt.ToString();
                }
                else
                {
                    CountdownText.text = gameTimeAsInt.ToString();
                }
            }
        }
    }

    void UpdateInventoryText(ItemsManager.Items itm)
    {
        var item = Main.Instance.GetItemManager().GetItem(itm);
        if (item == null)
        {
            powerUpButton.GetComponentInChildren<Text>().text = "No Powerup";
        }
        else if (item.type.Contains("gun"))
        {
            currentGunText.text = "Current Gun: " + itm;
        }
        else
        {
            powerUpButton.GetComponentInChildren<Text>().text = "Use " + itm;
        }
    }
    void OnGameStateChanged()
    {
        var gameState = GameStateManager.Instance.GetGameState();
        switch (gameState)
        {
            case GameStateManager.GameState.Lobby:
            {
                GameTimeText.gameObject.SetActive(true);
                GameTimeText.text = GameStateManager.GAME_DURATION.ToString();
                GameOverText.gameObject.SetActive(true);
                GameOverText.text = "Waiting for players";
                break;
            }
            case GameStateManager.GameState.Countdown:
            {
                GameOverText.gameObject.SetActive(false);
                CountDownParent.SetActive(true);
                break;
            }

            case GameStateManager.GameState.Running:
            {
                GameOverText.gameObject.SetActive(false);
                CountDownParent.SetActive(false);
                break;
            }

            case GameStateManager.GameState.GameOver:
            {
                // wait for game over event
                GameTimeText.gameObject.SetActive(false);
                GameTimeText3D.text = "Game Over";
                break;
            }

            default:
            {
                break;
            }
        }
    }
}