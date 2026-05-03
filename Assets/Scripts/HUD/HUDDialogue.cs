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
    private float streamTick = 0.01f;

    private Timer fadeTimer;
    private Timer transitionTimer;
    [SerializeField]
    private Timer streamTimer;

    public GameObject DiaPanel {  get { return panel; } }

    void Start()
    {
        fadeTimer = new Timer(1f, FadeOut);
        transitionTimer = new Timer(transitionDelay, AdvanceDialogue);
        streamTimer = new Timer(streamTick, StreamNextChar);

        if (panel != null)
        {
            tmpSpeaker = panel.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            tmpText = panel.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            animator = panel.GetComponent<Animator>();
        }
    }

    void Update()
    {
        fadeTimer.Update();
        transitionTimer.Update();
    }

    public void ShowDialogue(Dialoguer dia)
    {
        if (!dia.isActive) return;

        foreach (DialogueEntryData entry in dia.entries)
        {
            queuedEntries.Enqueue(entry);
        }
        DialogueEntryData first = queuedEntries.Dequeue();

        tmpSpeaker.text = first.title;
        tmpText.text = first.text;
        streamText = first.text;
        fadeTimer.Reset(first.time);

        panel.SetActive(true);
        dia.isActive = false;
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
        animator.SetBool("Fade", false);
        DialogueEntryData next = queuedEntries.Dequeue();
        tmpSpeaker.text = next.title;
        tmpText.text = next.text;
        streamText = next.text;
        fadeTimer.Reset(next.time);
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
        streamText.Substring(0, streamIndex);
        streamIndex++;
    }
}
