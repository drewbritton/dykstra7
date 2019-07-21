using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ContosoUniversity.Data;
using ContosoUniversity.Models;

namespace ContosoUniversity.Controllers {

    public class StudentsController : Controller {

        private readonly SchoolContext _context;

        public StudentsController(SchoolContext context) {
            _context = context;
        }

        // GET: Students
        public async Task<IActionResult> Index(string sortOrder) {
            // default code for Index:
            //return View(await _context.Students.ToListAsync());

            // the sortOrder parameter allows a query string to be derived from the app's URL (either "Name" or "Date")
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            var students = from s in _context.Students select s;

            switch (sortOrder) {
                case "name_desc":
                    students = students.OrderByDescending(s => s.LastName);
                    break;
                case "Date":
                    students = students.OrderBy(s => s.EnrollmentDate);
                    break;
                case "date_desc":
                    students = students.OrderByDescending(s => s.EnrollmentDate);
                    break;
                default:
                    students = students.OrderBy(s => s.LastName);
                    break;
            }

            return View(await students.AsNoTracking().ToListAsync());
        }

        // GET: Students/Details/5
        public async Task<IActionResult> Details(int? id) {
            if (id == null) {
                return NotFound();
            }

            var student = await _context.Students
                // make a call to Include() on the context, so that the Enrollments class can be used as a nav property
                .Include(s => s.Enrollments).ThenInclude(e => e.Course).AsNoTracking()  // AsNoTracking() may help overall app performance in cases where the data entity isn't likely to be updated any time soon 
                .FirstOrDefaultAsync(m => m.ID == id);


            if (student == null) {
                return NotFound();
            }

            return View(student);
        }

        // GET: Students/Create
        public IActionResult Create() {
            return View();
        }

        // POST: Students/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EnrollmentDate, FirstMidName, LastName")] Student student) {

            try {
                if (ModelState.IsValid) {
                    _context.Add(student);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            } catch (DbUpdateException ex) {
                // log the error (by uncommenting ex var name and write to a log)
                ModelState.AddModelError("", "Unable to save changes. " + "Try again--if the problem persists\ncontact your system administrator.");
            }
            
            return View(student);
        }

        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(int? id) {
            if (id == null) {
                return NotFound();
            }

            var student = await _context.Students.FindAsync(id);
            if (student == null) {
                return NotFound();
            }
            return View(student);
        }

        // POST: Students/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, [Bind("ID,LastName,FirstMidName,EnrollmentDate")] Student student) {
            // default code for edit action method:

            //if (id != student.ID) {
            //    return NotFound();
            //}

            //if (ModelState.IsValid) {
            //    try {
            //        _context.Update(student);
            //        await _context.SaveChangesAsync();
            //    } catch (DbUpdateConcurrencyException) {
            //        if (!StudentExists(student.ID)) {
            //            return NotFound();
            //        } else {
            //            throw;
            //        }
            //    }
            //    return RedirectToAction(nameof(Index));
            //}
            //return View(student);

            // recommended implementation for Edit:
            if (id == null) {
                return NotFound();
            }

            var studentToUpdate = await _context.Students.FirstOrDefaultAsync(s => s.ID == id);

            if (await TryUpdateModelAsync<Student>(studentToUpdate, "", s => s.FirstMidName, s => s.LastName, s => s.EnrollmentDate)) {
                try {
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                } catch (DbUpdateException ex) {
                    // log the error (by uncommenting ex var name and write to a log)
                    ModelState.AddModelError("", "Unable to save changes. " + "Try again--if the problem persists\ncontact your system administrator.");
                }
            }

            return View(studentToUpdate);
        }

        // GET: Students/Delete/5
        public async Task<IActionResult> Delete(int? id, bool? saveChangesError = false) {
            if (id == null) {
                return NotFound();
            }

            var student = await _context.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);

            if (student == null) {
                return NotFound();
            }

            // provides better functionality for displaying errors related to the GET request
            if (saveChangesError.GetValueOrDefault()) {
                ViewData["ErrorMessage"] = "Delete failed, please try again. If the problem persists\ncontact your system administrator.";
            }

            return View(student);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) {
            var student = await _context.Students.FindAsync(id);

            // ensure that a Student object exists
            if (student == null) {
                return RedirectToAction(nameof(Index));
            }

            try {
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            } catch (DbUpdateException ex) {
                // log the error (by uncommenting ex var name and create a log)
                return RedirectToAction(nameof(Delete), new { id = id, saveChangesError = true });
            }

            // default code for POST Delete method:
            //_context.Students.Remove(student);
            //await _context.SaveChangesAsync();
            //return RedirectToAction(nameof(Index));
        }

        private bool StudentExists(int id) {
            return _context.Students.Any(e => e.ID == id);
        }
    }
}
