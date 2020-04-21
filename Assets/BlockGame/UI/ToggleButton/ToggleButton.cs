using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


public class ToggleButton : TextElement, INotifyValueChanged<bool>
{
    bool _isPressed = false;
    bool _mouseOver = false;

    public bool value
    {
        get => _isPressed;
        set
        {
            _isPressed = value;
            UpdateState();
        }
    }
    
    public void SetValueWithoutNotify(bool newValue)
    {
        _isPressed = newValue;
    }

    public ToggleButton()
    {

        text = "ToggleButton";
        var styleSheet = Resources.Load<StyleSheet>("ToggleButtonStyles");
        styleSheets.Add(styleSheet);

        RegisterCallback<MouseDownEvent>(e =>
        {
            _isPressed = !_isPressed;
            UpdateState();
        });

        RegisterCallback<MouseEnterEvent>(e =>
           {
               _mouseOver = true;
               UpdateState();
           });

        RegisterCallback<MouseLeaveEvent>(e =>
            {
                _mouseOver = false;
                UpdateState();
            });
    }

    void UpdateState()
    {
        ClearClassList();

        if (_isPressed)
        {
            AddToClassList("buttonPressed");
            return;
        }

        if(_mouseOver)
        {
            AddToClassList("buttonHover");
        }
        
    }

}
