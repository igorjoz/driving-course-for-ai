using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private DriverLearningData _driverLearningData;
    
    public DriverLearningData driverLearningData
    {
        get
        {
            if (_driverLearningData == null)
                LoadDriverDataFromFile(false);

            return _driverLearningData;
        }
    }

    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        LoadDriverDataFromFile(false);
    }

    public static bool TryGetInstance(out GameManager gameManager)
    {
        if(instance == null)
#if UNITY_2023_1_OR_NEWER
            instance = FindFirstObjectByType<GameManager>();
#else
            instance = FindObjectOfType<GameManager>();
#endif

        gameManager = instance;
        return gameManager != null;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
            LoadDriverDataFromFile(true);
    }

    private void LoadDriverDataFromFile(bool logChanges)
    {
        var previousData = _driverLearningData;
        DriverLearningData loadedData;
        bool createdDefaultFile = false;

        try
        {
            if(File.Exists("AI_LearningData.json"))
            {
                using (FileStream fileStream = File.OpenRead("AI_LearningData.json"))
                    loadedData = JsonUtility.FromJson<DriverLearningData>(new StreamReader(fileStream).ReadToEnd());
            }
            else
            {
                loadedData = DriverLearningData.CreateDefault();
                File.WriteAllText("AI_LearningData.json", JsonUtility.ToJson(loadedData, true));
                createdDefaultFile = true;
            }
        }
        catch(System.Exception exception)
        {
            Debug.LogError($"Failed to load AI_LearningData.json. Keeping the previous driver learning data.\n{exception}", this);
            return;
        }

        if(loadedData == null)
        {
            Debug.LogError("Failed to load AI_LearningData.json. JsonUtility returned no driver learning data. Keeping the previous settings.", this);
            return;
        }

        _driverLearningData = loadedData;

        if(logChanges)
            LogDriverDataReload(previousData, loadedData, createdDefaultFile);
    }

    private void LogDriverDataReload(DriverLearningData previousData, DriverLearningData loadedData, bool createdDefaultFile)
    {
        if(createdDefaultFile)
        {
            Debug.Log("AI_LearningData.json was missing. Created the default driver learning data file and applied it.", this);
            return;
        }

        if(previousData == null)
        {
            Debug.Log("Driver learning data loaded from AI_LearningData.json.", this);
            return;
        }

        var changes = new StringBuilder();
        int changeCount = AppendChangedFields(changes, string.Empty, previousData, loadedData);

        if(changeCount == 0)
        {
            Debug.Log("Driver learning data reloaded from AI_LearningData.json. No value changes detected.", this);
            return;
        }

        Debug.Log($"Driver learning data reloaded from AI_LearningData.json. Applied {changeCount} change(s):\n{changes}", this);
    }

    private static int AppendChangedFields(StringBuilder changes, string path, object previousValue, object currentValue)
    {
        if(previousValue == null || currentValue == null)
        {
            if(ValuesAreEqual(previousValue, currentValue))
                return 0;

            changes.AppendLine($"{path}: {FormatValue(previousValue)} -> {FormatValue(currentValue)}");
            return 1;
        }

        var valueType = currentValue.GetType();

        if(IsSimpleValue(valueType))
        {
            if(ValuesAreEqual(previousValue, currentValue))
                return 0;

            changes.AppendLine($"{path}: {FormatValue(previousValue)} -> {FormatValue(currentValue)}");
            return 1;
        }

        int changeCount = 0;
        FieldInfo[] fields = valueType.GetFields(BindingFlags.Instance | BindingFlags.Public);

        foreach(FieldInfo field in fields)
        {
            string fieldPath = string.IsNullOrEmpty(path) ? field.Name : $"{path}.{field.Name}";
            changeCount += AppendChangedFields(changes, fieldPath, field.GetValue(previousValue), field.GetValue(currentValue));
        }

        return changeCount;
    }

    private static bool IsSimpleValue(System.Type type)
    {
        return type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal);
    }

    private static bool ValuesAreEqual(object previousValue, object currentValue)
    {
        if(previousValue == null || currentValue == null)
            return previousValue == currentValue;

        if(previousValue is float previousFloat && currentValue is float currentFloat)
            return Mathf.Approximately(previousFloat, currentFloat);

        if(previousValue is double previousDouble && currentValue is double currentDouble)
            return System.Math.Abs(previousDouble - currentDouble) < 0.000001d;

        return previousValue.Equals(currentValue);
    }

    private static string FormatValue(object value)
    {
        switch(value)
        {
            case null:
                return "null";
            case float floatValue:
                return floatValue.ToString("0.####", CultureInfo.InvariantCulture);
            case double doubleValue:
                return doubleValue.ToString("0.####", CultureInfo.InvariantCulture);
            case bool boolValue:
                return boolValue ? "true" : "false";
            default:
                return value.ToString();
        }
    }
}
