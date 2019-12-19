using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;

public class f : MonoBehaviour
{
    //i kinda need this stuff
    public KMBombInfo bomb;
    public KMAudio audio;
    public KMBombModule Module;
    public GameObject text;
    public Material[] backgrounds;
    static int ModuleIdCounter = 1;
    int ModuleId;
    private bool moduleSolved = false;
    private bool incorrect = false;

    //egg object
    public GameObject egg;

    //buttons are put in array in reading order
    public KMSelectable[] buttons;

    // Use this for initialization
    void Start()
    {
        text.GetComponent<Renderer>().material = backgrounds[0];
        DebugMsg("Correggt: " + bomb.GetSerialNumber()[5]);
    }

    void Awake()
    {
        ModuleId = ModuleIdCounter++;

        foreach (KMSelectable button in buttons)
        {
            KMSelectable pressedButton = button;
            button.OnInteract += delegate () { buttonPressed(pressedButton); return false; };
        }
    }

    void buttonPressed(KMSelectable pressedButton)
    {
        if (moduleSolved == true)
        {
            return;
        }
        int bombTimeCurrent = (int)bomb.GetTime() % 60 % 10;
        if (bombTimeCurrent.ToString() != bomb.GetSerialNumber()[5].ToString())
        {
            incorrect = true;
        }
        DebugMsg("Pregged: " + bombTimeCurrent + " Eggspected: " + bomb.GetSerialNumber()[5]);
        if (incorrect)
        {
            incorrect = false;
            DebugMsg("egg strikegged.");
            Module.HandleStrike();
        }
        else
        {
            moduleSolved = true;
            DebugMsg("egg solvegged.");
            text.GetComponent<Renderer>().material = backgrounds[1];
            StartCoroutine(Animation());
            Module.HandlePass();
        }
    }

    private IEnumerator Animation()
    {
        int rando = UnityEngine.Random.Range(0, 3);
        int rando2 = UnityEngine.Random.Range(0, 2);
        int rando3 = UnityEngine.Random.Range(0, 3);
        float[] getYurStuff = { 0.0f, .02f, -.02f };
        while(rando == 0 && rando2 == 0 && rando3 == 0)
        {
            rando = UnityEngine.Random.Range(0, 3);
            rando2 = UnityEngine.Random.Range(0, 2);
            rando3 = UnityEngine.Random.Range(0, 3);
        }
        for (int i = 0; i < 200; i++)
        {
            buttons[0].transform.localPosition = new Vector3(buttons[0].transform.localPosition.x + getYurStuff[rando], buttons[0].transform.localPosition.y + getYurStuff[rando2], buttons[0].transform.localPosition.z + +getYurStuff[rando3]);
            yield return new WaitForSeconds(.02f);
        }
        egg.SetActive(false);
    }

    void DebugMsg(string msg)
    {
        Debug.LogFormat("[egg #{0}] {1}", ModuleId, msg);
    }

    //twitch plays
    private bool isValid(string s)
    {
        char[] valids = { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' };
        if (!valids.Contains(s.ElementAt(0)))
        {
            return false;
        }
        return true;
    }

    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} egg <#> [Presses egg when last digit of bomb timer is '#']";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        string[] parameters = command.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*egg\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            if(parameters.Length == 1 || parameters.Length > 2)
            {
                yield return "sendtochaterror egg is confused.";
            }
            else
            {
                if (isValid(parameters[1]))
                {
                    yield return null;
                    if (parameters[1].Equals(bomb.GetSerialNumber()[5] + ""))
                    {
                        yield return "solve";
                    }
                    else
                    {
                        yield return "strike";
                    }
                    int temp = 0;
                    int.TryParse(parameters[1], out temp);
                    while ((int)bomb.GetTime() % 60 % 10 != temp)
                    {
                        yield return "trycancel egg was told to stop.";
                    }
                    buttons[0].OnInteract();
                }
                else
                {
                    yield return "sendtochaterror egg is confused.";
                }
            }
            yield break;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        int temp = 0;
        int.TryParse(""+bomb.GetSerialNumber()[5], out temp);
        while (temp != (int)bomb.GetTime() % 60 % 10)
        {
            yield return true;
            yield return new WaitForSeconds(0.01f);
        }
        buttons[0].OnInteract();
    }
}
