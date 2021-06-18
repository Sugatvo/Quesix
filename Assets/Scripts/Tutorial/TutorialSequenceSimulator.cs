using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TutorialSequenceSimulator : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI CardCountText;
    [SerializeField] TextMeshProUGUI LeftCountText;
    [SerializeField] TextMeshProUGUI FowardCountText;
    [SerializeField] TextMeshProUGUI BackCountText;
    [SerializeField] TextMeshProUGUI RightCountText;

    private static TutorialSequenceSimulator _instance;
    public static TutorialSequenceSimulator Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    public void SimulateSequence()
    {
        StartCoroutine(GenerateSequence());
    }

    IEnumerator GenerateSequence()
    {
        int count = 1;
        while (count <= 16)
        {
            if(count == 9)
            {
                ManagerCopiloto.Instance.isFirstProgramming = false;
            }
            Debug.Log("Waiting...Time elapsed: " + count);
            count += 1;
            yield return new WaitForSeconds(1f);
        }

        for(int i = 0; i < int.Parse(CardCountText.text); i++)
        {
            int index = ManagerCopiloto.Instance.tutorialCards[i];
            Instantiate(GetPrefabByIndex(index), ManagerCopiloto.Instance.dropZones[i].transform, false);
            AssignCard(index);
            ManagerCopiloto.Instance.dropZones[i].GetComponent<DropZoneTutorial>().movementAction.text = GetActionByIndex(index);
        }
        CardCountText.text = "0";
        ManagerCopiloto.Instance.ReadyForReview();
    }

    public void AssignCard(int index)
    {
        if (index == 0)
        {
            LeftCountText.text = (int.Parse(LeftCountText.text) - 1).ToString();
        }
        else if (index == 1)
        {
            FowardCountText.text = (int.Parse(FowardCountText.text) - 1).ToString();
        }
        else if (index == 2)
        {
            BackCountText.text = (int.Parse(BackCountText.text) - 1).ToString();
        }
        else
        {
            RightCountText.text = (int.Parse(RightCountText.text) - 1).ToString();
        }
    }

    public GameObject GetPrefabByIndex(int index)
    {
        if (index == 0)
        {
            return ManagerCopiloto.Instance.leftPrefab;
        }
        else if (index == 1)
        {
            return ManagerCopiloto.Instance.fowardPrefab;
        }
        else if (index == 2)
        {
            return ManagerCopiloto.Instance.backPrefab;
        }
        else
        {
            return ManagerCopiloto.Instance.rightPrefab;
        }
    }

    public string GetActionByIndex(int index)
    {
        if (index == 0)
        {
            return "Girar";
        }
        else if (index == 1)
        {
            return "Avanzar";
        }
        else if (index == 2)
        {
           return "Retroceder";
        }
        else
        {
         return "Girar";
        }
    }
}
