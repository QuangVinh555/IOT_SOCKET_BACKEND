using Core.Common;
using Core.Extensions;
using Infrastructure.Data;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Masterdata.Application.Features.V1.Commands.Question
{
    public class UpdateQuestionCommand : IRequest<bool>
    {
        public List<UpdateQuestion2Command> ListQuestions { get; set; } = new();
    }
    public class UpdateQuestion2Command
    {
        public Guid? QuestionId { get; set; }
        public Guid? TopicId { get; set; }
        public string QuestionName { get; set; }
        public int? QuestionTime { get; set; }
        public string ImageUrl { get; set; }
        public string VideoUrl { get; set; }
        public List<UpdateAnswerCommand> ListAnswerDatas { get; set; } = new();

    }
    public class UpdateAnswerCommand
    {
        public Guid? AnswerId { get; set; }
        public string AnswerKey { get; set; }
        public string AnswerName { get; set; }
        public bool? IsCorrect { get; set; }
    }
    public class UpdateQuestionCommandHandler : IRequestHandler<UpdateQuestionCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOT_SOCKETContext _context;
        private readonly ICommonService _service;

        public UpdateQuestionCommandHandler(IOT_SOCKETContext context, IUnitOfWork unitOfWork, ICommonService service)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _service = service;
        }

        public async Task<bool> Handle(UpdateQuestionCommand request, CancellationToken cancellationToken)
        {
            if (!request.ListQuestions.Any()) return false;
            var existPath = Path.Combine(new ConfigManager().DocumentDomainUploadRes);
            foreach (var question in request.ListQuestions)
            {
                string urlImage = String.Empty;
                string urlVideo = String.Empty;
                if (string.IsNullOrEmpty(question.QuestionName)) return false;

                if (!string.IsNullOrEmpty(question.ImageUrl) && !question.ImageUrl.Contains(existPath))
                {
                    //Convert Base64 to Iformfile
                    byte[] bytes = Convert.FromBase64String(question.ImageUrl);
                    MemoryStream stream = new MemoryStream(bytes);

                    IFormFile file = new FormFile(stream, 0, bytes.Length, $"Upload_{question.QuestionId}", $"Upload_{question.QuestionId}.jpg");
                    //Save image to server
                    var imagePath = await _service.UploadFile(file, "Upload");

                    urlImage = imagePath;
                }
                if (!string.IsNullOrEmpty(question.VideoUrl) && !question.VideoUrl.Contains(existPath))
                {
                    //Convert Base64 to Iformfile
                    byte[] bytes = Convert.FromBase64String(question.VideoUrl);
                    MemoryStream stream = new MemoryStream(bytes);

                    IFormFile file = new FormFile(stream, 0, bytes.Length, $"Upload_{question.QuestionId}", $"Upload_{question.QuestionId}.mp4");
                    //Save image to server
                    var imagePath = await _service.UploadFile(file, "Upload");

                    urlVideo = imagePath;
                }

                var QuestionByTopic = await _context.QuestionModels.Where(x => x.TopicId == question.TopicId && x.QuestionId == question.QuestionId).FirstOrDefaultAsync();

                if (QuestionByTopic == null) return false;
                if(!String.IsNullOrEmpty(urlImage))
                    QuestionByTopic.ImageUrl = urlImage;
                if (!String.IsNullOrEmpty(urlVideo))
                    QuestionByTopic.VideoUrl = urlVideo;
                QuestionByTopic.QuestionName = question.QuestionName;
                QuestionByTopic.QuestionTime = question.QuestionTime;
                QuestionByTopic.CreateTime = DateTime.Now;
                _context.QuestionModels.Update(QuestionByTopic);
                if (!question.ListAnswerDatas.Any()) return false;

                var listAnswer = await _context.AnswerModels.Where(x => x.QuestionId == question.QuestionId).ToListAsync();
                if (listAnswer == null || listAnswer.Count == 0) return false;

                foreach(var item in listAnswer)
                {
                    var existRequestAnswer = question.ListAnswerDatas.Where(x => x.AnswerId == item.AnswerId).FirstOrDefault();
                    if (existRequestAnswer == null) continue;
                    item.AnswerKey = existRequestAnswer.AnswerKey;
                    item.AnswerName = existRequestAnswer.AnswerName;
                    item.IsCorrect = existRequestAnswer.IsCorrect;
                    item.CreateTime = DateTime.Now;
                }
                _context.AnswerModels.UpdateRange(listAnswer);
            }
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
