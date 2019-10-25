using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class f : MonoBehaviour
{
    //i kinda need this stuff
    public KMBombInfo bomb;
    public KMAudio audio;
    public KMBombModule Module;
    static int ModuleIdCounter = 1;
    int ModuleId;
    private bool moduleSolved = false;
    private bool incorrect = false;

    //buttons are put in array in reading order
    public KMSelectable[] buttons;

    // Use this for initialization
    void Start()
    {

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
        int bombTimeCurrent = (int)bomb.GetTime() % 10;
        if (bombTimeCurrent.ToString() != bomb.GetSerialNumber()[5].ToString())
        {
            incorrect = true;
        }
        if (incorrect)
        {
            incorrect = false;
            DebugMsg("Egg striked. Time pressed: " + bombTimeCurrent + " Correct time: " + bomb.GetSerialNumber()[5]);
            Module.HandleStrike();
        }
        else
        {
            moduleSolved = true;
            DebugMsg("Egg solved.");
            StartCoroutine(Animation());
            Module.HandlePass();
        }
    }

    private IEnumerator Animation()
    {
            for(int i = 0; i< 1000; i++)
            {
                buttons[0].transform.localPosition = new Vector3(buttons[0].transform.localPosition.x, buttons[0].transform.localPosition.y, buttons[0].transform.localPosition.z + .02f);
            yield return new WaitForSeconds(.02f);
            }
    }

    void DebugMsg(string msg)
    {
        Debug.LogFormat("[Module Name #{0}] {1}", ModuleId, msg);
    }
}
