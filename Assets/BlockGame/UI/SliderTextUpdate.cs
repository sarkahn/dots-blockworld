using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderTextUpdate : MonoBehaviour
{
    [SerializeField]
    Text _text = default;

    [SerializeField]
    Slider _slider = default;

    private void OnEnable()
    {
        _slider.onValueChanged.AddListener(UpdateText);
        UpdateText(_slider.value);
    }

    void UpdateText(float f)
    {
        _text.text = f.ToString() + " ";
    }

}
