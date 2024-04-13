using System;
using System.Collections.Generic;
using System.Reflection;
using Assets.ClonerExample;
using Unity.VisualScripting;
using UnityEngine;
using Object = UnityEngine.Object;

public class ClonerDemo1 : MonoBehaviour 
{
    public List<ValueAnimator> Animators = new List<ValueAnimator>();

    public ClonerComponent ClonerComp;
    public ClonerDistributeCircle DistributorComp;
    public ClonerSelectRange RangeComp;
    public ClonerSetValue SetValueComp;
    public ClonerOffset OffsetComp;
    public ClonerReplicate ReplicateComp;
    public ClonerRenderer RendererComp;

    public void Start()
    {
        var comps = GetComponents(typeof(Component));
        ClonerComp = (ClonerComponent)comps[0];
        DistributorComp = (ClonerDistributeCircle)comps[1];
        RangeComp = (ClonerSelectRange)comps[2];
        SetValueComp = (ClonerSetValue)comps[3];
        OffsetComp = (ClonerOffset)comps[4];
        ReplicateComp = (ClonerReplicate)comps[5];
        RendererComp = (ClonerRenderer)comps[6];

        Animators.Clear();
        ToggleBoolean(0.5f, ReplicateComp, "enabled", true);
        foreach (var a in Animators)
            a.Reset();
    }

    public void Update()
    {
        foreach (var a in Animators)
        {
            a.Update(Time.time);
        }
    }

    public object FloatInterpolator(object x, object y, float t) => Mathf.Lerp((float)x, (float)y, t);
    public object IntInterpolator(object x, object y, float t) => (int)Mathf.Lerp((int)x, (int)y, t);

    public float AddAnimation(float curTime, float duration, Component comp, string field, object from, object to, Func<object, object, float, object> interpolator)
    {
        var a = new ValueAnimator();
        a.StartValue = from;
        a.EndValue = to;
        a.Duration = duration;
        a.Interpolator = interpolator;
        a.FieldName = field;
        a.Component = comp;
        return curTime + duration;
    }

    public float AnimateFloat(float curTime, float duration, Component comp, string field, float from, float to)
    {
        return AddAnimation(curTime, duration, comp, field, from, to, FloatInterpolator);
    }

    public float AnimateInt(float curTime, float duration, Component comp, string field, int from, int to)
    {
        return AddAnimation(curTime, duration, comp, field, from, to, IntInterpolator);
    }

    public float ToggleBoolean(float curTime, Component comp, string field, bool onOrOff)
    {
        return AddAnimation(curTime, 0, comp, field, !onOrOff, onOrOff, null);
    }
}

public class ValueAnimator
{
    public object StartValue;
    public object EndValue;
    public Func<object, object, float, object> Interpolator; 
    public Component Component;
    public Type Type;
    public string FieldName;
    public float StartAt;
    public float EndAt => StartAt + Duration;
    public float Duration;
    public FieldInfo FieldInfo;
    public PropertyInfo PropertyInfo;

    public void Start()
    {
        Type = Component.GetType();
        FieldInfo = Type.GetField(FieldName);
        if (FieldInfo == null)
        {
            PropertyInfo = Type.GetProperty(FieldName);
            if (PropertyInfo == null)
            {
                throw new Exception("Could not find field or property");
            }
        }
    }

    public void Reset()
    {
        SetValue(StartValue);
    }

    public void SetValue(object value)
    {
        if (FieldInfo != null)
            FieldInfo.SetValue(Component, value);
        else if (PropertyInfo != null)
            PropertyInfo.SetValue(Component, value);
    }

    public void Update(float t)
    {
        if (t >= EndAt)
        {
            SetValue(EndValue);
            return;
        }

        if (t < StartAt)
        {
            SetValue(StartValue);
            return;
        }

        var amount = (t - StartAt) / Duration;
        var value = Interpolator == null ? StartValue : Interpolator(StartValue, EndValue, amount);
        SetValue(value);
    }
}