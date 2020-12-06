﻿/** UsersController.cs
 * 
 * TODO
 * 
 * Author: Haran
 * Date: December 6th, 2020
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Notes.Models;

namespace Notes.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationContext _context;

        public UsersController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            /* include the notes in the query, so the number of notes created
             * can be shown */
            return View(await _context.Users.Include(m => m.Notes).ToListAsync());
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(m => m.Notes) // load any notes for this user (if they exist)
                .ThenInclude(n => n.Group) // and load any groups associated with the notes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            // return the Create page
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Username")] User user)
        {
            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            /* It seems that, if the model state is *not* valid, ASP does a 
             * very good job of taking care of that.
             * Instead of erroring out, it looks like it will redirect the
             * user back to the page and display an error. This is done 
             * server-side.
             * This is on top of the validation scripts included by default,
             * which also takes care of this on the client-side. 
             */
            return View(user);
        }

        // Users cannot be edited or deleted, so those routes have been removed

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
