using TMPro;
using UnityEngine;

public class VariableField : MonoBehaviour
{
    public GameObject nameObj;
    public GameObject fieldObj;

    public string variable;
    public string variableName;
    public float value;

    private TextMeshProUGUI nameText;
    private TMP_InputField inputField;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Initialise()
    {
        nameText = nameObj.GetComponent<TextMeshProUGUI>();
        inputField = nameObj.GetComponent<TMP_InputField>();    
    }

    public void setVariable(string _var, string _name, float _value)
    { 

        variable = _var;
        variableName = _name;
        value = _value;
        Debug.Log(inputField);
        nameText.text = variableName;
        inputField.text = value.ToString();
    }
}
