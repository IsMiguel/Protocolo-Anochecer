using UnityEngine;
using UnityEngine.UI;

/// <summary>Completa el objetivo provisional al alcanzar el punto de reunión.</summary>
public sealed class MissionObjectiveTrigger : MonoBehaviour
{
    [SerializeField] private Text objectiveText;
    [SerializeField] private string completionText = "OBJETIVO CUMPLIDO\nPuesto alcanzado";
    private bool completed;

    public void Configure(Text view, string completedMessage)
    {
        objectiveText = view;
        completionText = completedMessage;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (completed || !other.CompareTag("Player"))
            return;

        completed = true;
        if (objectiveText != null)
            objectiveText.text = completionText;
    }
}
