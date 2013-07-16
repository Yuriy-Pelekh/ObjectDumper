using System;
using System.Collections;
using System.Text;

namespace Dumper
{
  public class MessageDumper
  {
    public MessageDumper()
    {
      DeepLevel = 10;
      IsArrayProcessing = true;
    }

    public MessageDumper(int deepLavel, bool isArrayProcessing)
    {
      DeepLevel = deepLavel;
      IsArrayProcessing = isArrayProcessing;
    }

    public int DeepLevel { get; set; }

    public bool IsArrayProcessing { get; set; }

    public string Dump(object message, int level = 0)
    {
      var type = message.GetType();
      var properties = type.GetProperties();

      var headerTab = GetLevelTab(level);
      var propertyTab = GetLevelTab(level + 1);

      if (level > DeepLevel)
      {
        return ComposePropertyInfoString(headerTab, type.Name, "... (max deep level reached)");
      }

      var dump = new StringBuilder();
      dump.Append(ComposePropertyHeaderInfoString(headerTab, type.Name));

      foreach (var propertyInfo in properties)
      {
        var name = propertyInfo.Name;
        var value = propertyInfo.GetValue(message, null);

        if (IsNeedGoDeeper(value))
        {
          dump.Append(Dump(value, level + 1));
        }
        else if (value != null && IsArrayOrCollection(value))
        {
          if (IsArrayProcessing)
          {
            dump.Append(ComposePropertyInfoString(propertyTab, name, string.Empty));
            dump.Append(DumpArray(value, level + 1));
          }
          else
          {
            dump.Append(ComposePropertyInfoString(headerTab, type.Name, "... (array processing disabled)"));
          }
        }
        else
        {
          dump.Append(ComposePropertyInfoString(propertyTab, name, value));
        }
      }

      return dump.ToString();
    }

    private string DumpArray(object value, int level)
    {
      var dump = new StringBuilder();

      if (value is IEnumerable)
      {
        var enumerable = value as IEnumerable;

        foreach (var variable in enumerable)
        {
          dump.Append(IsNeedGoDeeper(variable)
                        ? Dump(variable, level + 1)
                        : ComposePropertyHeaderInfoString(GetLevelTab(level + 1), variable));
        }
      }

      return dump.ToString();
    }

    private bool IsNeedGoDeeper(object propertyValue)
    {
      if (propertyValue != null)
      {
        var valueType = propertyValue.GetType();

        if (valueType.Namespace != "System" && !valueType.IsEnum && !IsArrayOrCollection(propertyValue))
        {
          return true;
        }
      }

      return false;
    }

    private string GetLevelTab(int level)
    {
      var tab = new StringBuilder();

      for (var i = 0 ; i< level;i++)
      {
        tab.Append("\t");
      }

      return tab.ToString();
    }

    private bool IsArrayOrCollection(object value)
    {
      return value != null && value is IEnumerable && !(value is string);
    }

    private string ComposePropertyInfoString(string propertyTab, string propertyName, object propertyValue)
    {
      return String.Format("{0}{1}: {2}{3}", propertyTab, propertyName, propertyValue, Environment.NewLine);
    }

    private string ComposePropertyHeaderInfoString(string tab, object variable)
    {
      return String.Format("{0}{1}{2}", tab, variable, Environment.NewLine);
    }
  }
}
