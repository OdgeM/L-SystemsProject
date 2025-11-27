using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class MenuPanel : MonoBehaviour
{
    public GameObject titleLabelObj;
    private TextMeshProUGUI titleLabel;

    public TMP_InputField inputField;
    public TMP_Dropdown inputDropdownField;

    public GameObject iterationLabelObj;
    private TextMeshProUGUI iterationLabel;

    public GameObject rulePanel;
    private TextMeshProUGUI ruleText;

    public LSystem system;

    private Dictionary<bool, Dictionary<string, string>> currentParams;
    public Dictionary<string, CommandType> currentCommands;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void initialise()
    {
        titleLabel = titleLabelObj.GetComponent<TextMeshProUGUI>();
        iterationLabel = iterationLabelObj.GetComponent<TextMeshProUGUI>(); 
        ruleText = rulePanel.GetComponent<TextMeshProUGUI>();
        inputField.onEndEdit.AddListener((value) => { ParamChange(float.Parse(value)); });
        inputDropdownField.onValueChanged.AddListener((value) => { OptionChange(value); });
    }

    public void SetConfig(Config cfg, Dictionary<string, CommandType> commands, Dictionary<bool, Dictionary<string,string>> parameterDict)
    {
        currentParams = parameterDict;
        currentCommands = commands;

        titleLabel.text = cfg.name;
        ruleText.text = "Axiom: " + cfg.axiom + "\n\n";

        foreach (ProductionRules rule in cfg.rules)
        {
            string key = rule.key;
            if (rule.key.EndsWith("()")) key = key[0] + "(l,w)";
            ruleText.text += key + "→" + rule.value + "\n"; 
        }

        inputDropdownField.ClearOptions();
        inputDropdownField.AddOptions(parameterDict[true].Keys.ToList());
        OptionChange(0);
        
    }

    public void changeIteration(int iteration, int n)
    {
        iterationLabel.text = "Iteration: " + (iteration+1).ToString() + "/" + n.ToString();
    }


    public void ParamChange(float value)
    {
        string commandKey = currentParams[true].Values.ToList()[inputDropdownField.value];
        system.parameterChange(commandKey, value);
    }

    public void OptionChange(int value) 
    {
        string commandKey = currentParams[true].Values.ToList()[value];
        float paramValue = currentCommands[commandKey].amount;

        inputField.text = paramValue.ToString();
    }
}
