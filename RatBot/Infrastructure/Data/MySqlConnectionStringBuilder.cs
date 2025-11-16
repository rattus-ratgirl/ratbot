namespace RatBot.Infrastructure.Data;

public static class MySqlConnectionStringBuilder
{
    public static string Build(IConfiguration config)
    {
        // Check for complete connection string first
        string? connectionString = config["DB:ConnectionString"];
        if (!string.IsNullOrWhiteSpace(connectionString))
            return connectionString.StartsWith("jdbc:mysql://", StringComparison.OrdinalIgnoreCase)
                ? ConvertJdbcToMySqlConnectionString(connectionString, config)
                : connectionString;

        // Fall back to building from individual components
        string host = config["DB:Host"] ?? throw new InvalidOperationException("DB:Host not configured");
        string port = config["DB:Port"] ?? throw new InvalidOperationException("DB:Port not configured");
        string database = config["DB:Database"] ?? throw new InvalidOperationException("DB:Database not configured");
        string user = config["DB:User"] ?? throw new InvalidOperationException("DB:User not configured");
        string password = config["DB:Password"] ?? throw new InvalidOperationException("DB:Password not configured");

        return $"Server={host};Port={port};Database={database};User Id={user};Password={password};SslMode=Preferred";
    }

    private static string ConvertJdbcToMySqlConnectionString(string jdbcString, IConfiguration config)
    {
        try
        {
            // Parse JDBC string formats:
            // jdbc:mysql://host:port/database?param1=value1&param2=value2
            // jdbc:mysql://user:password@host:port/database
            // jdbc:mysql://host:port/database (credentials separate)

            string withoutPrefix = jdbcString.Replace("jdbc:mysql://", "");

            string host;
            int port = 3306;
            string? user = null;
            string? password = null;
            Dictionary<string, string> queryParams = new Dictionary<string, string>();

            // Check if credentials are in the URL (user:password@host format)
            int atIndex = withoutPrefix.IndexOf('@');
            if (atIndex > 0)
            {
                string credentials = withoutPrefix[..atIndex];
                string[] credentialParts = credentials.Split(':', 2);
                user = Uri.UnescapeDataString(credentialParts[0]);
                if (credentialParts.Length > 1)
                {
                    password = Uri.UnescapeDataString(credentialParts[1]);
                }

                withoutPrefix = withoutPrefix[(atIndex + 1)..];
            }

            // Parse host:port/database?params
            int queryIndex = withoutPrefix.IndexOf('?');
            string hostAndPath;

            if (queryIndex > 0)
            {
                hostAndPath = withoutPrefix[..queryIndex];
                string query = withoutPrefix[(queryIndex + 1)..];

                // Parse query parameters
                foreach (string pair in query.Split('&'))
                {
                    string[] keyValue = pair.Split('=', 2);
                    if (keyValue.Length == 2)
                    {
                        queryParams[keyValue[0]] = Uri.UnescapeDataString(keyValue[1]);
                    }
                }
            }
            else
            {
                hostAndPath = withoutPrefix;
            }

            // Parse host:port/database
            int slashIndex = hostAndPath.IndexOf('/');
            string hostPart = slashIndex > 0 ? hostAndPath[..slashIndex] : hostAndPath;
            string database = slashIndex > 0 ? hostAndPath[(slashIndex + 1)..] : "";

            // Parse host:port
            int colonIndex = hostPart.IndexOf(':');
            if (colonIndex > 0)
            {
                host = hostPart[..colonIndex];
                if (int.TryParse(hostPart[(colonIndex + 1)..], out int parsedPort))
                {
                    port = parsedPort;
                }
            }
            else
            {
                host = hostPart;
            }

            // Get credentials from query parameters if not in URL
            user ??= queryParams.GetValueOrDefault("user");
            password ??= queryParams.GetValueOrDefault("password");

            // Fall back to individual config values if still not found
            user ??= config["DB:User"];
            password ??= config["DB:Password"];

            if (string.IsNullOrEmpty(user))
            {
                throw new InvalidOperationException(
                    "User not found in JDBC connection string. Provide it via DB:User config or in the connection string."
                );
            }

            if (string.IsNullOrEmpty(password))
            {
                throw new InvalidOperationException(
                    "Password not found in JDBC connection string. Provide it via DB:Password config or in the connection string."
                );
            }

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
