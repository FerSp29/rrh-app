using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RRH_APP.Data;
using RRH_APP.Models;

namespace RRH_APP.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var totalDepartments = await _context.Departments.CountAsync();
        var totalEmployees = await _context.Employees.CountAsync();
        var totalPayroll = await _context.Employees.SumAsync(e => (decimal?)e.Salary) ?? 0m;
        var avgSalary = totalEmployees > 0 ? await _context.Employees.AverageAsync(e => (decimal?)e.Salary) ?? 0m : 0m;

        var deptSummaries = await _context.Departments
            .Select(d => new DepartmentSummaryDto
            {
                Id = d.Id,
                Code = d.Code,
                Name = d.Name,
                Location = d.Location,
                Budget = d.Budget,
                EmployeeCount = d.Employees.Count,
                DepartmentPayroll = d.Employees.Sum(e => e.Salary)
            })
            .OrderByDescending(d => d.EmployeeCount)
            .ToListAsync();

        var recentEmployees = await _context.Employees
            .Include(e => e.Department)
            .OrderByDescending(e => e.CreatedAt)
            .Take(5)
            .ToListAsync();

        var model = new DashboardViewModel
        {
            TotalDepartments = totalDepartments,
            TotalEmployees = totalEmployees,
            TotalPayroll = totalPayroll,
            AverageSalary = avgSalary,
            DepartmentSummaries = deptSummaries,
            RecentEmployees = recentEmployees
        };

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}