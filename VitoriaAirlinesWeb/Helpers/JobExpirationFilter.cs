using Hangfire.States;
using Hangfire.Storage;

namespace VitoriaAirlinesWeb.Helpers
{
    public class JobExpirationFilter : IApplyStateFilter
    {
        private readonly TimeSpan _expiration;

        public JobExpirationFilter(TimeSpan expiration)
        {
            _expiration = expiration;
        }

        public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            if (context.NewState is SucceededState || context.NewState is DeletedState || context.NewState is FailedState)
            {
                transaction.ExpireJob(context.BackgroundJob.Id, _expiration);
            }
        }

        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction) { }
    }
}
