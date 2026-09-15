namespace RRH_APP.Models;

public class DashboardViewModel
{
    public int TotalDepartments { get; set; }
    public int TotalEmployees { get; set; }
    public decimal TotalPayroll { get; set; }
    public decimal AverageSalary { get; set; }
    public List<DepartmentSummaryDto> DepartmentSummaries { get; set; } = new();
    public List<Employee> RecentEmployees { get; set; } = new();
}

public class DepartmentSummaryDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
    public decimal? Budget { get; set; }
    public int EmployeeCount { get; set; }
    public decimal DepartmentPayroll { get; set; }
}
