﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Vtest94.Data;
using Vtest94.Interfaces;
using Vtest94.Models;
using Vtest94.ViewModels;

namespace Vtest94.Controllers
{
    public class VideoController : Controller
    {
        private readonly IUploadVideoRepository _videoRepository;
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _appDbContext;
        public VideoController(IUploadVideoRepository videoRepository, UserManager<User> userManager, AppDbContext appDbContext)
        {
            _videoRepository = videoRepository;
            _userManager = userManager;
            _appDbContext = appDbContext;
        }

        [HttpGet] // Actually [HttpGet] is set by default we dont have to wirte it explicitly
        [AllowAnonymous]
        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;
            var videos = await _videoRepository.GetAllVideosAsync(searchString);
            return View(videos);
        }

        // Display the form for uploading a new video
        [Authorize]
        public async Task<IActionResult> Create()
        {
            var categories = await _appDbContext.Categories.OrderBy(c => c.Name).ToListAsync();
            var model = new CreateVideoViewModel
            {
                Categories = new SelectList(categories, "Id", "Name")  // Populate the SelectList for categories
            };
            return View(model);
        }

        // Process the form submission for a new video
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(100_000_000)]
        [Authorize]
        public async Task<IActionResult> Create(CreateVideoViewModel videoViewModel)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                   .Select(e => e.ErrorMessage)
                                   .ToList();
                return BadRequest(new { Errors = errors });
                //return View(videoViewModel);
            }

            var user = await _userManager.GetUserAsync(User);  // Get the authenticated user
            if (user == null)
            {
                ModelState.AddModelError("", "User not found. Please login again.");
                return View(videoViewModel);
            }

            if (videoViewModel.VideoFile != null && videoViewModel.VideoFile.Length > 0)
            {
                videoViewModel.Video.UserId = user.Id;  // Set the UserId to the logged user's Id
                await _videoRepository.AddVideoAsync(videoViewModel.Video, videoViewModel.VideoFile, videoViewModel.ThumbnailFrameTime, videoViewModel.SelectedCategoryId.Value);
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ModelState.AddModelError("VideoFile", "Please select a video file to upload.");
                return View(videoViewModel);
            }
        }

        [Authorize]
        public async Task<IActionResult> Profile(string searchString)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge(); // Ensure user is logged in

            var videos = await _videoRepository.GetVideosByUserIdAsync(user.Id); // Fetch videos by user ID
            return View(videos);
        }
    }
}
