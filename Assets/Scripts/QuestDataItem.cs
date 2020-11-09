using Newtonsoft.Json;
using System;
using UnityEngine.Scripting;

[Preserve]
public class QuestDataItem
{
    public string name;
    public QuestFeatures features;
    public int points;
    public string deadline;
    public string description;

    public QuestDataItem()
    {
        name = "";
        features = QuestFeatures.None;
    }

    public QuestDataItem(string _name, QuestFeatures _features)
    {
        name = _name;
        if (_features == QuestFeatures.None)
            features = _features;
    }

    public QuestDataItem(string _name, QuestFeatures _features, int _points)
    {
        name = _name;
        features = _features;
        if (_features == QuestFeatures.Points)
            points = _points;
    }

    public QuestDataItem(string _name, QuestFeatures _features, string _string)
    {
        name = _name;
        features = _features;
        if (_features == QuestFeatures.Description)
            description = _string;
        else if (_features == QuestFeatures.Deadline)
            deadline = _string;
    }

    public QuestDataItem(string _name, QuestFeatures _features, int _points, string _string)
    {
        name = _name;
        features = _features;
        if (_features == (QuestFeatures.Points | QuestFeatures.Description))
        {
            points = _points;
            description = _string;
        }
        else if (_features == (QuestFeatures.Points | QuestFeatures.Deadline))
        {
            points = _points;
            deadline = _string;
        }
    }

    public QuestDataItem(string _name, QuestFeatures _features, string _description, string _deadline)
    {
        name = _name;
        features = _features;
        if (_features == (QuestFeatures.Description | QuestFeatures.Deadline))
        {
            description = _description;
            deadline = _deadline;
        }
    }

    public QuestDataItem(string _name, QuestFeatures _features, int _points, string _description, string _deadline)
    {
        name = _name;
        features = _features;
        if (_features == (QuestFeatures.Description | QuestFeatures.Deadline | QuestFeatures.Points))
        {
            points = _points;
            description = _description;
            deadline = _deadline;
        }
    }

    [Flags]
    public enum QuestFeatures : short
    {
        None = 0,
        Points = 1,
        Description = 2,
        Deadline = 4
    }
}
[Preserve]
public class QuestDataItemConverter : JsonConverter<QuestDataItem>
{
    public override void WriteJson(JsonWriter writer, QuestDataItem quest, JsonSerializer serializer)
    {
        writer.Formatting = Formatting.Indented;
        writer.WriteStartObject();
        writer.WritePropertyName("name");
        writer.WriteValue(quest.name);
        writer.WritePropertyName("type");
        writer.WriteValue(quest.features);
        writer.WritePropertyName("features");
        writer.WriteStartObject();
        if (quest.features.HasFlag(QuestDataItem.QuestFeatures.Points))
        {
            writer.WritePropertyName("points");
            writer.WriteValue(quest.points);
        }
        if (quest.features.HasFlag(QuestDataItem.QuestFeatures.Description))
        {
            writer.WritePropertyName("description");
            writer.WriteValue(quest.description);
        }
        if (quest.features.HasFlag(QuestDataItem.QuestFeatures.Deadline))
        {
            writer.WritePropertyName("deadline");
            writer.WriteValue(quest.deadline);
        }
        writer.WriteEndObject();
        writer.WriteEndObject();
    }

    public override QuestDataItem ReadJson(JsonReader reader, Type objectType, QuestDataItem existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        serializer.CheckAdditionalContent = false;
        reader.Read();
        reader.ReadAsString();
        string name = (string)reader.Value;
        reader.Read();
        reader.ReadAsInt32();
        QuestDataItem.QuestFeatures features = (QuestDataItem.QuestFeatures)(int)reader.Value;
        reader.Read();
        reader.Read();
        if (features.HasFlag(QuestDataItem.QuestFeatures.Points))
        {
            reader.Read();
            reader.ReadAsInt32();
            int points = (int)reader.Value;
            reader.Read();
            reader.Read();
            if (features.HasFlag(QuestDataItem.QuestFeatures.Description) ^ features.HasFlag(QuestDataItem.QuestFeatures.Deadline))
            {
                string _string = (string)reader.Value;
                reader.Read();
                reader.Read();
                return new QuestDataItem(name, features, points, _string);
            }
            else if (features.HasFlag(QuestDataItem.QuestFeatures.Description) && features.HasFlag(QuestDataItem.QuestFeatures.Deadline))
            {
                string description = (string)reader.Value;
                reader.Read();
                reader.ReadAsString();
                string deadline = (string)reader.Value;
                reader.Read();
                reader.Read();
                return new QuestDataItem(name, features, points, description, deadline);
            }
            else return new QuestDataItem(name, features, points);
        }
        else if (features.HasFlag(QuestDataItem.QuestFeatures.Description) ^ features.HasFlag(QuestDataItem.QuestFeatures.Deadline))
        {
            reader.Read();
            reader.ReadAsString();
            string _string = (string)reader.Value;
            reader.Read();
            reader.Read();
            return new QuestDataItem(name, features, _string);
        }
        else if (features.HasFlag(QuestDataItem.QuestFeatures.Description) && features.HasFlag(QuestDataItem.QuestFeatures.Deadline))
        {
            reader.Read();
            reader.ReadAsString();
            string description = (string)reader.Value;
            reader.Read();
            reader.ReadAsString();
            string deadline = (string)reader.Value;
            reader.Read();
            reader.Read();
            return new QuestDataItem(name, features, description, deadline);
        }
        reader.Read();
        reader.Read();
        return new QuestDataItem(name, QuestDataItem.QuestFeatures.None);
    }
}