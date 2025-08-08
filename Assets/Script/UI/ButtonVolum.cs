using UnityEngine;

public class ButtonVolum : MonoBehaviour
{
    [SerializeField] private GameObject panelSlider;
    [SerializeField] private GameObject panelAbout;
    [SerializeField] private GameObject backgroundButton;
    private bool isslider = false;
    private bool isabout = false;

    public void Start()
    {
        panelSlider.SetActive(false);
        backgroundButton.SetActive(false);
    }
    public void TogglesliderUI()
    {
        isslider = !isslider;

        if (isslider)
        {
            panelSlider.SetActive(true);
        }
        else
        {
            panelSlider.SetActive(false);
        }
    }
    public void ToggleUI()
    {
        isabout = !isabout;
        panelAbout.SetActive(isabout);
        backgroundButton.SetActive(isabout);  // Bật/tắt background khi bật panel
    }

    public void CloseUI()
    {
        isabout = false;
        panelAbout.SetActive(false);
        backgroundButton.SetActive(false);
    }

    // Update is called once per frame
}
