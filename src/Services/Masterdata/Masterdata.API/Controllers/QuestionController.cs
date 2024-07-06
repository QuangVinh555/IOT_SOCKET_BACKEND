using Core.Responses;
using Masterdata.Application.Features.V1.Commands.Question;
using Masterdata.Application.Features.V1.Queries.Question;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.IO;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Masterdata.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IQuestionQuery _query;

        public QuestionController(IMediator mediator, IQuestionQuery query)
        {
            _mediator = mediator;
            _query = query;
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateQuestion(CreateQuestionCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(new ApiSuccessResponse
            {
                Data = result,
                Message = "Tạo câu hỏi thành công"
            });
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateQuestion(UpdateQuestionCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(new ApiSuccessResponse
            {
                Data = result,
                Message = result == true ?"Cập nhật câu hỏi thành công":"Cập nhật câu hỏi thát bại"
            });
        }

        [HttpGet("list-question-by-topicId")]
        public async Task<IActionResult> GetListQuestionByTopicId([FromQuery] Guid TopicId)
        {
            var result = await _query.GetListQuestionByTopicId(TopicId);
            return Ok(new ApiSuccessResponse
            {
                Data = result,
                Message = "Get list câu hỏi thành công"
            });
        }

        [HttpGet("get-by-id")]
        public async Task<IActionResult> GetQuestionById([FromQuery] Guid QuestionId)
        {
            var result = await _query.GetQuestionById(QuestionId);
            return Ok(new ApiSuccessResponse
            {
                Data = result,
                Message = "Get câu hỏi thành công"
            });
        }
    }
}
