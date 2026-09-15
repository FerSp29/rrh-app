using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RRH_APP.Data;
using RRH_APP.Models;

namespace RRH_APP.Controllers;

public class EmployeesController : Controller
{
    private readonly AppDbContext _context;

    public EmployeesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: Employees
    public async Task<IActionResult> Index(string? search, Guid? departmentId, bool? isActive)
    {
        var query = _context.Employees
            .Include(e => e.Department)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(e => e.FirstName.ToLower().Contains(s) ||
                                     e.LastName.ToLower().Contains(s) ||
                                     e.Document.ToLower().Contains(s) ||
                                     e.Email.ToLower().Contains(s) ||
                                     e.Position.ToLower().Contains(s));
        }

        if (departmentId.HasValue)
        {
            query = query.Where(e => e.DepartmentId == departmentId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(e => e.IsActive == isActive.Value);
        }

        ViewBag.Departments = new SelectList(await _context.Departments.OrderBy(d => d.Name).ToListAsync(), "Id", "Name", departmentId);
        ViewBag.Search = search;
        ViewBag.DepartmentId = departmentId;
        ViewBag.IsActive = isActive;

        var employees = await query.OrderBy(e => e.LastName).ThenBy(e => e.FirstName).ToListAsync();
        return View(employees);
    }

    // GET: Employees/Details/5
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var employee = await _context.Employees
            .Include(e => e.Department)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (employee == null) return NotFound();

        return View(employee);
    }

    // GET: Employees/Create
    public async Task<IActionResult> Create()
    {
        ViewBag.DepartmentId = new SelectList(await _context.Departments.Where(d => d.IsActive).OrderBy(d => d.Name).ToListAsync(), "Id", "Name");
        return View(new Employee { IsActive = true, HireDate = DateOnly.FromDateTime(DateTime.Today) });
    }

    // POST: Employees/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Employee employee)
    {
        if (await _context.Employees.AnyAsync(e => e.Document == employee.Document))
        {
            ModelState.AddModelError("Document", "Ya existe un empleado con este documento.");
        }

        if (await _context.Employees.AnyAsync(e => e.Email == employee.Email))
        {
            ModelState.AddModelError("Email", "Ya existe un empleado con este correo electrónico.");
        }

        if (ModelState.IsValid)
        {
            employee.Id = Guid.NewGuid();
            employee.CreatedAt = DateTime.UtcNow;
            _context.Add(employee);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Empleado registrado exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.DepartmentId = new SelectList(await _context.Departments.Where(d => d.IsActive).OrderBy(d => d.Name).ToListAsync(), "Id", "Name", employee.DepartmentId);
        return View(employee);
    }

    // GET: Employees/Edit/5
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();

        var employee = await _context.Employees.FindAsync(id);
        if (employee == null) return NotFound();

        ViewBag.DepartmentId = new SelectList(await _context.Departments.OrderBy(d => d.Name).ToListAsync(), "Id", "Name", employee.DepartmentId);
        return View(employee);
    }

    // POST: Employees/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, Employee employee)
    {
        if (id != employee.Id) return NotFound();

        if (await _context.Employees.AnyAsync(e => e.Document == employee.Document && e.Id != id))
        {
            ModelState.AddModelError("Document", "Ya existe otro empleado con este documento.");
        }

        if (await _context.Employees.AnyAsync(e => e.Email == employee.Email && e.Id != id))
        {
            ModelState.AddModelError("Email", "Ya existe otro empleado con este correo electrónico.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                var existing = await _context.Employees.FindAsync(id);
                if (existing == null) return NotFound();

                existing.Document = employee.Document;
                existing.FirstName = employee.FirstName;
                existing.LastName = employee.LastName;
                existing.Email = employee.Email;
                existing.Phone = employee.Phone;
                existing.Position = employee.Position;
                existing.Salary = employee.Salary;
                existing.HireDate = employee.HireDate;
                existing.DepartmentId = employee.DepartmentId;
                existing.IsActive = employee.IsActive;
                existing.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                TempData["Success"] = "Empleado actualizado exitosamente.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Employees.AnyAsync(e => e.Id == id))
                    return NotFound();
                else
                    throw;
            }
            return RedirectToAction(nameof(Index));
        }

        ViewBag.DepartmentId = new SelectList(await _context.Departments.OrderBy(d => d.Name).ToListAsync(), "Id", "Name", employee.DepartmentId);
        return View(employee);
    }

    // GET: Employees/Delete/5
    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null) return NotFound();

        var employee = await _context.Employees
            .Include(e => e.Department)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (employee == null) return NotFound();

        return View(employee);
    }

    // POST: Employees/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee != null)
        {
            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Empleado eliminado correctamente.";
        }

        return RedirectToAction(nameof(Index));
    }
}
