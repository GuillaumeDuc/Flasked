using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class IPInput : MonoBehaviour
{
    public InputField inputField;

    // Start is called before the first frame update
    void Start()
    {
        inputField.onValueChanged.AddListener((s) => { ValueChangeCheck(s); });
    }

    // Invoked when the value of the text field changes.
    public void ValueChangeCheck(string s)
    {
        bool a = Regex.IsMatch(s, "^\\d*(\\d+[\\.d]\\d*)*$");
        if (!a)
        {
            s = s.Remove(s.Length - 1);
            inputField.text = s;
        }
    }
}
