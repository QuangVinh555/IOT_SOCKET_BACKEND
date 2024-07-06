using Core.Common;
using Infrastructure.Data;
using Masterdata.Application.Features.V1.Commands.Topic;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Masterdata.Application.Features.V1.Commands.Question
{
    public class CreateQuestionCommand : IRequest<bool>
    {
        public List<CreateQuestion2Command> ListQuestions { get; set; } = new();
    }
    public class CreateQuestion2Command
    {
        public Guid? TopicId { get; set; }
        public string QuestionName { get; set; }
        public int? QuestionTime { get; set; }
        public string ImageUrl { get; set; }
        public string VideoUrl { get; set; }
        public List<CreateAnswerCommand> ListAnswerDatas { get; set; } = new();

    }
    public class CreateAnswerCommand
    {
        public string AnswerKey { get; set; }
        public string AnswerName { get; set; }
        public bool? IsCorrect { get; set; }
    }
    public class CreateQuestionCommandHandler : IRequestHandler<CreateQuestionCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOT_SOCKETContext _context;
        private readonly ICommonService _service;

        public CreateQuestionCommandHandler(IOT_SOCKETContext context, IUnitOfWork unitOfWork, ICommonService service)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _service = service;
        }

        public async Task<bool> Handle(CreateQuestionCommand request, CancellationToken cancellationToken)
        {
            List<AnswerModel> lstAnswerModel = new List<AnswerModel>();
            if(!request.ListQuestions.Any()) return true;

            foreach (var question in request.ListQuestions)
            {
                if(string.IsNullOrEmpty(question.QuestionName)) continue;

                // Add câu hỏi
                var questionEntity = new QuestionModel
                {
                    QuestionId = Guid.NewGuid(),
                    TopicId = question.TopicId,
                    QuestionName = question.QuestionName,
                    QuestionTime = question.QuestionTime,
                    CreateTime = DateTime.Now
                };

                if (!string.IsNullOrEmpty(question.ImageUrl))
                {
                    //Convert Base64 to Iformfile
                    byte[] bytes = Convert.FromBase64String(question.ImageUrl);
                    MemoryStream stream = new MemoryStream(bytes);

                    IFormFile file = new FormFile(stream, 0, bytes.Length, $"Upload_{questionEntity.QuestionId}", $"Upload_{questionEntity.QuestionId}.jpg");
                    //Save image to server
                    var imagePath = await _service.UploadFile(file, "Upload");

                    questionEntity.ImageUrl = imagePath;
                }
                if (!string.IsNullOrEmpty(question.VideoUrl)) {
                    //Convert Base64 to Iformfile
                    byte[] bytes = Convert.FromBase64String(question.VideoUrl);
                    MemoryStream stream = new MemoryStream(bytes);

                    IFormFile file = new FormFile(stream, 0, bytes.Length, $"Upload_{questionEntity.QuestionId}", $"Upload_{questionEntity.QuestionId}.mp4");
                    //Save image to server
                    var imagePath = await _service.UploadFile(file, "Upload");

                    questionEntity.VideoUrl = imagePath;
                }

                 await _context.QuestionModels.AddAsync(questionEntity);

                // Add đáp án
                if (!question.ListAnswerDatas.Any()) continue;

                foreach (var item in question.ListAnswerDatas)
                {
                    var answerEntity = new AnswerModel
                    {
                        AnswerId = Guid.NewGuid(),
                        QuestionId = questionEntity.QuestionId,
                        AnswerKey = item.AnswerKey,
                        AnswerName = item.AnswerName,
                        IsCorrect = item.IsCorrect,
                        CreateTime = DateTime.Now,
                    };
                    lstAnswerModel.Add(answerEntity);
                }
                await _context.AnswerModels.AddRangeAsync(lstAnswerModel);
            }

            await _unitOfWork.SaveChangesAsync();   

            return true;
        }
    }
}
