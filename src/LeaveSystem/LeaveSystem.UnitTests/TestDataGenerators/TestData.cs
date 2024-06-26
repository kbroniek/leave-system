namespace LeaveSystem.UnitTests.TestDataGenerators;

public static class TestData
{
    public const string FakeRoleAttributeName = "fakeAttrName";
    public const string FakeRolesJson = """
          {
            "Roles": [
              "nulla",
              "aliquip",
              "amet",
              "aliqua",
              "magna",
              "cillum",
              "excepteur"
            ]
          }
          """;
    public const string FakeRolesJsonV2 = """
          {
            "Roles": [
              "enim",
              "ut",
              "et",
              "aliquip",
              "enim",
              "aute",
              "et"
            ]
          }
          """;
    public static readonly string[] FakeRoles = new string[]
    {
        "nulla",
        "aliquip",
        "amet",
        "aliqua",
        "magna",
        "cillum",
        "excepteur"
    };
}
