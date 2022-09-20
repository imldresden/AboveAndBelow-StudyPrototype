using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public struct StudyCondition
{
    /// <summary>
    /// Either "Floor" or "Ceiling".
    /// </summary>
    public string Area;
    /// <summary>
    /// Either "Icon" or "Text".
    /// </summary>
    public string ContentType;

    public string ParameterName;
    public float ParameterValue;

    public bool Equals(StudyCondition right)
    {
        if (Area != right.Area)
            return false;
        if (ContentType != right.ContentType)
            return false;
        if (ParameterName != right.ParameterName)
            return false;
        if (ParameterValue != right.ParameterValue)
            return false;
        return true;
    }

    public override int GetHashCode() => base.GetHashCode();

    public static bool operator ==(StudyCondition left, StudyCondition right) => left.Equals(right);

    public static bool operator !=(StudyCondition left, StudyCondition right) => !left.Equals(right);

}