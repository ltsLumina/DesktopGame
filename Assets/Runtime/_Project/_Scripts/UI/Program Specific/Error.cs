using MelenitasDev.SoundsGood;
using UnityEngine;

public class Error : PopUp
{
    public override void Start()
    {
        base.Start();

        title = "Error";

        var rect = GetComponent<RectTransform>();
        rect.SetAnchoredPosition(new (Random.Range(-250, 250), Random.Range(-250, 250)));

        var errorSFX = new Sound(SFX.error);
        errorSFX.Play();
    }

    public void OK()
    {
        Close();
    }
}
