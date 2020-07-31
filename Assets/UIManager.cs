using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    private static UIManager _instance;

    public RectTransform mainMenu;

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
        mainMenu.DOAnchorPos(Vector2.zero, 0.5f);
    }

    void jumpFrame( GameObject frame, float time)
    {
        
    }

}
