namespace ABT;

public sealed partial class ArchitectureBuilder
{
    public enum EfContext
    {
        Npgsql,
        Mssql,
        Mysql,
        Sqlite
    }
}