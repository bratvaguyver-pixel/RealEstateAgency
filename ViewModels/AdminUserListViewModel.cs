namespace RealEstateAgency.ViewModels;

public class AdminUserListViewModel
{
    public List<AdminUserItemViewModel> Users { get; set; } = new();
}

public class AdminUserItemViewModel
{
    public string Id { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Roles { get; set; } = string.Empty;

    public DateTime RegistrationDate { get; set; }

    public bool IsActive { get; set; }

    public bool CanChangeStatus { get; set; }
}