namespace MIACopilot.Models;

/// <summary>Available user roles in the system.</summary>
public enum UserRole { SuperAdmin, Trainer, Apprentice }

/// <summary>Static session — holds the currently logged-in user.</summary>
public static class Session
{
    public static UserRole Role      { get; set; }
    public static int      UserId   { get; set; }   // ApprenticeId or TrainerId
    public static string   Username { get; set; } = "";
    public static string   FullName { get; set; } = "";

    public static void Clear()
    {
        Role     = UserRole.SuperAdmin;
        UserId   = 0;
        Username = "";
        FullName = "";
    }
}
