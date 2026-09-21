using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TwitchTools.Utils;

internal static class ReflectionUtils
{
    private const BindingFlags PrivateFields = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

    public static T GetPrivate<T>(this object instance, string field)
    {
        var privateField = instance.GetType().GetField(field, PrivateFields);
        return (T)(privateField != null ? privateField.GetValue(instance) : null);
    }
        
    /*
     * Courtesy of end_4
     */
    public static IEnumerable<Type> SafeGetTypes(Assembly assembly) {
        try {
            return assembly.GetTypes();
        } catch (ReflectionTypeLoadException ex) {
            return ex.Types.Where(t => t != null)!;
        } catch (Exception)
        {
            return [];
        }
    }
        
    public static bool IsSystemAssembly(Assembly assembly) {
        var name = assembly.FullName;
        if (string.IsNullOrEmpty(name)) return true;

        return name.StartsWith("System") || name.StartsWith("Microsoft") || name.StartsWith("mscorlib") ||
               name.StartsWith("Unity") || name.StartsWith("UnityEngine");
    }
        
    public static bool SafeIsAssignableFrom(Type targetType, Type candidateType) {
        try {
            return targetType.IsAssignableFrom(candidateType);
        } catch (TypeLoadException) {
            return false;
        } catch (Exception) {
            return false;
        }
    }
}