namespace LeaveSystem.Shared;

[System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
public enum LeaveTypeCatalog
{
    Holiday,
    OnDemand,
    Saturday,
    Sick
}
