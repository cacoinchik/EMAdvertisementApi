using EMAdvertisementApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Immutable;

namespace EMAdvertisementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdvertisementController : ControllerBase
    {
        private readonly AdvertisementData data;
        public AdvertisementController(AdvertisementData data) => this.data = data;

        /// <summary>
        /// Метод загрузки рекламных площадок
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file is null)
                return BadRequest("Файл не загружен");

            if (file.Length == 0)
                return BadRequest("Файл пуст");

            if (!string.Equals(Path.GetExtension(file.FileName), ".txt", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Неверный формат файла");

            string? line;
            int countLines = 0, validLines = 0, invalidLines = 0;
            var warnings = new List<string>();
            var parsed = new List<(string Name, List<string> Locations)>();

            using var reader = new StreamReader(file.OpenReadStream());

            while ((line = await reader.ReadLineAsync()) is not null)
            {
                countLines++;

                if (string.IsNullOrWhiteSpace(line))
                {
                    invalidLines++;
                    continue;
                }

                if (!TryParseLine(line, out string name, out List<string> locations, out string errorMessage))
                {
                    invalidLines++;
                    var warning = $"Ошибка в строке номер {countLines}. Описание: {errorMessage}";
                    warnings.Add(warning);
                    continue;
                }

                validLines++;
                parsed.Add((name, locations));
            }

            var advDic = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);

            foreach ((string name, List<string> locations) in parsed)
            {
                foreach (var loc in locations)
                {
                    if (!advDic.TryGetValue(loc, out var set))
                        advDic[loc] = set = new HashSet<string>(StringComparer.Ordinal);
                    set.Add(name);
                }
            }

            var immutable = advDic.ToImmutableDictionary(
                kv => kv.Key,
                kv => kv.Value.ToImmutableHashSet(StringComparer.Ordinal),
                StringComparer.Ordinal
            );

            //Перезаписываем всю информацию
            data.Replace(immutable);

            return Ok(new { message = "Файл обработан.", countLines, validLines, invalidLines, warnings });
        }

        /// <summary>
        /// Метод поиска рекламных площадок для заданной локации
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetAdvertisement([FromQuery] string location)
        {
            if (string.IsNullOrWhiteSpace(location))
                return BadRequest("Укажите локацию");

            if (!IsValidLocation(location))
                return BadRequest("Неверный формат локации. Локация должна начинаться с '/' и не содержать пробелов.");

            var result = data.FindAdvertisementByLocation(location);

            return Ok(new { location, result });
        }

        //Парсер
        private bool TryParseLine(string line, out string name, out List<string> locations, out string errorMessage)
        {
            name = "";
            locations = new List<string>();
            errorMessage = "";

            line = line.Trim();

            var colonIndex = line.IndexOf(':');
            if (colonIndex <= 0)
            {
                errorMessage = "Нет начала указания локаций в виде ':'";
                return false;
            }
            else if (colonIndex == line.Length - 1)
            {
                errorMessage = "Отсутсвуют локации";
                return false;
            }

            var parsedName = line[..colonIndex].Trim();
            if (string.IsNullOrWhiteSpace(parsedName))
            {
                errorMessage = "Отсутсвует название рекламной площадки";
                return false;
            }

            var allLocations = line[(colonIndex + 1)..].Trim().Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var loc in allLocations)
            {
                if (!IsValidLocation(loc))
                    continue;
                locations.Add(loc);
            }

            if (locations.Count == 0)
            {
                errorMessage = "Список валидных локаций для данной рекламной площадки пуст";
                return false;
            }

            name = parsedName;
            return true;
        }

        //Проверка валидности локации
        private bool IsValidLocation(string loc)
        {
            if (string.IsNullOrWhiteSpace(loc) || !loc.StartsWith("/"))
                return false;

            if (loc.Contains(' ') || loc.Contains(',') || loc.Contains("//"))
                return false;

            return true;
        }
    }
}
