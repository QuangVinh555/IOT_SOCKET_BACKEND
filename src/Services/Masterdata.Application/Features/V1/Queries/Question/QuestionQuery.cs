﻿using Core.Exceptions;
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
            var topic = await _context.TopicModels.FirstOrDefaultAsync(x => x.TopicId == TopicId);
            if (topic == null) throw new BadRequestException("TopicId này không tồn tại!");

            return QuestionRes;
        }
    }
}