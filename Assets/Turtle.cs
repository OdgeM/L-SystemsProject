using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Turtle : MonoBehaviour
{

    public GameObject linePrefab;
    public Vector3 initialPosition = new Vector3(0,0,0);
    private TurtleOrientation initialOrientation = new TurtleOrientation(new Vector3(0, 1, 0), new Vector3(-1, 0, 0), new Vector3(0,0,1));

    public float strokeLength;

    private Vector3 minimums = new(float.MaxValue, float.MaxValue, float.MaxValue);
    private Vector3 maximums = new(float.MinValue, float.MinValue, float.MinValue);

    private Vector3 totalPoints = new(0, 0,0);
    private int numberPoints = 0;
    public Vector3 averagePoint;
    public Vector3 span;

    public Dictionary<string, CommandType> commands;

    public float strokeWidth = 1f;
    public float angle = 25.7f;

    private TurtlePoint currentTurtlePos;
    private Stack<TurtlePoint> stack;
    private Line activeLine;
    private List<Line> lines;

    public bool is3D = false;
    private void InitialiseTurtle()
    {
        currentTurtlePos = new TurtlePoint(initialPosition, initialOrientation, strokeWidth);
        stack = new Stack<TurtlePoint>();
        NewLine(currentTurtlePos);

    }

    private void NewLine(TurtlePoint pos)
    {
        GameObject newLine = Instantiate(linePrefab, this.transform);
        
        if (activeLine)
        {
            activeLine.finaliseWidth();
        }
        else
        {
            totalPoints += pos.position;
            numberPoints += 1;
        }

            activeLine = newLine.GetComponent<Line>();
        if (pos.lineWidth <= 0)
            activeLine.UpdateLine(pos.position);
        else
        {
            activeLine.UpdateLine(pos.position, pos.lineWidth,0);
        }
            
        
        currentTurtlePos = pos;
    }

    public void drawSystem(string system)
    {
        InitialiseTurtle();
        int idx = 0;
        string attribute = "";
        bool readingAttribute = false;
        CommandType comm = null;

        foreach (char c in system)
        {
            idx++;
            if (!(readingAttribute))
            {
                if (commands.ContainsKey(c.ToString()))
                {
                    char next = new char();
                    if (idx <= system.Count() - 1) next = system[idx];

                    comm = commands[c.ToString()];

                    if (next == '(')
                    {
                        readingAttribute = true;
                    }
                    else
                    {
                        DetermineMove(comm);

                    }


                }
            }
            else
            {
                if (c == ')')
                {
                    string[] componentsStrings = attribute.Split(",");  
                    float[] componentFloats = Array.ConvertAll(componentsStrings, num => float.Parse(num));
                    float amount = componentFloats[0];
                    attribute = "";
                    readingAttribute = false;

                    if (componentsStrings.Length > 1) DetermineMove(comm, amount * comm.amount, componentFloats[1]);
                    else DetermineMove(comm, amount * comm.amount);

                }
                else if (c != '(')
                {
                    attribute += c;
                }
            }

        }

        activeLine.finaliseWidth();
        averagePoint = totalPoints/numberPoints;
        span = maximums - minimums;
    }

    private void DetermineMove(CommandType comm, float? customAmount = null, float? customWidth = null)
    {
        float amount = customAmount ?? comm.amount;
        switch (comm.type)
        {
            case CommandType.TYPES.DRAW:
                HandleForward(amount, customWidth);
                break;
            case CommandType.TYPES.TURN:
            case CommandType.TYPES.ROLL:
            case CommandType.TYPES.PITCH:
                HandleRotation(comm, amount);
                break;
            case CommandType.TYPES.PUSH:
                stack.Push(new TurtlePoint(currentTurtlePos.position, currentTurtlePos.orientation, currentTurtlePos.lineWidth));
                break;
            case CommandType.TYPES.POP:
                NewLine(stack.Pop());
                break;
            case CommandType.TYPES.HORIZONTAL:
                HandleHorizontal();
                break;
        }
    }

    private void HandleRotation(CommandType rotType, float amount)
    {
        float angleRad = Mathf.Deg2Rad * amount;
        float[,] rotationMatrix = { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };

        switch (rotType.type)
        {
            case CommandType.TYPES.TURN:
                rotationMatrix[0,0] = Mathf.Cos(angleRad);
                rotationMatrix[0,1] = Mathf.Sin(angleRad);
                rotationMatrix[1,0] = -Mathf.Sin(angleRad);
                rotationMatrix[1, 1] = Mathf.Cos(angleRad);
                rotationMatrix[2, 2] = 1;
                break;
            case CommandType.TYPES.PITCH:
                rotationMatrix[0, 0] = Mathf.Cos(angleRad);
                rotationMatrix[0, 2] = -Mathf.Sin(angleRad);
                rotationMatrix[2, 0] = Mathf.Sin(angleRad);
                rotationMatrix[2, 2] = Mathf.Cos(angleRad);
                rotationMatrix[1, 1] = 1;
                break;
            case CommandType.TYPES.ROLL:
                rotationMatrix[1, 1] = Mathf.Cos(angleRad);
                rotationMatrix[2, 1] = -Mathf.Sin(angleRad);
                rotationMatrix[1, 2] = Mathf.Sin(angleRad);
                rotationMatrix[2, 2] = Mathf.Cos(angleRad);
                rotationMatrix[0, 0] = 1;
                break;
        }
        currentTurtlePos.orientation = currentTurtlePos.orientation.Rotate(rotationMatrix);
    }

    private void HandleHorizontal()
    {
        TurtleOrientation currentOrientation = currentTurtlePos.orientation;
        Vector3 cross = Vector3.Cross(Vector3.down, currentOrientation.heading);
        currentOrientation.left = cross / cross.magnitude;
        currentOrientation.up = Vector3.Cross(currentOrientation.heading, currentOrientation.left);
    }

    private void HandleForward(float strokeLengthMult, float? width)
    {

        currentTurtlePos.position += currentTurtlePos.orientation.heading * strokeLength * strokeLengthMult;
        
        if (width != null)
        {
            currentTurtlePos.lineWidth = width.Value*strokeWidth;
        }

        minimums.x = Mathf.Min(minimums.x, currentTurtlePos.position.x);
        minimums.y = Mathf.Min(minimums.y, currentTurtlePos.position.y);
        minimums.z = Mathf.Min(minimums.z, currentTurtlePos.position.z);
        maximums.x = Mathf.Max(maximums.x, currentTurtlePos.position.x);
        maximums.y = Mathf.Max(maximums.y, currentTurtlePos.position.y);
        maximums.z = Mathf.Max(maximums.z, currentTurtlePos.position.z);

        totalPoints += currentTurtlePos.position;
        numberPoints += 1;

        if(currentTurtlePos.position.z != 0)
        {
            is3D = true;
        }

        activeLine.UpdateLine(currentTurtlePos.position, currentTurtlePos.lineWidth, strokeLength * strokeLengthMult);

    }
}

public class TurtlePoint
{
    public Vector3 position;
    public TurtleOrientation orientation;
    public float lineWidth;

    public TurtlePoint(Vector3 position, TurtleOrientation orientation, float width)
    {
        this.position = position;
        this.orientation = new TurtleOrientation(orientation);
        this.lineWidth = width;
    }
}

public class TurtleOrientation
{
    public Vector3 heading;
    public Vector3 left;
    public Vector3 up;

    public Vector3[] coordFrame = new Vector3[3];

    public TurtleOrientation(Vector3 heading, Vector3 left, Vector3 up)
    {
        this.heading = heading;
        this.left = left;
        this.up = up;

        coordFrame[0] = heading;
        coordFrame[1] = left;
        coordFrame[2] = up; 
        
    }

    public TurtleOrientation(TurtleOrientation orientation) 
    {
        heading = orientation.heading;
        left = orientation.left;
        up = orientation.up;

        coordFrame[0] = heading;
        coordFrame[1] = left;
        coordFrame[2] = up;



        // hx hy hz
        // lx ly lz
        // ux uy uz
    }

    public TurtleOrientation Rotate(float[,] rotationMatrix) {
        Vector3[] newCoordframe = new Vector3[3];   
        Vector3 newHeading = new(0,0,0);
        Vector3 newLeft = new(0,0,0);
        Vector3 newUp = new(0,0,0);

        newCoordframe[0] = newHeading;
        newCoordframe[1] = newLeft;
        newCoordframe[2] = newUp;


        for (int i = 0; i<3; i++)
        {
            for (int j = 0; j<3; j++)
            {
                for (int k = 0; k < 3; k++)
                {
                    newCoordframe[i][j] += rotationMatrix[i, k] * coordFrame[k][j];
                }
            }
        }
       
        

        return new TurtleOrientation(newCoordframe[0], newCoordframe[1], newCoordframe[2]); 

    }
}