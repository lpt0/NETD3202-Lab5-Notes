﻿/** NotesController.cs
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
    public class NotesController : Controller
    {
        // the context is what allows the controller to communicate with the database
        private readonly ApplicationContext _context;

        public NotesController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: Notes
        public async Task<IActionResult> Index()
        {
            var applicationContext = _context.Notes.Include(n => n.Group).Include(n => n.User);
            return View(await applicationContext.ToListAsync());
        }

        // GET: Notes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var note = await _context.Notes
                .Include(n => n.Group)
                .Include(n => n.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (note == null)
            {
                return NotFound();
            }

            return View(note);
        }

        // GET: Notes/Create
        public IActionResult Create()
        {
            // populate the group and user select lists
            ViewData["GroupId"] = new SelectList(_context.Groups, "Id", "Name");
            // if there are items in the list, prepend a blank item
            if ((ViewData["GroupId"] as SelectList).Count() != 0)
            {
                // create the option itself, and have it be selected by default
                SelectListItem option = new SelectListItem("", "", true);

                // and prepend it
                (ViewData["GroupId"] as SelectList).Prepend(option); //TODO
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Username");
            return View();
        }

        // POST: Notes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Content,Description,UserId,GroupId")] Note note)
        {
            // inputted data is valid?
            if (ModelState.IsValid)
            {
                _context.Add(note); // add the note
                await _context.SaveChangesAsync(); // wait for the database write to complete
                return RedirectToAction(nameof(Index)); // send the user back to the note list
            }
            ViewData["GroupId"] = new SelectList(_context.Groups, "Id", "Name", note.GroupId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Username", note.UserId);
            return View(note);
        }

        // GET: Notes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var note = await _context.Notes.FindAsync(id);
            if (note == null)
            {
                return NotFound();
            }
            ViewData["GroupId"] = new SelectList(_context.Groups, "Id", "Name", note.GroupId);
            // removed the user id from view data, since I don't want that to be editable
            return View(note);
        }

        // POST: Notes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // Even though the UserId field is not modified, it still needs to be bound to
        //TODO
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Content,Description,GroupId")] Note note)
        {
            if (id != note.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // so, I had to get some help for this one
                    // this stackoverflow answer helped me find the solution below: https://stackoverflow.com/a/35966721
                    // from the way that answer explained it, it seems that the solution below may not be good performance-wise
                    // this would require more research time, which I do not have right now
                    // specify the fields to update in the database
                    // workaround for ASP trying to modify the user id that doesn't exist on the passed model
                    var entry = _context.Entry(note);

                    /* only need to update the bound properties specified in 
                     * the parameters
                     * since the Id is the primary key, it cannot be updated
                     */
                    entry.Property(n => n.Name).IsModified = true;
                    entry.Property(n => n.Description).IsModified = true;
                    entry.Property(n => n.Content).IsModified = true;
                    entry.Property(n => n.GroupId).IsModified = true;

                    // since the properties being modified are specified, no need call update; just save
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NoteExists(note.Id))
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
            ViewData["GroupId"] = new SelectList(_context.Groups, "Id", "Name", note.GroupId);
            // again, no user id since I don't want it to be edited TODO
            return View(note);
        }

        // GET: Notes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var note = await _context.Notes
                .Include(n => n.Group)
                .Include(n => n.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (note == null)
            {
                return NotFound();
            }

            return View(note);
        }

        // POST: Notes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var note = await _context.Notes.FindAsync(id);
            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NoteExists(int id)
        {
            return _context.Notes.Any(e => e.Id == id);
        }
    }
}
