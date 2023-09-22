using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlatformService.Data;
using PlatformService.Dtos;
using PlatformService.Modles;
using PlatformService.SyncDataServices.Http;

namespace PlatformService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlatformsController : ControllerBase
    {
        private readonly IPlatformRepo _platformRepo;
        protected readonly IMapper _mapper;
        private readonly ICommandDataClient _commandDataClient;

        public PlatformsController(IPlatformRepo platformRepo, IMapper mapper, ICommandDataClient commandDataClient)
        {
            Console.WriteLine("Changes is ok");
            _platformRepo = platformRepo;
            _mapper = mapper;
            _commandDataClient = commandDataClient;
        }

        [HttpGet]
        public ActionResult<IEnumerable<PlatformReadDtos>> GetAllPlatforms()
        {
            Console.WriteLine("asdasdadas");
            Console.WriteLine("--> Getting Platforms");
            var platformItem = _platformRepo.GetAllPlatforms();
            return Ok(_mapper.Map<IEnumerable<PlatformReadDtos>>(platformItem));
        }

        [HttpGet("{Id}" , Name = "GetPlatformById")]
        public ActionResult<PlatformReadDtos> GetPlatformById(int Id)
        {
            Console.WriteLine($"--> Getting Platform by ID:{Id}");

            var platform = _platformRepo.GetPlatformById(Id);

            if (platform != null)
            {
                return Ok(_mapper.Map<PlatformReadDtos>(platform));
            }

            return NotFound();
        }
        [HttpPost]
        public async Task<ActionResult<PlatformCreateDtos>> CreatePlatformDto(PlatformCreateDtos platformCreateDtos)
        {
            var platform = _mapper.Map<Platform>(platformCreateDtos);
            _platformRepo.CreatePlatrom(platform);
            _platformRepo.SaveChanges();

            var platformReadDto = _mapper.Map<PlatformReadDtos>(platform);

            try 
            {
                await _commandDataClient.SendPlatformToCommand(platformReadDto);
            }
            catch (Exception e) 
            {
                Console.WriteLine($"---> Could not sync syncroniusly: {e.Message} ");
            }

            return CreatedAtRoute(nameof(GetPlatformById), new {id = platformReadDto.Id}, platformReadDto);
        }

    }
}