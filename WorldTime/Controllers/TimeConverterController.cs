using Microsoft.AspNetCore.Mvc;
using WorldTime.Models;

namespace WorldTime.Controllers
{
    public class TimeConverterController : Controller
    {
        private List<TimeZoneOption> GetZoneOptions()
        {
            return TimeZoneInfo.GetSystemTimeZones()
                .Select(z => new TimeZoneOption { Id = z.Id, DisplayName = z.DisplayName })
                .OrderBy(z => z.DisplayName)
                .ToList();
        }

        [HttpGet]
        public IActionResult Index()
        {
            var model = new TimeConverterViewModel
            {
                AvailableZones = GetZoneOptions(),
                FromZoneId = TimeZoneInfo.Local.Id,
                SourceTime = DateTime.Now
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult Index(TimeConverterViewModel model)
        {
            model.AvailableZones = GetZoneOptions();

            if (string.IsNullOrEmpty(model.FromZoneId) || string.IsNullOrEmpty(model.ToZoneId))
                return View(model);

            try
            {
                var fromZone = TimeZoneInfo.FindSystemTimeZoneById(model.FromZoneId);
                var toZone = TimeZoneInfo.FindSystemTimeZoneById(model.ToZoneId);

                var sourceTime = model.SourceTime ?? DateTime.Now;
                var sourceUtc = TimeZoneInfo.ConvertTimeToUtc(
                    DateTime.SpecifyKind(sourceTime, DateTimeKind.Unspecified), fromZone);

                model.ConvertedTime = TimeZoneInfo.ConvertTimeFromUtc(sourceUtc, toZone);
            }
            catch (TimeZoneNotFoundException)
            {
                ModelState.AddModelError("", "One of the selected time zones could not be found.");
            }

            return View(model);
        }
    }
}