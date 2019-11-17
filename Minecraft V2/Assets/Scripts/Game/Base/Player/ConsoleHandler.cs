using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConsoleHandler : MonoBehaviour
{
    Player player;
    World world;

    [SerializeField] private GameObject ConsolePanel;
    [SerializeField] private InputField NewCommandInput;
    [SerializeField] private GameObject OldCommandspanel;
    [SerializeField] private GameObject oldTextPrefab;

    public Queue<string> OldCommands = new Queue<string>();
    Queue<GameObject> oldCommPrefabs = new Queue<GameObject>();

    bool ConsolePanelOn = false;

    void Start()
    {
        Debug.Log("ConsoleHandler Start");
        player = GameObject.Find("Player").GetComponent<Player>();
        world = GameObject.Find("World").GetComponent<World>();

        ConsolePanel.SetActive(false);
    }

    int i = 0;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Slash) && !player.inConsole)
        {
            Debug.Log("Slash Pressed");
            NewCommandInput.text = "/";
            NewCommandInput.enabled = true;
            player.inConsole = true;
        }

        if(Input.GetKeyDown(KeyCode.Escape) && player.inConsole)
        {
            player.inConsole = false;
        }

        if (player.inConsole && !ConsolePanelOn)
        {
            ConsolePanel.SetActive(true);
            ConsolePanelOn = true;
        }
        else if(!player.inConsole && ConsolePanelOn)
        {
            ConsolePanel.SetActive(false);
            ConsolePanelOn = false;
        }

        if (player.inConsole)
        {
            if (Input.GetKeyDown(KeyCode.Return)) //Normal Enter on keyboard
            {
                OldCommands.Enqueue(ManageCommands(NewCommandInput.text));
                NewCommandInput.text = "/";
                UpdateOldCommandsConsole();
            }
        }
    }

    string ManageCommands(string lastcomm)
    {
        if (lastcomm.StartsWith("/")) //If inputText starts with /
        {
            string DoesntExists = "That command doesnt exists! Type /help";

            string[] wordsComm = lastcomm.ToLower().Split(" ".ToCharArray());
            int WordsInCommand = wordsComm.Length;
            if(WordsInCommand == 1)
            {
                switch (lastcomm)
                {
                    case "/clear":
                        {
                            OldCommands.Clear();
                            return "Console has been Cleared!";
                        }

                    default:
                        return DoesntExists;
                }
            }
            if (!player.AllowCheats && player.GameMode == VoxelData.GameModes.Survival)
            {
                return "You dont have access to commands!";
            }

            if(WordsInCommand == 2)
            {
                switch (wordsComm[0])
                {
                    case "/gamemode":
                    {
                        switch (wordsComm[1])
                        {
                            case "survival":
                            {
                                player.GameMode = VoxelData.GameModes.Survival;
                                       
                                return "Your Gamemode has been changed to Survival!";
                            }

                            case "creative":
                            {
                                player.GameMode = VoxelData.GameModes.Creative;
                                return "Your Gamemode has been changed to Creative!";
                            }

                            default:
                                return "That Gamemode doesnt exists!";
                        }
                    }
                    case "/timeset":
                    {
                        switch (wordsComm[1])
                        {
                            case "day":
                                {
                                    world.globalLightLevel = 0.5f;
                                    return "Time set to day!";
                                }

                            case "night":
                                {
                                    world.globalLightLevel = 0.06f;
                                    return "Time set to night!";
                                }

                            default:
                                return "That Gamemode doesnt exists!";
                        }
                    }
                    default:
                        return DoesntExists;
                }
            }
            else
            {
                return DoesntExists;
            }
        }
        else // return string
        {
            return lastcomm;
        }
    }

    void UpdateOldCommandsConsole()
    {
        if (OldCommands.Count > 0)
        {
            foreach(GameObject g in oldCommPrefabs)
            {
                Destroy(g);
            }

            if (OldCommands.Count > 8)
            {
                OldCommands.Dequeue();
            }

            Vector3 pos = OldCommandspanel.transform.position;
            for (int i = OldCommands.Count; i > 0; i--)
            {
                GameObject oldText = Instantiate(oldTextPrefab, new Vector3(pos.x, pos.y + 50 + i * 50, pos.z), Quaternion.identity, OldCommandspanel.transform);
                Text t = oldText.GetComponent<Text>();
                t.text = OldCommands.Dequeue();
                OldCommands.Enqueue(t.text);
                oldCommPrefabs.Enqueue(oldText);
            }
        }
    }
}
