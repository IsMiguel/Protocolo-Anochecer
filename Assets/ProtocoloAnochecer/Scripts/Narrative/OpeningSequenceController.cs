using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class OpeningSequenceController : MonoBehaviour
{
    [SerializeField] private Camera sequenceCamera;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private FirstPersonController playerController;
    [SerializeField] private Transform televisionShot;
    [SerializeField] private Transform cabinShot;
    [SerializeField] private Transform flightShot;
    [SerializeField] private Transform landingShot;
    [SerializeField] private Transform helicopter;
    [SerializeField] private Transform rotor;
    [SerializeField] private Transform playerStart;
    [SerializeField] private Text televisionText;
    [SerializeField] private Text subtitleText;
    [SerializeField] private Text objectiveText;
    [SerializeField] private CanvasGroup fadeGroup;

    private IEnumerator Start()
    {
        playerController.enabled = false;
        playerCamera.enabled = false;
        sequenceCamera.enabled = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        sequenceCamera.transform.SetPositionAndRotation(televisionShot.position, televisionShot.rotation);
        televisionText.text = "MATERIAL INFORMATIVO\nEUROPA · DÉCADA DE 1940\n\nPRESENTACIÓN OFICIAL";
        yield return new WaitForSeconds(4f);
        televisionText.text = "INFORME MILITAR\n\nInformación oficial para el personal asignado al despliegue.";
        yield return new WaitForSeconds(4f);

        yield return FadeTo(1f);
        televisionText.text = string.Empty;
        sequenceCamera.transform.SetPositionAndRotation(cabinShot.position, cabinShot.rotation);
        yield return FadeTo(0f);
        yield return ShowSubtitle("Compañero: ¿Te quedó claro qué tenemos que hacer?", 3.5f);
        yield return ShowSubtitle("Protagonista: Solo que tenemos que llegar. Las órdenes vienen después.", 4f);
        yield return ShowSubtitle("Compañero: Entonces buscamos a la unidad cuando aterricemos. Juntos.", 4f);

        yield return FadeTo(1f);
        sequenceCamera.transform.SetPositionAndRotation(flightShot.position, flightShot.rotation);
        helicopter.gameObject.SetActive(true);
        yield return FadeTo(0f);
        yield return ShowSubtitle("RUMBO A EUROPA", 2f);
        yield return FlyToLandingArea(7f);

        yield return FadeTo(1f);
        sequenceCamera.transform.SetPositionAndRotation(landingShot.position, landingShot.rotation);
        yield return new WaitForSeconds(0.75f);
        yield return FadeTo(0f);
        yield return ShowSubtitle("Compañero: Vamos. Primero encontramos a los demás.", 3f);

        playerController.transform.SetPositionAndRotation(playerStart.position, playerStart.rotation);
        playerCamera.enabled = true;
        sequenceCamera.enabled = false;
        objectiveText.text = "OBJETIVO\nEncontrá al resto de la unidad";
        playerController.enabled = true;
        subtitleText.text = string.Empty;
    }

    private IEnumerator FlyToLandingArea(float duration)
    {
        Vector3 start = helicopter.position;
        Vector3 destination = new Vector3(65f, start.y - 0.5f, start.z);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            helicopter.position = Vector3.Lerp(start, destination, progress);
            rotor.Rotate(Vector3.up, 900f * Time.deltaTime, Space.Self);
            yield return null;
        }

        helicopter.position = destination;
    }

    private IEnumerator ShowSubtitle(string line, float duration)
    {
        subtitleText.text = line;
        yield return new WaitForSeconds(duration);
        subtitleText.text = string.Empty;
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        float startAlpha = fadeGroup.alpha;
        float elapsed = 0f;
        const float duration = 0.6f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            fadeGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }

        fadeGroup.alpha = targetAlpha;
    }
}
