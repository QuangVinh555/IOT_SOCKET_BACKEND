using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masterdata.Application.Features.V1.DTOs.Question
{
    public class QuestionResponse
    {
        public Guid? TopicId { get; set; }
        public string TopicName { get; set; }
        public List<QuestionDataResponse> ListQuestionDatas { get; set; } = new();
    }

    public class QuestionDataResponse
    {
        public Guid? TopicId { get; set; }
        public Guid? QuestionId { get; set; }
        public int? QuestionTime { get; set; }
        public string QuestionName { get; set; }
        public string ImageUrl { get; set; }
        public string VideoUrl { get; set; }
        public List<AnswerResponse> ListAnswerDatas { get; set; } = new();
    }

    public class AnswerResponse
    {
        public Guid? AnswerId { get; set; }
        public string AnswerKey { get; set; }
        public string AnswerName { get; set; }
        public bool? IsCorrect { get; set; }
    }
}
