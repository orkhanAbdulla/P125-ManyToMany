using FiorelloBack.DAL;
using FiorelloBack.Helpers;
using FiorelloBack.Models;
using KontaktHome.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FiorelloBack.Areas.Manage.Controllers
{
    [Area("Manage")]
    public class FlowerController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        public FlowerController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
     
        public IActionResult Index(int page=1)
        {
            ViewBag.CurrentPage = page;
            ViewBag.TottalPage = Math.Ceiling((decimal)(_context.Flowers.Count())/1);
            List<Flower> flower = _context.Flowers.Include(f => f.FlowerImages).Skip((page-1)*1).Take(4).ToList();
            return View(flower);
        }
        public IActionResult Create()
        {
            ViewBag.Campaigns = _context.Campaigns.ToList();
            ViewBag.Categories = _context.Categories.ToList();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Flower flower)
        {
            ViewBag.Campaigns = _context.Campaigns.ToList();
            ViewBag.Categories = _context.Categories.ToList();
            if (!ModelState.IsValid) return View();
            if (flower.CampaignId==0)
            {
                flower.CampaignId = null;
            }
            flower.FlowerCategories = new List<FlowerCategory>();
            flower.FlowerImages = new List<FlowerImage>();
            foreach (var id in flower.CategoryIds)
            {
                FlowerCategory fCategory = new FlowerCategory()
                {
                    CategoryId = id,
                    FlowerId = flower.Id

                };
                flower.FlowerCategories.Add(fCategory);
            }
            foreach (var image in flower.ImageFilies)
            {
                if (!image.IsValidType("image/"))
                {
                    ModelState.AddModelError("ImageFilies", "Please select the image file");
                    return View();
                }
                if (!image.IsValidSize(200))
                {
                    ModelState.AddModelError("ImageFilies", "You can choose file which size is max 200kb");
                    return View();
                }
                
   
            }
            foreach (var image in flower.ImageFilies)
            {
                FlowerImage fimage = new FlowerImage()
                {
                    Image = image.SavaFile(_env.WebRootPath, "assets/images"),
                    IsMain = flower.FlowerImages.Count < 1 ? true : false,
                    FlowerId = flower.Id
                };
                flower.FlowerImages.Add(fimage);
            }
            _context.Flowers.Add(flower);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Edit(int id)
        {
            ViewBag.Campaigns = _context.Campaigns.ToList();
            ViewBag.Categories = _context.Categories.ToList();
            Flower flower = _context.Flowers.Include(f=>f.FlowerCategories).Include(f=>f.FlowerImages).FirstOrDefault(f => f.Id == id);
            if (flower == null) return NotFound();
            return View(flower);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Flower flower)
        {
            ViewBag.Campaigns = _context.Campaigns.ToList();
            ViewBag.Categories = _context.Categories.ToList();
            Flower exsistedFlower = _context.Flowers.Include(x => x.FlowerImages).Include(fc=>fc.FlowerCategories).FirstOrDefault(x => x.Id == flower.Id);
            if (exsistedFlower == null) return NotFound();
            if (!ModelState.IsValid) return View(exsistedFlower);

            //List<FlowerCategory> removableCategory = exsistedFlower.FlowerCategories.Where(fc => !flower.CategoryIds.Contains(fc.Id)).ToList();
            //exsistedFlower.FlowerCategories.RemoveAll(fc => removableCategory.Any(rc => rc.Id == fc.Id));

            //foreach (var categoryid in flower.CategoryIds)
            //{
            //    FlowerCategory flowerCategory = exsistedFlower.FlowerCategories.FirstOrDefault(fc => fc.CategoryId == categoryid);
            //    if (flowerCategory == null)
            //    {
            //        FlowerCategory fCategory = new FlowerCategory()
            //        {
            //            CategoryId = categoryid,
            //            FlowerId = exsistedFlower.Id
            //        };
            //        exsistedFlower.FlowerCategories.Add(flowerCategory);
            //    }
            //}

            if (flower.ImageFilies!=null)
            {
                foreach (var image in flower.ImageFilies)
                {
                    if (!image.IsValidType("image/"))
                    {
                        ModelState.AddModelError("ImageFilies", "Please select the image file");
                        return View(exsistedFlower);
                    }
                    if (!image.IsValidSize(200))
                    {
                        ModelState.AddModelError("ImageFilies", "You can choose file which size is max 200kb");
                        return View(exsistedFlower);
                    }
                }

                List<FlowerImage> removableImages = exsistedFlower.FlowerImages.Where(fi => !flower.ImageIds.Contains(fi.Id)).ToList();

                exsistedFlower.FlowerImages.RemoveAll(fi=> removableImages.Any(ri=>ri.Id==fi.Id));

                foreach (var item in removableImages)
                {
                    Helper.DeleteFile(_env.WebRootPath, "assets/images",item.Image);
                }

                foreach (var image in flower.ImageFilies)
                {
                    FlowerImage fimage = new FlowerImage()
                    {
                        Image = image.SavaFile(_env.WebRootPath, "assets/images"),
                        IsMain = false,
                        FlowerId = flower.Id
                    };
                    //add database image row
                    exsistedFlower.FlowerImages.Add(fimage);
                }

            }
            _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }

    }
}
