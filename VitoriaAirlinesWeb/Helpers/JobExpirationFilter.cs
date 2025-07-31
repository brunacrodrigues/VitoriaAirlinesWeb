using Hangfire.States;
using Hangfire.Storage;

namespace VitoriaAirlinesWeb.Helpers
{
    /// <summary>
    /// A Hangfire filter that sets an expiration time for jobs based on their final state.
    /// This helps in automatically cleaning up job records after completion, deletion, or failure.
    /// </summary>
    public class JobExpirationFilter : IApplyStateFilter
    {
        private readonly TimeSpan _expiration;

        /// <summary>
        /// Initializes a new instance of the JobExpirationFilter.
        /// </summary>
        /// <param name="expiration">The TimeSpan duration after which the job record should expire once it reaches a final state.</param>
        public JobExpirationFilter(TimeSpan expiration)
        {
            _expiration = expiration;
        }


        /// <summary>
        /// Called when a new state is applied to a background job.
        /// If the new state is Succeeded, Deleted, or Failed, the job is marked for expiration.
        /// </summary>
        /// <param name="context">The context for the state application.</param>
        /// <param name="transaction">The transaction to use for state manipulation.</param>
        public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            if (context.NewState is SucceededState || context.NewState is DeletedState || context.NewState is FailedState)
            {
                transaction.ExpireJob(context.BackgroundJob.Id, _expiration);
            }
        }


        /// <summary>
        /// Called when a state is unapplied from a background job.
        /// This method is empty as no action is required when a state is unapplied.
        /// </summary>
        /// <param name="context">The context for the state unapplication.</param>
        /// <param name="transaction">The transaction to use for state manipulation.</param>
        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction) { }
    }
}