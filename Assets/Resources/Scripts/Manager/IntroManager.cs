using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class IntroManager : MonoBehaviour 
{
    #region Public Attributes
    [Header("Intro Attributes")]
    public float fadeDuration;
    public float finalPositionY;
    public float logoMoveSpeed;
    #endregion

    #region Private Attributes
    private Vector3 logoPosition;
    #endregion

    #region References
    [Header("Intro References")]
    public TweenAlpha fade;
    public Transform logo;
    public GameObject logoLabel;
    #endregion

    #region Main Methods
    private void Start () 
    {
        Application.targetFrameRate = 60;

        fade.SetTween(2);
        Invoke("NextScene", fadeDuration);
	}
	
	private void Update () 
    {
	    if(logo.localPosition.y < finalPositionY)
        {
            logoPosition = logo.localPosition;
            logoPosition.y += logoMoveSpeed;
            logo.localPosition = logoPosition;
        }
        else
        {
            if (!logoLabel.activeSelf)
            {
                logoLabel.SetActive(true);
            }
        }
	}
    #endregion

    #region Intro Methods
    private void NextScene()
    {
        fade.image.color = new Color32(0, 0, 0, 0);
        fade.SetTween(1);
        Invoke("ChangeScene", 1 / fade.speed + 1);
    }

    private void ChangeScene()
    {
        Application.LoadLevel(1);
    }
    #endregion
}