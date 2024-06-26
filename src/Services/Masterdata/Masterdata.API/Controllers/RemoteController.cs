﻿using Core.Responses;
using Masterdata.Application.Features.V1.Commands.BeginGame;
using Masterdata.Application.Features.V1.Queries.Question;
using Masterdata.Application.Features.V1.Queries.Remote;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Masterdata.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RemoteController : ControllerBase
    {
        private readonly IRemoteQuery _query;

        public RemoteController(IRemoteQuery query)
        {
            _query = query;
        }

        [HttpPost("list")]
        public async Task<IActionResult> GetListStatusRemote()
        {
            var result = await _query.GetStatusRemoteByRemoteName();
            return Ok(new ApiSuccessResponse
            {
                Data = result,
                Message = "Tạo thành công"
            });
        }

        [HttpGet("list-connect-remote")]
        public async Task<IActionResult> GetLstConnectedRemote()
        {
            var result = await _query.GetListConnectedRemote();
            return Ok(new ApiSuccessResponse
            {
                Data = result,
                Message = "Tạo thành công"
            });
        }
        [HttpPost("disconnect-remote")]
        public async Task<IActionResult> DisconnectRemote()
        {
            var result = await _query.DisconnectRemote();
            return Ok(new ApiSuccessResponse
            {
                Data = result,
                Message = result == true?"Ngắt kết nối thiết bị thành công":"Ngắt kết nối thiết bị thất bại!!!"
            });
        }

        [HttpPost("initial-remote")]
        public IActionResult InitialRemote()
        {
            var result = _query.InitialRemote();
            return Ok(new ApiSuccessResponse
            {
                Data = result,
                Message = result == true ? "Khởi tạo thiết bị thành công" : "Khởi tạo thiết bị thất bại!!!"
            });
        }
    }
}
