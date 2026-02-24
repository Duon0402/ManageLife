using ManageLife.Base;
using ManageLife.Entities;
using ManageLife.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.Client
{
    public class AlbumController : WebClientControllerBase
    {
        private readonly IAlbumService _albumService;

        public AlbumController(IAlbumService albumService)
        {
            _albumService = albumService;
        }

        // --- MVC Views ---

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Details(string id)
        {
            var albumResult = await _albumService.GetAlbumAsync(id);
            if (!albumResult.IsOk())
            {
                return RedirectToAction("Index");
            }

            return View(albumResult.Data);
        }

        // --- API Endpoints ---

        [HttpGet]
        public async Task<Result<IEnumerable<AlbumEntity>>> GetAll()
        {
            return await _albumService.GetAllAlbumsAsync();
        }

        [HttpGet]
        public async Task<Result<IEnumerable<FileEntity>>> GetFiles(string albumId)
        {
            return await _albumService.GetAlbumFilesAsync(albumId);
        }

        [HttpPost]
        public async Task<Result<AlbumEntity>> Create([FromForm] string title, [FromForm] string? description, [FromForm] string? coverPhotoId)
        {
            return await _albumService.CreateAlbumAsync(title, description, coverPhotoId);
        }

        [HttpPost]
        public async Task<Result> LinkFile([FromForm] string albumId, [FromForm] string fileId)
        {
            return await _albumService.LinkFileToAlbumAsync(albumId, fileId);
        }

        [HttpPost]
        public async Task<Result> Delete(string id)
        {
            return await _albumService.DeleteAlbumAsync(id);
        }
    }
}
