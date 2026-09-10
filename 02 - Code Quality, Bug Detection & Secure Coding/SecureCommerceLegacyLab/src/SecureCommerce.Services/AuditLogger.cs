namespace SecureCommerce.Services;
public static class AuditLogger {
    public static void Log(string message) {
        var p = Path.Combine(AppContext.BaseDirectory, "logs", "audit.log");
        Directory.CreateDirectory(Path.GetDirectoryName(p)!);
        File.AppendAllText(p, DateTime.UtcNow + " " + message + Environment.NewLine);
    }
}
