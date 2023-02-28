using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace HighAvailabilitySandbox.Controllers;

[ApiController]
[Route("/")]
public class StatusController : ControllerBase
{
    private class Statuses
    {
        public const string Init = "INIT";
        public const string Active = "ACTIVE";
        public const string StandBy = "STANDBY";
    }
    
    private readonly Guid _id = Guid.NewGuid();
    private static string _status = Statuses.Init;
    
    private readonly ILogger<StatusController> _logger;

    public StatusController(ILogger<StatusController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    [Route("/")]
    public string GetId()
    {
        return $"{_status} - {_id} - {GetLocalIpAddress()}";
    }

    [HttpGet]
    [Route("/status")]
    public IActionResult GetStatus()
    {
        Response.BodyWriter.Write(Encoding.UTF8.GetBytes(_status));
        if (_status == Statuses.StandBy)
        {
            return new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);
        }

        return Ok();
    }
    
    [HttpGet]
    [Route("/set")]
    public IActionResult Set(string status)
    {
        if (status is Statuses.Init or Statuses.Active or Statuses.StandBy)
        {
            _status = status;
            return Ok();
        }
    
        return BadRequest($"Invalid status '{status}' provided. Valid options are: {Statuses.Active} or {Statuses.StandBy}");
    }
    
    private static string GetLocalIpAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        return "ERROR: No network adapters with an IPv4 address in the system!";
    }
}
