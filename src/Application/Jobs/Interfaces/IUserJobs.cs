namespace backend.src.Application.Jobs.Interfaces
{
    /// <summary>
    /// Interface defining user-related background jobs.
    /// </summary>
    public interface IUserJobs
    {
        /// <summary>
        /// Deletes user accounts that have not been confirmed within a certain time frame.
        /// </summary>
        Task DeleteUnconfirmedUserAccountsAsync();
    }
}
