using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New MovementCard", menuName = "Quesix/new MovementCard")]
public class MovementCard : ScriptableObject
{
    [SerializeField] private string titulo = string.Empty;
    public string Titulo { get { return titulo; } }

    [SerializeField] private Sprite arrow;
    public Sprite Arrow { get { return arrow; } }

    [SerializeField] private int id;
    public int ID { get { return id; } }
}
