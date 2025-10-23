# Editor Localization Toolkit

Localization toolkit for Math Editor.

## Usage

### Localized Strings

You can simplify your code by replacing calls like:
```csharp
Internationalization.GetTranslation("Editor_Plugin_Localization_Demo_Plugin_Name")
```
with:
```csharp
Localize.Editor_Plugin_Localization_Demo_Plugin_Name()
```

If your localization string uses variables, it becomes even simpler! From this:
```csharp
Internationalization.GetTranslation("Editor_Plugin_Localization_Demo_Value_With_Keys"), firstName, lastName);
```
To this:
```csharp
Localize.Editor_Plugin_Localization_Demo_Value_With_Keys(firstName, lastName);
```

If you would like to add summary for functions of localization strings, you need to comment strings in xaml files like this:
```xml
<!--
<summary>Demo plugin name</summary>
-->
<system:String x:Key="Editor_Plugin_Localization_Demo_Plugin_Name">Demo</system:String>
```

Or if you would like to change the default types or names of variables in localization strings, you need to comment strings in xaml file like this:
```xml
<!--
<param index="0" name="value0" type="object" />
<param index="1" name="value1" type="string" />
<param index="2" name="value2" type="int" />
-->
<system:String x:Key="Editor_Plugin_Localization_Demo_Value_With_Keys">Demo {2:00}, {1,-35:D} and {0}</system:String>
```

### Localized Enums

For enum types (e.g., `DemoEnum`) that need localization in UI controls such as combo boxes, use the `EnumLocalize` attribute to enable localization. For each enum field:
- Use `EnumLocalizeKey` to provide a custom localization key.
- Use `EnumLocalizeValue` to provide a constant localization string.

Example:

```csharp
[EnumLocalize] // Enable localization support
public enum DemoEnum
{
    // Specific localization key
    [EnumLocalizeKey("localize_key_1")]
    Value1,

    // Specific localization value
    [EnumLocalizeValue("This is my enum value localization")]
    Value2,

    // Key takes precedence if both are present
    [EnumLocalizeKey("localize_key_3")]
    [EnumLocalizeValue("Localization Value")]
    Value3,

    // Using the Localize class. This way, you can't misspell localization keys, and if you rename
    // them in your .xaml file, you won't forget to rename them here as well because the build will fail.
    [EnumLocalizeKey(nameof(Localize.Editor_Plugin_Localization_Demo_Plugin_Description))]
    Value4,
}
```

Then, use the generated `DemoEnumLocalized` class within your view model to bind to a combo box control:

```csharp
// ComboBox ItemSource
public List<DemoEnumLocalized> AllDemoEnums { get; } = DemoEnumLocalized.GetValues();

// ComboBox SelectedValue
public DemoEnum SelectedDemoEnum { get; set; }
```

In your XAML, bind as follows:

```xml
<ComboBox
    DisplayMemberPath="Display"
    ItemsSource="{Binding AllDemoEnums}"
    SelectedValue="{Binding SelectedDemoEnum}"
    SelectedValuePath="Value" />
```

To update localization strings when the language changes, you can call:

```csharp
DemoEnumLocalize.UpdateLabels(AllDemoEnums);
```
