using TMPro;
using UnityEngine;

public class HeightUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tmp;

    // Update is called once per frame
    void Update()
    {
        tmp.text = $"{PlayerController.Instance.height} M";
    }
}