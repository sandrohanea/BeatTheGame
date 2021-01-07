using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeatTheGame.Services
{
    public class CleanOldGamesJob : IJob
    {
        private readonly IGameSessionService gameSessionService;

        public CleanOldGamesJob(IGameSessionService gameSessionService)
        {
            this.gameSessionService = gameSessionService;
        }

        public Task Execute(IJobExecutionContext context)
        {
            gameSessionService.CleanOldSessions();
            return Task.CompletedTask;
        }
    }
}
