using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RRH_APP.Data;
using RRH_APP.Models;

namespace RRH_APP.Controllers;

public class DepartmentsController : Controller
{
    private readonly AppDbContext _context;

    public DepartmentsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: Departments
    public async Task<IActionResult> Index(string? search)
    {
        var query = _context.Departments
            .Include(d => d.Employees)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(d => d.Name.ToLower().Contains(s) || 
                                     d.Code.ToLower().Contains(s) || 
                                     (d.Location != null && d.Location.ToLower().Contains(s)));
        }

        ViewBag.Search = search;
        var departments = await query.OrderBy(d => d.Name).ToListAsync();
        return View(departments);
    }

    // GET: Departments/Details/5
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var department = await _context.Departments
            .Include(d => d.Employees)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (department == null) return NotFound();

        return View(department);
    }

    // GET: Departments/Create
    public IActionResult Create()
    {
        return View(new Department { IsActive = true });
    }

    // POST: Departments/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Department department)
    {
        if (await _context.Departments.AnyAsync(d => d.Code == department.Code))
        {
            ModelState.AddModelError("Code", "Ya existe un departamento con este código.");
        }

        if (await _context.Departments.AnyAsync(d => d.Name == department.Name))
        {
            ModelState.AddModelError("Name", "Ya existe un departamento con este nombre.");
        }

        if (ModelState.IsValid)
        {
            department.Id = Guid.NewGuid();
            department.CreatedAt = DateTime.UtcNow;
            _context.Add(department);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Departamento creado exitosamente.";
            return RedirectToAction(nameof(Index));
        }
        return View(department);
    }

    // GET: Departments/Edit/5
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();

        var department = await _context.Departments.FindAsync(id);
        if (department == null) return NotFound();

        return View(department);
    }

    // POST: Departments/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, Department department)
    {
        if (id != department.Id) return NotFound();

        if (await _context.Departments.AnyAsync(d => d.Code == department.Code && d.Id != id))
        {
            ModelState.AddModelError("Code", "Ya existe otro departamento con este código.");
        }

        if (await _context.Departments.AnyAsync(d => d.Name == department.Name && d.Id != id))
        {
            ModelState.AddModelError("Name", "Ya existe otro departamento con este nombre.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                var existing = await _context.Departments.FindAsync(id);
                if (existing == null) return NotFound();

                existing.Code = department.Code;
                existing.Name = department.Name;
                existing.Description = department.Description;
                existing.Location = department.Location;
                existing.Budget = department.Budget;
                existing.Phone = department.Phone;
                existing.Email = department.Email;
                existing.IsActive = department.IsActive;
                existing.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                TempData["Success"] = "Departamento actualizado exitosamente.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Departments.AnyAsync(e => e.Id == id))
                    return NotFound();
                else
                    throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(department);
    }

    // GET: Departments/Delete/5
    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null) return NotFound();

        var department = await _context.Departments
            .Include(d => d.Employees)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (department == null) return NotFound();

        return View(department);
    }

    // POST: Departments/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var department = await _context.Departments
            .Include(d => d.Employees)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (department != null)
        {
            if (department.Employees.Any())
            {
                TempData["Error"] = "No se puede eliminar el departamento porque tiene empleados asignados. Reasigne o elimine los empleados primero.";
                return RedirectToAction(nameof(Index));
            }

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Departamento eliminado correctamente.";
        }

        return RedirectToAction(nameof(Index));
    }
}
