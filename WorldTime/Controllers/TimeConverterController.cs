using Microsoft.AspNetCore.Mvc;
using WorldTime.Models;
using Microsoft.Extensions.Configuration;

namespace WorldTime.Controllers
{
    public class TimeConverterController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TimeConverterController> _logger;

        public TimeConverterController(IConfiguration configuration, ILogger<TimeConverterController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

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
            var defaultZoneId = _configuration["DefaultLocation"];

            if (string.IsNullOrEmpty(defaultZoneId) || !IsValidTimeZone(defaultZoneId))
            {
                _logger.LogWarning("DefaultLocation '{ConfiguredValue}' was missing or invalid on this OS. Falling back to {Fallback}.",
                    defaultZoneId, TimeZoneInfo.Local.Id);
                defaultZoneId = TimeZoneInfo.Local.Id;
            }
            else
            {
                _logger.LogInformation("Default time zone set to {DefaultZone} from configuration.", defaultZoneId);
            }

            var model = new TimeConverterViewModel
            {
                AvailableZones = GetZoneOptions(),
                FromZoneId = defaultZoneId,
                SourceTime = DateTime.Now,
            };

            return View(model);
        }

        private bool IsValidTimeZone(string id)
        {
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(id);
                return true;
            }
            catch (TimeZoneNotFoundException)
            {
                return false;
            }
        }

        [HttpPost]
        public IActionResult Index(TimeConverterViewModel model)
        {
            model.AvailableZones = GetZoneOptions();

            if (string.IsNullOrEmpty(model.FromZoneId) || string.IsNullOrEmpty(model.ToZoneId))
            {
                _logger.LogWarning("Conversion attempted with missing zone selection. FromZoneId='{From}', ToZoneId='{To}'",
                    model.FromZoneId, model.ToZoneId);
                return View(model);
            }

            try
            {
                var fromZone = TimeZoneInfo.FindSystemTimeZoneById(model.FromZoneId);
                var toZone = TimeZoneInfo.FindSystemTimeZoneById(model.ToZoneId);

                var sourceTime = model.SourceTime ?? DateTime.Now;
                var sourceUtc = TimeZoneInfo.ConvertTimeToUtc(
                    DateTime.SpecifyKind(sourceTime, DateTimeKind.Unspecified), fromZone);

                model.ConvertedTime = TimeZoneInfo.ConvertTimeFromUtc(sourceUtc, toZone);

                _logger.LogInformation("Converted {SourceTime} from {From} to {To} -> {Result}",
                    sourceTime, model.FromZoneId, model.ToZoneId, model.ConvertedTime);
            }
            catch (TimeZoneNotFoundException ex)
            {
                _logger.LogError(ex, "Time zone conversion failed for From='{From}' To='{To}'",
                    model.FromZoneId, model.ToZoneId);
                ModelState.AddModelError("", "One of the selected time zones could not be found.");
            }

            return View(model);
        }
    }
}