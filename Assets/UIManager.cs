using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
//using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    private static UIManager _instance;

    public RectTransform mainMenu;
    public Image background;

    void Awake()
    {
        //singleton patern
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this);
        }

    }

    private void Start()
    {
        //on start transition
        // mainMenu.DOAnchorPos(Vector2.zero, 0.5f);
        background.DOColor(Color.white, 1);

    }

    void jumpFrame( GameObject frame, float time)
    {

    }

}
