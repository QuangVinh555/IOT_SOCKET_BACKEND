using Core.Exceptions;
using Core.Extensions;
using Infrastructure.Data;
using Masterdata.Application.Features.V1.DTOs.Question;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masterdata.Application.Features.V1.Queries.Question
{
    public interface IQuestionQuery
    {
        public Task<QuestionResponse> GetListQuestionByTopicId(Guid TopicId);
        public Task<QuestionDataResponse> GetQuestionById(Guid QuestionId);
    }
    public class QuestionQuery : IQuestionQuery
    {
        private readonly IOT_SOCKETContext _context;

        public QuestionQuery(IOT_SOCKETContext context) {
            _context = context;
        }
        public async Task<QuestionResponse> GetListQuestionByTopicId(Guid TopicId)
        {
            var QuestionRes = new QuestionResponse();

            var config = new ConfigManager();

            var topic = await _context.TopicModels.FirstOrDefaultAsync(x => x.TopicId == TopicId);
            if (topic == null) throw new BadRequestException("TopicId này không tồn tại!");

            // Response Topic
            QuestionRes.TopicId = TopicId;
            QuestionRes.TopicName = topic.TopicName;

            var listQuestions = await _context.QuestionModels.Where(x => x.TopicId == TopicId)
                .Include(x => x.AnswerModels)
                .OrderBy(x => x.CreateTime)
                .ToListAsync();

            // Response ListQuestion
            var ListQuestionDataRes = new List<QuestionDataResponse>();
            foreach (var question in listQuestions)
            {
                var QuestionDataRes = new QuestionDataResponse
                {
                    TopicId = TopicId,
                    QuestionId = question.QuestionId,
                    QuestionName = question.QuestionName,
                    QuestionTime = question.QuestionTime,
                    ImageUrl = !string.IsNullOrEmpty(question.ImageUrl) ? $"{config.DocumentDomainUploadRes}{question.ImageUrl}" : null,
                    VideoUrl = !string.IsNullOrEmpty(question.VideoUrl) ? $"{config.DocumentDomainUploadRes}{question.VideoUrl}" : null,
                };

                // Response List Answer
                var ListAnswerRes = new List<AnswerResponse>();

                var answerInfo = question.AnswerModels.Where(x => x.QuestionId == question.QuestionId).ToList();
                foreach (var item in answerInfo)
                {
                    var answerRes = new AnswerResponse
                    {
                        AnswerKey = item.AnswerKey,
                        AnswerId = item.AnswerId,
                        AnswerName = item.AnswerName,
                        IsCorrect = item.IsCorrect,
                    };

                    ListAnswerRes.Add(answerRes);
                }
                // Response List Answer in Question
                QuestionDataRes.ListAnswerDatas = ListAnswerRes.OrderBy(x=>x.AnswerKey).ToList();
                // Response List Question in Topic
                ListQuestionDataRes.Add(QuestionDataRes);
            };

            QuestionRes.ListQuestionDatas = ListQuestionDataRes;

            return QuestionRes;
        }

        public async Task<QuestionDataResponse> GetQuestionById(Guid QuestionId)
        {
            var config = new ConfigManager();

            var question = await _context.QuestionModels.FirstOrDefaultAsync(x => x.QuestionId == QuestionId);
            if (question == null) throw new BadRequestException("Id câu hỏi này không tồn tại");

            var QuestionDataRes = new QuestionDataResponse
            {
                QuestionId = question.QuestionId,
                QuestionName = question.QuestionName,
                QuestionTime = question.QuestionTime,
                ImageUrl = !string.IsNullOrEmpty(question.ImageUrl) ? $"{config.DocumentDomainUploadRes}{question.ImageUrl}" : null,
                VideoUrl = !string.IsNullOrEmpty(question.VideoUrl) ? $"{config.DocumentDomainUploadRes}{question.VideoUrl}" : null,
            };

            var listAnswer = await _context.AnswerModels.Where(x => x.QuestionId == question.QuestionId)
                .Select(x => new AnswerResponse
                {
                    AnswerId = x.AnswerId,
                    AnswerName = x.AnswerName,
                    IsCorrect = x.IsCorrect,
                })
                .ToListAsync();

            QuestionDataRes.ListAnswerDatas = listAnswer;

            return QuestionDataRes;

        }
    }
}
