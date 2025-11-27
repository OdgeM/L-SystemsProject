using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class LSystem : MonoBehaviour
{
    public TextAsset jsonFile;
    public GameObject turtlePrefab;
    public InputActionReference left;
    public InputActionReference right;
    public InputActionReference up;
    public InputActionReference down;
    public InputActionReference scroll;
    public MenuPanel panel;

    public bool is3D = false;

    public float strokeLength = 1.0f;

    private DirectoryInfo configFolder = new DirectoryInfo("Assets/Configs");
    private List<Config> DefaultConfigs;
    private List<Dictionary<string, string>> ruleList = new();
    private List<Dictionary<string, CommandType>> commandList = new();

    private List<GameObject> turtleContainers = new List<GameObject>();
    private List<List<Turtle>> turtles = new List<List<Turtle>>();
    private List<Turtle> activeTurtles = new List<Turtle>();

    private List<Dictionary<bool,Dictionary<string, string>>> ParameterDict = new();

    private int currentIteration = 0;
    private int currentConfig = 0;

    public void Start()
    {
        panel.initialise();
        
        left.action.performed += onMoveIteration;
        right.action.performed += onMoveIteration;

        up.action.performed += onMoveConfig;
        down.action.performed += onMoveConfig;

        LoadDefaultConfigs();
    }

    public void Update()
    {
        float scrollValue = scroll.action.ReadValue<Vector2>().y;

        Camera.main.transform.position = Camera.main.transform.position + new Vector3(0,0,scrollValue);

        if (scrollValue != 0)
            Debug.Log(scrollValue);
        if (is3D)
        {
            turtleContainers[currentConfig].transform.RotateAround(activeTurtles.Last().averagePoint, Vector3.up, 0.1f);
        }
    }


    private string RunIteration(string initialString, Dictionary<string, string> rules)
    {
        bool readingAttribute = false;
        string attribute = "";
        string preceding = "";
        string result = "";


        foreach (char c in initialString)
        {
            string key = c.ToString();

            if (readingAttribute)
            {

                if (key != ")")
                {
                    attribute += key;
                }
                else
                {
                    readingAttribute = false;
                    
                    
                    if (rules.ContainsKey(preceding+"()"))
                    {
                        string rule = rules[preceding + "()"];
                        Regex reg = new Regex(@"(?<=\()[^\(]*(?=\))");
                        rule = reg.Replace(rule, match => MatchEval(match, attribute));
                        result += rule;
                       
                    }
                    else
                    {
                        result += preceding+"("+attribute+")";
                    }
                    attribute = "";
                    preceding = "";
                }
            }
            else
            {
                if (key == "(")
                {
                    readingAttribute = true;
                }
                else
                {
                    if (rules.ContainsKey(preceding))
                    {
                        result += rules[preceding];
                    }
                    else
                        result += preceding;


                    preceding = key;
                    
                }
                

            }
        }
        if (rules.ContainsKey(preceding))
        {
            result += rules[preceding];
        }
        else
            result += preceding;
        return result;
    }

    public string MatchEval(Match m, string attribute)
    {
        string[] attributes = attribute.Split(',');
        string[] rules = m.Value.Split(",");
        string[] outputs = new string[rules.Length];

        for (int i = 0; i < attributes.Length; i++)
        {
            outputs[i] = (float.Parse(attributes[i]) * float.Parse(rules[i])).ToString();
        }
        return String.Join(",", outputs); 
    }

    private List<Turtle> RunSystem(Config config, Dictionary<string, CommandType> commands,Dictionary<string,string> rules, GameObject turtleContainer)
    {

        

        int iterations = config.n;
        List<string> result = new List<string>(iterations + 1);
        List<Turtle> currentTurtles = new List<Turtle>();


        result.Add(config.axiom);

        
        for (int idx = 0; idx < iterations; idx++)
        {
            GameObject newTurtle = Instantiate(turtlePrefab, turtleContainer.transform);
            Turtle turtle = newTurtle.GetComponent<Turtle>();
            turtle.commands = commands;
            turtle.strokeWidth = config.lineWidth;
            //turtle.angle = config.delta;
            currentTurtles.Add(turtle);

            string _result = RunIteration(result[idx], rules);
            turtle.drawSystem(_result);
            turtle.gameObject.SetActive(false);

            result.Add(_result);
        }
        if (currentIteration >= iterations) currentIteration = iterations - 1;

        //turtleContainer.transform.position = -1 * currentTurtles.Last().averagePoint;
        return currentTurtles;
    }



    private void LoadDefaultConfigs()
    {

        FileInfo[] info = configFolder.GetFiles("*.json");
        DefaultConfigs = new List<Config>(info.Length);
        foreach (FileInfo file in info)
        {

            TextAsset _json = new TextAsset(File.ReadAllText(file.FullName));
            Config _config = JsonUtility.FromJson<Config>(_json.text);
            DefaultConfigs.Add(_config);

            Dictionary<string, CommandType> commands = UnwrapCommands(_config);
            commandList.Add(commands);
            ParameterDict.Add(GetParameters(commands));

            Dictionary<string, string> rules = UnwrapRules(_config);
            ruleList.Add(rules);
            
            GameObject turtleContainer = new GameObject();
            turtleContainer.SetActive(false);
            turtleContainers.Add(turtleContainer);

            turtles.Add(RunSystem(_config, commands,rules, turtleContainer));

        }

        turtleContainers[0].SetActive(true);
        activeTurtles = turtles[0];
        activateTurtle(activeTurtles[0]);
        panel.SetConfig(DefaultConfigs[0], commandList[0], ParameterDict[0]);
        panel.changeIteration(0, DefaultConfigs[0].n);
        
    }

    public Dictionary<bool,Dictionary<string, string>> GetParameters(Dictionary<String, CommandType> commands)
    {
        Dictionary<bool,Dictionary<string, string>> output = new();
        output[true] = new();
        output[false] = new();
        

        foreach (string key in commands.Keys) 
        {
            string newKey = key + " : ";
            bool isEditable = true;

            switch (commands[key].type)
            {
                case CommandType.TYPES.DRAW:
                    newKey += "Forward";
                    break;
                case CommandType.TYPES.TURN:
                    newKey += "Turn";
                    break;
                case CommandType.TYPES.ROLL:
                    newKey += "Roll";
                    break;
                case CommandType.TYPES.PITCH:
                    newKey += "Pitch";
                    break;
                case CommandType.TYPES.PUSH:
                    newKey += "Push State";
                    isEditable = false;
                    break;
                case CommandType.TYPES.POP:
                    newKey += "Pop State";
                    isEditable = false;
                    break;
                case CommandType.TYPES.HORIZONTAL:
                    newKey += "Rotate to Vertical";
                    isEditable = false;
                    break;
            }

            output[isEditable][newKey] = key;



        }

        return output;
    }

    public Dictionary<string, string> UnwrapRules(Config cfg)
    {
        List<ProductionRules> rules = cfg.rules;
        Dictionary<string, string> dict = new Dictionary<string, string>();

        foreach (ProductionRules rule in rules)
        {
            dict[rule.key] = rule.value;
        }

        return dict;
    }

    public Dictionary<string, CommandType> UnwrapCommands(Config cfg)
    {
        List<Commands> commands = cfg.commands;
        Dictionary<string, CommandType> dict = new Dictionary<string, CommandType>();

        foreach (Commands command in commands)
        {
            dict[command.key] = command.type;
        }
        return dict;
    }

    public void onMoveIteration(InputAction.CallbackContext callback)
    {
        activeTurtles[currentIteration].gameObject.SetActive(false);

        int prevIteration = currentIteration;

        if (callback.action == left.action) currentIteration--;
        else currentIteration++;


        if (currentIteration >= activeTurtles.Count) currentIteration = activeTurtles.Count - 1;
        else if (currentIteration < 0) currentIteration = 0;

        //activeTurtles[currentIteration].transform.rotation = activeTurtles[prevIteration].transform.rotation;
        //activeTurtles[currentIteration].transform.position = new Vector3(0, 0, 0);


        //[currentIteration].transform.RotateAround(activeTurtles[currentIteration].averagePoint, Vector3.up, activeTurtles[prevIteration].transform.rotation.eulerAngles.y);

        panel.changeIteration(currentIteration, DefaultConfigs[currentConfig].n );    
        activateTurtle(activeTurtles[currentIteration]);
    }

    public void onMoveConfig(InputAction.CallbackContext callback)
    {
        turtleContainers[currentConfig].SetActive(false);
        activeTurtles[currentIteration].gameObject.SetActive(false);

        if (callback.action == up.action) currentConfig++;
        else currentConfig--;

        if (currentConfig >= DefaultConfigs.Count) currentConfig = DefaultConfigs.Count - 1;
        else if (currentConfig < 0) currentConfig = 0;


        turtleContainers[currentConfig].SetActive(true);
        activeTurtles = turtles[currentConfig];



        if (currentIteration >= activeTurtles.Count) currentIteration = activeTurtles.Count - 1;
        panel.SetConfig(DefaultConfigs[currentConfig], commandList[currentConfig], ParameterDict[currentConfig]);
        panel.changeIteration(currentIteration, DefaultConfigs[currentConfig].n);

        activateTurtle(activeTurtles[currentIteration]);
    }

    public void parameterChange(string key, float value)
    {
        if (commandList[currentConfig][key].amount == value)
            return;
        commandList[currentConfig][key].amount = value;
        Debug.Log(commandList[currentConfig][key].amount);

        for (int i = 0 ; i < turtles[currentConfig].Count; i++)
        {
            Destroy(turtles[currentConfig][i].gameObject);
        }

        turtles[currentConfig] = RunSystem(DefaultConfigs[currentConfig], commandList[currentConfig], ruleList[currentConfig], turtleContainers[currentConfig]);

        activeTurtles = turtles[currentConfig];
        activateTurtle(activeTurtles[currentIteration]);

        panel.currentCommands = commandList[currentConfig];

    }

    public void activateTurtle(Turtle turtle)
    {
        if (turtle.span.z > 0)
        {
            is3D = true;
        }
        else is3D = false;

        Turtle lastTurtle = activeTurtles.Last();
        turtle.gameObject.SetActive(true);
        Camera.main.transform.position = lastTurtle.averagePoint - new Vector3(0, 0, 2 * lastTurtle.span.y - lastTurtle.averagePoint.z);
    }
}
