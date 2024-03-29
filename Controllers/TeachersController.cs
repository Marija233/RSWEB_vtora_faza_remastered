﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project.Data;
using Project.Models;
using Project.ViewModels;

namespace Project.Controllers
{
    public class TeachersController : Controller
    {
        private readonly ProjectContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public TeachersController(ProjectContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Teachers
        public async Task<IActionResult> Index(string TeacherAcademicRank, string TeacherDegree, string SearchString)
        {

             
            IEnumerable<Teacher> teachers = _context.Teacher;
            IQueryable<string> RankQuery = _context.Teacher.OrderBy(m => m.AcademicRank).Select(m => m.AcademicRank).Distinct();
            IQueryable<string> DegreeQuery = _context.Teacher.OrderBy(m => m.Degree).Select(m => m.Degree).Distinct();

            if (!string.IsNullOrEmpty(TeacherAcademicRank))
            {
                teachers = teachers.Where(x => x.AcademicRank == TeacherAcademicRank);
            }
            if (!string.IsNullOrEmpty(TeacherDegree))
            {
                teachers = teachers.Where(x => x.Degree == TeacherDegree);
            }

            if (!string.IsNullOrEmpty(SearchString))
            {
                teachers = teachers.Where(s => (s.FirstName + " " + s.LastName).ToLower().Contains(SearchString.ToLower())).ToList();
                // teachers = teachers.Where(s => s.FullName.ToLower().Contains(SearchString.ToLower())); 
            }


            //IQueryable teacher = teachers.AsQueryable();
            // teachers = teachers.Include(m => m.FirstCourses).Include(c => c.SecondCourses).ThenInclude(m => m.Course);

            var TeacherFullNameDegreeAcademicRank = new TeacherFullNameDegreeAcademicRankViewModel
            {
                AcademicRanges = new SelectList(await RankQuery.ToListAsync()),
                Degrees = new SelectList(await DegreeQuery.ToListAsync()),
                Teachers = teachers.ToList()
            };

            return View(TeacherFullNameDegreeAcademicRank);
        }

        // GET: Teachers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Teacher
                .FirstOrDefaultAsync(m => m.Id == id);
            if (teacher == null)
            {
                return NotFound();
            }

            return View(teacher);
        }

        // GET: Teachers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Teachers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TeacherFormVM Vmodel)
        {
            if (ModelState.IsValid)
            {
                string uniqueFileName = UploadedFile(Vmodel);

                Teacher teacher = new Teacher
                {
                    ProfilePicture = uniqueFileName,
                    FirstName = Vmodel.FirstName,
                    LastName = Vmodel.LastName,
                    Degree = Vmodel.Degree,
                    AcademicRank = Vmodel.AcademicRank,
                    OfficeNumber = Vmodel.OfficeNumber,
                    HireDate = Vmodel.HireDate,

                    FirstCourses = Vmodel.FirstCourses,
                    SecondCourses = Vmodel.SecondCourses
                };

                _context.Add(teacher);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View();
        }
        private string UploadedFile(TeacherFormVM Vmodel)
        {
            string uniqueFileName = null;

            if (Vmodel.ProfilePicture != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(Vmodel.ProfilePicture.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    Vmodel.ProfilePicture.CopyTo(fileStream);
                }
            }
            return uniqueFileName;
        }



        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Teacher.FindAsync(id);
            if (teacher == null)
            {
                return NotFound();
            }
            TeacherFormVM Vmodel = new TeacherFormVM
            {
                Id = teacher.Id,
                FirstName = teacher.FirstName,
                LastName = teacher.LastName,
                Degree = teacher.Degree,
                AcademicRank = teacher.AcademicRank,
                OfficeNumber = teacher.OfficeNumber,
                HireDate = teacher.HireDate,
                FirstCourses = teacher.FirstCourses,
                SecondCourses = teacher.SecondCourses
            };
            ViewData["TeacherFullName"] = _context.Teacher.Where(t => t.Id == id).Select(t => t.FullName).FirstOrDefault();

            return View(Vmodel);
        }

        // POST: Teachers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TeacherFormVM Vmodel)
        {
            if (id != Vmodel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    string uniqueFileName = UploadedFile(Vmodel);

                    Teacher teacher = new Teacher
                    {
                        ProfilePicture = uniqueFileName,
                        Id = Vmodel.Id,
                        FirstName = Vmodel.FirstName,
                        LastName = Vmodel.LastName,
                        Degree = Vmodel.Degree,
                        AcademicRank = Vmodel.AcademicRank,
                        OfficeNumber = Vmodel.OfficeNumber,
                        HireDate = Vmodel.HireDate,
                        FirstCourses = Vmodel.FirstCourses,
                        SecondCourses = Vmodel.SecondCourses
                    };

                    _context.Update(teacher);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeacherExists(Vmodel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(Vmodel);
        }

        // GET: Teachers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Teacher
                .FirstOrDefaultAsync(m => m.Id == id);
            if (teacher == null)
            {
                return NotFound();
            }

            return View(teacher);
        }

        // POST: Teachers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
       
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var teacher = await _context.Teacher.FindAsync(id);
            string path = Path.Combine(_webHostEnvironment.WebRootPath, "images", teacher.ProfilePicture);
            FileInfo file = new FileInfo(path);
            if (file.Exists)
            {
                file.Delete();
            }

            _context.Teacher.Remove(teacher);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TeacherExists(int id)
        {
            return _context.Teacher.Any(e => e.Id == id);
        }
        // GET: Teachers/Courses/
        public async Task<IActionResult> Courses(int id)
        {
           
            var courses = _context.Course.Where(c => c.FirstTeacherID == id || c.SecondTeacherID == id);
            courses = courses.Include(t => t.FirstTeacher).Include(t => t.SecondTeacher);

            ViewData["TeacherId"] = id;
            ViewData["TeacherAcademicRank"] = _context.Teacher.Where(t => t.Id == id).Select(t => t.AcademicRank).FirstOrDefault();
            ViewData["TeacherFullName"] = _context.Teacher.Where(t => t.Id == id).Select(t => t.FullName).FirstOrDefault();
            return View(courses);
        }

    }
}
