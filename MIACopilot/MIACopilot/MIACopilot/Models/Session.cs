namespace MIACopilot.Models;

/// <summary>
/// Available user roles in the system.
/// Determines access rights and UI behavior.
/// </summary>
public enum UserRole
{
    SuperAdmin,    // Full system access (administration)
    Trainer,       // Vocational trainer access (read-only)
    Apprentice,    // Apprentice (learner) access
    CompanyAdmin   // Company-scoped admin with CRUD for own company
}

/// <summary>
/// Static session class holding information
/// about the currently logged-in user.
/// </summary>
public static class Session
{
    // Role of the current user (Admin / Trainer / Apprentice)
    public static UserRole Role { get; set; }

    // ID of the logged-in user (ApprenticeId or TrainerId)
    public static int UserId { get; set; }

    // For CompanyAdmin: the company they administer
    public static int CompanyId { get; set; }

    // Username used for login
    public static string Username { get; set; } = "";

    // Full name of the logged-in user
    public static string FullName { get; set; } = "";

    /// <summary>
    /// Clears the session and resets all values.
    /// Typically called on logout.
    /// </summary>
    public static void Clear()
    {
        Role      = UserRole.SuperAdmin;
        UserId    = 0;
        CompanyId = 0;
        Username  = "";
        FullName  = "";
    }
}