using UnityEngine;

public class LightingController : MonoBehaviour
{
    public Color surfaceColor;
    public Color surfaceBgColor;
    public Color caveColor;
    public Color caveBgColor;

    public Camera mainCamera;

    public void SetLighting(bool isCave)
    {
        if (isCave)
        {
            RenderSettings.ambientLight = caveColor;
            RenderSettings.subtractiveShadowColor = caveColor;
            if (mainCamera != null)
            {
                mainCamera.backgroundColor = caveBgColor;
            }
        }
        else
        {
            RenderSettings.ambientLight = surfaceColor;
            RenderSettings.subtractiveShadowColor = surfaceColor;
            if (mainCamera != null)
            {
                mainCamera.backgroundColor = surfaceBgColor;
            }
        }
        RenderSettings.fog = !isCave;
    }
}
