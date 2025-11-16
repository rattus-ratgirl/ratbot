namespace RatBot.Infrastructure.Data;

public static class MySqlConnectionStringBuilder
{
    public static string Build(IConfiguration config)
    {
        string? connectionString = config["DB:ConnectionString"];
        
        // If connection string is provided, use it directly
        if (!string.IsNullOrWhiteSpace(connectionString))
            return connectionString.StartsWith("jdbc:mysql://", StringComparison.OrdinalIgnoreCase)
                ? ConvertJdbcToMySqlConnectionString(connectionString)
                : connectionString;

        // Fall back to building from individual components
        string host = config["DB:Host"] ?? throw new InvalidOperationException("DB:Host not configured");
        string port = config["DB:Port"] ?? throw new InvalidOperationException("DB:Port not configured");
        string database = config["DB:Database"] ?? throw new InvalidOperationException("DB:Database not configured");
        string user = config["DB:User"] ?? throw new InvalidOperationException("DB:User not configured");
        string password = config["DB:Password"] ?? throw new InvalidOperationException("DB:Password not configured");

        return $"Server={host};Port={port};Database={database};User Id={user};Password={password};SslMode=Preferred";
    }

    private static string ConvertJdbcToMySqlConnectionString(string jdbcString)
    {
        try
        {
            // Parse JDBC string: jdbc:mysql://host:port/database?param1=value1&param2=value2
            string withoutPrefix = jdbcString.Replace("jdbc:mysql://", "https://"); // Hack to use Uri parser
            Uri uri = new Uri(withoutPrefix);

            string host = uri.Host;
            int port = uri.Port > 0 ? uri.Port : 3306;
            string database = uri.AbsolutePath.TrimStart('/');

            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(uri.Query))
            {
                string query = uri.Query.TrimStart('?');
                foreach (string pair in query.Split('&'))
                {
                    string[] keyValue = pair.Split('=', 2);
                    if (keyValue.Length == 2)
                    {
                        queryParams[keyValue[0]] = Uri.UnescapeDataString(keyValue[1]);
                    }
                }
            }

            string user =
                queryParams.GetValueOrDefault("user")
                ?? throw new InvalidOperationException("User not found in JDBC connection string");

            string password =
                queryParams.GetValueOrDefault("password")
                ?? throw new InvalidOperationException("Password not found in JDBC connection string");

            // Map SSL settings
            string sslMode = "Preferred";
            if (
                queryParams.TryGetValue("useSSL", out string? useSSL)
                && useSSL.Equals("false", StringComparison.OrdinalIgnoreCase)
            )
            {
                sslMode = "None";
            }
            else if (
                queryParams.TryGetValue("requireSSL", out string? requireSSL)
                && requireSSL.Equals("true", StringComparison.OrdinalIgnoreCase)
            )
            {
                sslMode = "Required";
            }

            return $"Server={host};Port={port};Database={database};User Id={user};Password={password};SslMode={sslMode}";
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to parse JDBC connection string: {ex.Message}", ex);
        }
    }
}
