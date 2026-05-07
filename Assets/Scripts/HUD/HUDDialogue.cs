using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HUDDialogue : MonoBehaviour
{
    [SerializeField]
    private GameObject panel;
    private TextMeshProUGUI tmpSpeaker;
    private TextMeshProUGUI tmpText;
    private Animator animator;
    private readonly Queue<DialogueEntryData> queuedEntries = new();

    [SerializeField]
    private float transitionDelay = 0.5f;
    private static readonly int FadeHash = Animator.StringToHash("Fade");

    private string streamText;
    private int streamIndex = 0;
    [SerializeField]
    private float streamSpeed = 100f;
    private float streamTick;

    private Timer fadeTimer;
    private Timer transitionTimer;
    private Timer streamTimer;

    public GameObject DiaPanel {  get { return panel; } }

    private void Awake()
    {
        streamTick = 1f / streamSpeed;
    }

    void Start()
    {
        fadeTimer = new Timer(1f, FadeOut);
        transitionTimer = new Timer(transitionDelay, AdvanceDialogue);
        streamTimer = new Timer(streamTick, StreamNextChar);

        if (panel != null)
        {
            TextMeshProUGUI[] tmps = panel.GetComponentsInChildren<TextMeshProUGUI>();
            tmpSpeaker = tmps[0];
            tmpText = tmps[1];
            animator = panel.GetComponent<Animator>();
            panel.SetActive(false);
        }
    }

    void Update()
    {
        fadeTimer.Update();
        transitionTimer.Update();
        streamTimer.Update();
    }

    public void ShowDialogue(Dialoguer dia)
    {
        if (!dia.isActive) return;

        foreach (DialogueEntryData entry in dia.entries)
        {
            queuedEntries.Enqueue(entry);
        }
        DialogueEntryData first = queuedEntries.Dequeue();

        panel.SetActive(true);
        dia.isActive = false;

        SetDialogue(first);
    }

    private void FadeOut()
    {
        animator.SetBool(FadeHash, true);
        transitionTimer.Reset();
    }

    public void AdvanceDialogue()
    {
        if (queuedEntries.Count <= 0)
        {
            HideDialogue();
            return;
        }
        DialogueEntryData next = queuedEntries.Dequeue();

        SetDialogue(next);
    }

    private void SetDialogue(DialogueEntryData data)
    {
        tmpSpeaker.text = data.title;
        tmpText.text = "";

        streamText = data.text;
        streamIndex = 0;
        streamTimer.Reset();

        animator.SetBool("Fade", false);
        fadeTimer.Reset(data.time);
    }

    public void HideDialogue()
    {
        panel.SetActive(false);
    }

    private void StreamNextChar()
    {
        if (streamText == null) return;
        if (streamIndex >= streamText.Length)
        {
            streamIndex = 0;
            return;
        }

        if (streamText[streamIndex] == '<')
        {
            int closingIndex = streamText.IndexOf('>', streamIndex);
            if (closingIndex > 0) { streamIndex = closingIndex + 1; }
        }
        else if (streamText[streamIndex] == '\\')
        {
            if (streamText[streamIndex + 1] == 'n') { streamIndex++; }
        }
        string modText = streamText.Insert(streamIndex + 1, "<color=#ffffff00>");

        tmpText.text = modText;

        streamIndex++;
        streamTimer.Reset();
    }
}
