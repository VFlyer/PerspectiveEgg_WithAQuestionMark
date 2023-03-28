using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class EggSaladScript : MonoBehaviour {

	static int[] pressOffsetOrderBase = new[] { 1, 2, 0, 3, 4 };

    public KMSelectable[] eggs;
    public KMBombModule modSelf;
    public TextMesh eggText;

    private Vector3[] storedEggsPositions;
    [SerializeField]
    private Vector3 offset;
    [SerializeField]
    private float circleSize;
    static int modIDCnt;

    int[] idxPositionsAll, idxColorsAll, expectedPressIdx;
    int idxCurPress, moduleID;
    bool waiting = true, solved;
    Color[] possibleColors = new[] { Color.red * 0.5f + Color.white * 0.5f, Color.green * 0.5f + Color.white * 0.5f, Color.blue * 0.5f + Color.white * 0.5f, Color.yellow * 0.25f + Color.white * 0.75f, (Color)new Color32(223,209,171,255) };
    List<string> Comparer = new List<string> { "red", "green", "blue", "yellow", "eggshell" },
    Type = new List<string> { "top", "top-right", "bottom-right", "bottom-left", "top-left" };
    float[] speeds = new[] { 1f, 2f, 4f, 8f };
    IEnumerator[] eggAnims;
    void QuickLog(string toLog, params object[] args)
    {
        Debug.LogFormat("[perspective eggs #{0}] {1}", moduleID, string.Format(toLog, args));
    }
    // Use this for initialization
    void Start () {
        moduleID = ++modIDCnt;
        idxColorsAll = Enumerable.Range(0, 5).ToArray().Shuffle();
        idxPositionsAll = Enumerable.Range(0, 5).ToArray();
        eggAnims = new IEnumerator[5];
        for (var x = 0; x < eggs.Length; x++)
        {
            eggs[x].transform.localPosition = offset + new Vector3(Mathf.Sin(Mathf.PI * 2 / 5 * x), 0, Mathf.Cos(Mathf.PI * 2 / 5 * x)) * circleSize;
            eggs[x].GetComponent<MeshRenderer>().material.color = possibleColors[idxColorsAll[x]];
            var y = x;
            eggs[x].OnInteract += delegate {
                Press(y);
                return false;
            };
            eggAnims[x] = ChangePosition(x, Enumerable.Range(0, 5).Where(a => a != x).PickRandom(), speeds.PickRandom());
        }
        expectedPressIdx = pressOffsetOrderBase.Select(a => (a + idxColorsAll.ToList().IndexOf(0)) % 5).ToArray();
        QuickLog("red at {0}! wow", Type[idxColorsAll.ToList().IndexOf(0)]);
        QuickLog("press these eggs in the order! {0}", expectedPressIdx.Select(a => Comparer[idxColorsAll[a]]).Join(", "));

        storedEggsPositions = eggs.Select(a => a.transform.localPosition).ToArray();
        idxPositionsAll = Enumerable.Range(0, 5).ToArray();
	}
    private void Press(int idx)
    {
        var idxPressed = expectedPressIdx.ToList().IndexOf(idx);

        if (idxCurPress == idxPressed)
        {
            QuickLog("correct! {0}!", Comparer[idxColorsAll[idx]]);
            idxCurPress++;
            idxPositionsAll[idx] = -1;
            StopCoroutine(eggAnims[idx]);
            waiting = false;
            StartCoroutine(DisappearAnimation(idx));
            if (idxCurPress >= 5)
            {
                QuickLog("mod done!");
                solved = true;
                modSelf.HandlePass();
                GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
                StartCoroutine(SolveAnim());
            }
        }
        else if (idxCurPress < idxPressed)
        {
            QuickLog("oh no! {0} is wrong egg! :(", Comparer[idxColorsAll[idx]]);
            modSelf.HandleStrike();
        }
    }
    private IEnumerator SolveAnim()
    {
        eggText.gameObject.SetActive(true);
        for (float t = 0;t < 1f;t += Time.deltaTime)
        {
            eggText.characterSize = .004f * t;
            yield return null;
        }
        eggText.characterSize = .004f;
    }
    private IEnumerator ChangePosition(int affectedEggIdx, int idxNext, float speed = 1f)
    {
        if (Enumerable.Range(0, 5).Where(a => a != affectedEggIdx).All(a => idxPositionsAll[a] != idxNext))
        {

            var lastPosAffectedEgg = storedEggsPositions[idxPositionsAll[affectedEggIdx]];
            var nextPosAffectedEgg = storedEggsPositions[idxNext];
            idxPositionsAll[affectedEggIdx] = idxNext;


            for (float t = 0; t < 1f; t += Time.deltaTime * speed)
            {
                eggs[affectedEggIdx].transform.localPosition = Vector3.LerpUnclamped(lastPosAffectedEgg, nextPosAffectedEgg, t);
                yield return null;
            }
            eggs[affectedEggIdx].transform.localPosition = storedEggsPositions[idxNext];
            yield return new WaitForSeconds(Random.value);
        }
        eggAnims[affectedEggIdx] = ChangePosition(affectedEggIdx, Random.Range(0, 5), speeds.PickRandom());
        waiting = false;
        yield break;
    }

    private IEnumerator DisappearAnimation(int idx)
    {
        int rando = Random.Range(0, 3);
        int rando2 = Random.Range(0, 2);
        int rando3 = Random.Range(0, 3);
        float[] getYurStuff = { 0.0f, .02f, -.02f };
        while (rando == 0 && rando2 == 0 && rando3 == 0)
        {
            rando = Random.Range(0, 3);
            rando2 = Random.Range(0, 2);
            rando3 = Random.Range(0, 3);
        }
        for (int i = 0; i < 100; i++)
        {
            eggs[idx].transform.localPosition = new Vector3(eggs[idx].transform.localPosition.x + getYurStuff[rando], eggs[idx].transform.localPosition.y + getYurStuff[rando2], eggs[idx].transform.localPosition.z + +getYurStuff[rando3]);
            yield return new WaitForSeconds(.01f);
        }
        eggs[idx].gameObject.SetActive(false);
    }
    // Update is called once per frame
    void Update () {
        if (waiting || solved) return;
		if (idxPositionsAll.Any(a => a == -1))
        {
            waiting = true;
            StartCoroutine(eggAnims[Enumerable.Range(0, 5).Where(a => idxPositionsAll[a] != -1).PickRandom()]);
        }
	}
}
