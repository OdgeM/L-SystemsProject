using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Line : MonoBehaviour
{
    public LineRenderer lineRenderer;
    private List<Vector3> points = new List<Vector3>();
    private Dictionary<float, float> widths = new Dictionary<float, float>(); 
    private AnimationCurve widthCurve = new();
    private float totalLength = 0;
    public void UpdateLine(Vector3 position, float? width = null, float? segmentLength = null)
    {
        if (points.Count == 0)
        {
            points = new List<Vector3>();
        }
        if (width != null)
        {
            totalLength += segmentLength.Value;
            widths[totalLength] = width.Value;
        }
        SetPoint(position);

        

    }

    public void SetPoint(Vector3 point)
    {
        

        //Vector3 adjPoint = new Vector3(point.y, point.x, point.z);
        points.Add(point);
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPosition(points.Count - 1, point);

        
       
    }

    public void finaliseWidth()
    {
        if (widths.Count > 1)
        {
            
            foreach (float length in widths.Keys)
            {
                widthCurve.AddKey((float)(length) / (totalLength), widths[length]);
            }


            lineRenderer.widthCurve = widthCurve;
            
        }
        else if (widths.Count == 1)
        {
            widthCurve.AddKey(0, widths[0]);
            widthCurve.AddKey(1, widths[0]);
            lineRenderer.widthCurve = widthCurve;   
        }

    }
}
