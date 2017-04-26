
namespace Sitecore.Support.Utils
{
    using System;
    using System.Reflection;

    public static class ReflectionHelper
    {
        public static void SetValueToPrivateField(Type ofType, object target, string fieldName, object value)
        {
            var field = ofType.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(target, value);
        }

        public static object GetValueOfPrivateField(Type ofType, object target, string fieldName)
        {
            var field = ofType.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            return field.GetValue(target);
        }
    }
}
