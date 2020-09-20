using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StudentData : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] TextMeshProUGUI studentInfo = null;

    [Header("References")]
    [SerializeField] GameEvents events = null;


    private RectTransform _rect = null;
    public RectTransform Rect
    {
        get
        {
            if (_rect == null)
            {
                _rect = GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
            }
            return _rect;
        }
    }

    private int _userIndex = -1;
    public int UserIndex { get { return _userIndex; } }

    private string id_usuario;
    public string UserID { get { return id_usuario; } }

    private string nombre;
    public string Nombre { get { return nombre; } }

    private string apellido;
    public string Apellido { get { return apellido; } }


    public void UpdateData(string id, string name, string last, int index)
    {
        id_usuario = id;
        nombre = name;
        apellido = last;
        _userIndex = index;
        studentInfo.text = nombre + " " + apellido;
    }


}

