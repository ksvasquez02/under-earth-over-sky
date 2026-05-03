using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HUDDialogue : MonoBehaviour
{
    [SerializeField]
    private GameObject panel;
    private TextMeshProUGUI tmpSpeaker;
    private TextMeshProUGUI tmpText;
    private Animator animator;
    private Queue<DialogueEntryData> queuedEntries = new Queue<DialogueEntryData>();

    [SerializeField]
    private float transitionDelay = 0.5f;

    private int streamIndex = 0;
    private string streamText;
    [SerializeField]
    private float streamSpeed = 100f;
    private float streamTick;

    private Timer fadeTimer;
    private Timer transitionTimer;
    [SerializeField]
    private Timer streamTimer;

    public GameObject DiaPanel {  get { return panel; } }

    private void Awake()
    {
        streamTick = 1f / streamSpeed; ;
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
        animator.SetBool("Fade", true);
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

    public void StreamIn()
    {

    }

    private void StreamNextChar()
    {
        if (streamText == null) return;
        if (streamIndex >= streamText.Length)
        {
            streamIndex = 0;
            return;
        }
        streamIndex++;
        string subText = streamText[..streamIndex];
        if (subText[^1] == '<')
        {
            int closingIndex = streamText.IndexOf('>', streamIndex);
            if (closingIndex > 0)
            {
                streamIndex = closingIndex + 1;
                subText = streamText[..streamIndex];
            }
        }
        tmpText.text = subText;
        streamTimer.Reset();
    }
}
